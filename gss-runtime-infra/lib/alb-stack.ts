import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// AlbStack
//
// Deploys INTO: runtime account 771355239306
//
// Creates:
//   • Internet-facing Application Load Balancer in public subnets
//   • HTTP :80 listener
//   • Target group on port 5000 with /health check
//
// Does NOT create: VPC, subnets, ECS, RDS
// ─────────────────────────────────────────────────────────────────────────────

export interface AlbStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Public subnets where the ALB is placed (172.19.142.128/28 & 172.19.143.128/28) */
    readonly publicSubnets: ec2.ISubnet[];
    readonly albSecurityGroup: ec2.SecurityGroup;
}

export class AlbStack extends cdk.Stack {

    /** The internet-facing ALB */
    public readonly alb: elbv2.ApplicationLoadBalancer;

    /** HTTP :80 listener — ECS registers itself as a target */
    public readonly listener: elbv2.ApplicationListener;

    /** Target group listening on port 5000 */
    public readonly targetGroup: elbv2.ApplicationTargetGroup;

    constructor(scope: Construct, id: string, props: AlbStackProps) {
        super(scope, id, props);

        // ── Application Load Balancer ──────────────────────────────────────────────
        this.alb = new elbv2.ApplicationLoadBalancer(this, 'GssAlb', {
            loadBalancerName: 'gss-admin-alb',
            vpc: props.vpc,
            internetFacing: true,
            securityGroup: props.albSecurityGroup,

            // Place ALB in the two public subnets from the enterprise network
            vpcSubnets: {
                subnets: props.publicSubnets,
            },
        });

        // ── Target Group ──────────────────────────────────────────────────────────
        // ip target type is required for Fargate (tasks register by private IP).
        this.targetGroup = new elbv2.ApplicationTargetGroup(this, 'GssTargetGroup', {
            targetGroupName: 'gss-admin-tg',
            vpc: props.vpc,

            // Container port 5000 as required
            port: 5000,
            protocol: elbv2.ApplicationProtocol.HTTP,
            targetType: elbv2.TargetType.IP,

            // Health check — /health endpoint on the container
            healthCheck: {
                path: '/health',
                port: '5000',
                protocol: elbv2.Protocol.HTTP,
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                healthyThresholdCount: 2,
                unhealthyThresholdCount: 3,
                healthyHttpCodes: '200',
            },

            // Faster deregistration for rolling deployments
            deregistrationDelay: cdk.Duration.seconds(30),
        });

        // ── HTTP :80 Listener ─────────────────────────────────────────────────────
        this.listener = this.alb.addListener('HttpListener', {
            port: 80,
            open: false,   // ALB SG controls access — do not open listener to world independently
            defaultTargetGroups: [this.targetGroup],
        });

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'AlbDnsName', {
            value: this.alb.loadBalancerDnsName,
            description: 'ALB DNS — service is reachable at http://<this>/health',
            exportName: 'GssAlbDnsName',
        });

        new cdk.CfnOutput(this, 'TargetGroupArn', {
            value: this.targetGroup.targetGroupArn,
            description: 'Target Group ARN',
            exportName: 'GssTargetGroupArn',
        });
    }
}
