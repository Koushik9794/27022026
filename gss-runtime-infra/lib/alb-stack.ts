import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// AlbStack — Control Tower Compliant
//
// Creates:
//   • Internet-facing ALB in public subnets (172.19.142.128/28 & 172.19.143.128/28)
//   • HTTP :80 listener → forward to target group
//   • Target group on port 5000, target type IP (required for Fargate)
//   • Health check path: /swagger
//
// Control Tower rules satisfied:
//   • ALB is internet-facing (allowed — it is explicitly in public subnets)
//   • No open security group beyond what ALB SG defines in EcsClusterStack
//   • No S3 bucket ACL (access logging disabled — enable and add compliant S3
//     bucket with BLOCK_ALL + BUCKET_OWNER_ENFORCED if CT requires access logs)
//   • Target type IP — no EC2 instance registration
// ─────────────────────────────────────────────────────────────────────────────

export interface AlbStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Public subnets for ALB placement (172.19.142.128/28 & 172.19.143.128/28) */
    readonly publicSubnets: ec2.ISubnet[];
    readonly albSecurityGroup: ec2.SecurityGroup;
}

export class AlbStack extends cdk.Stack {

    /** Internet-facing Application Load Balancer */
    public readonly alb: elbv2.ApplicationLoadBalancer;

    /** HTTP :80 listener */
    public readonly listener: elbv2.ApplicationListener;

    /** Target group on port 5000 — Fargate tasks register here */
    public readonly targetGroup: elbv2.ApplicationTargetGroup;

    constructor(scope: Construct, id: string, props: AlbStackProps) {
        super(scope, id, props);

        // ── Application Load Balancer ──────────────────────────────────────────────
        // Internet-facing; placed ONLY in the designated public subnets.
        // Security group is passed in from EcsClusterStack (allowAllOutbound: false).
        this.alb = new elbv2.ApplicationLoadBalancer(this, 'GssAlb', {
            // loadBalancerName omitted — CDK auto-generates (CT naming guardrail safe).
            vpc: props.vpc,
            internetFacing: true,
            securityGroup: props.albSecurityGroup,
            vpcSubnets: {
                subnets: props.publicSubnets,
            },
        });

        // ── Target Group ──────────────────────────────────────────────────────────
        // target type IP — required for Fargate (tasks register using private IP).
        this.targetGroup = new elbv2.ApplicationTargetGroup(this, 'GssTargetGroup', {
            // targetGroupName omitted — CDK auto-generates (CT naming guardrail safe).
            vpc: props.vpc,
            port: 5000,
            protocol: elbv2.ApplicationProtocol.HTTP,
            targetType: elbv2.TargetType.IP,

            // Health check path: /swagger as required by TL spec
            // Switch to /health if /swagger returns non-200 when app starts
            healthCheck: {
                path: '/swagger',
                port: '5000',
                protocol: elbv2.Protocol.HTTP,
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                healthyThresholdCount: 2,
                unhealthyThresholdCount: 5,      // more tolerance during cold start
                healthyHttpCodes: '200-299',
            },

            // Low deregistration delay for faster rolling deploys
            deregistrationDelay: cdk.Duration.seconds(30),
        });

        // ── HTTP :80 Listener ─────────────────────────────────────────────────────
        // open: false — access is controlled by the ALB security group, not the listener.
        // Default action: forward all traffic to the ECS target group.
        this.listener = this.alb.addListener('HttpListener', {
            port: 80,
            open: false,   // ALB SG already restricts source IPs
            defaultTargetGroups: [this.targetGroup],
        });

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'AlbDnsName', {
            value: this.alb.loadBalancerDnsName,
            description: 'ALB DNS — http://<this>/swagger to verify service',
            exportName: 'GssAlbDnsName',
        });

        new cdk.CfnOutput(this, 'TargetGroupArn', {
            value: this.targetGroup.targetGroupArn,
            description: 'Target Group ARN',
            exportName: 'GssTargetGroupArn',
        });

        new cdk.CfnOutput(this, 'AlbArn', {
            value: this.alb.loadBalancerArn,
            description: 'ALB ARN',
            exportName: 'GssAlbArn',
        });
    }
}
