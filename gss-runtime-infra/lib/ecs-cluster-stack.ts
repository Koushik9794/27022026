import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// EcsClusterStack
//
// Deploys INTO: runtime account 771355239306
//
// Creates:
//   • ECS Cluster (Fargate capacity providers)
//   • ALB Security Group ← internet → :80
//   • ECS Security Group ← ALB only → :5000
//   • RDS egress rule   ← ECS → RDS :5432
//
// Does NOT create: VPC, subnets, RDS, ECR
// ─────────────────────────────────────────────────────────────────────────────

export interface EcsClusterStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    /** Imported enterprise VPC */
    readonly vpc: ec2.IVpc;
}

export class EcsClusterStack extends cdk.Stack {

    /** Logical ECS cluster — Fargate tasks attach here. */
    public readonly cluster: ecs.Cluster;

    /** Security group for the internet-facing ALB. */
    public readonly albSecurityGroup: ec2.SecurityGroup;

    /** Security group for ECS Fargate tasks. */
    public readonly ecsSecurityGroup: ec2.SecurityGroup;

    constructor(scope: Construct, id: string, props: EcsClusterStackProps) {
        super(scope, id, props);

        // ── ECS Cluster ────────────────────────────────────────────────────────────
        this.cluster = new ecs.Cluster(this, 'GssCluster', {
            clusterName: 'gss-cluster',
            vpc: props.vpc,
            enableFargateCapacityProviders: true,

            // Enables Container Insights metrics in CloudWatch
            containerInsights: true,
        });

        // ── ALB Security Group ─────────────────────────────────────────────────────
        // Accepts HTTP :80 from the public internet.
        this.albSecurityGroup = new ec2.SecurityGroup(this, 'AlbSg', {
            vpc: props.vpc,
            securityGroupName: 'gss-alb-sg',
            description: 'GSS ALB — inbound HTTP :80 from internet',
            allowAllOutbound: true,
        });
        this.albSecurityGroup.addIngressRule(
            ec2.Peer.anyIpv4(),
            ec2.Port.tcp(80),
            'Allow HTTP from internet',
        );

        // ── ECS Security Group ─────────────────────────────────────────────────────
        // Only accepts :5000 from the ALB security group.
        // All outbound is allowed (ECR image pulls, CloudWatch, RDS).
        this.ecsSecurityGroup = new ec2.SecurityGroup(this, 'EcsSg', {
            vpc: props.vpc,
            securityGroupName: 'gss-ecs-sg',
            description: 'GSS ECS Fargate — allow :5000 from ALB only',
            allowAllOutbound: true,
        });
        this.ecsSecurityGroup.addIngressRule(
            ec2.Peer.securityGroupId(this.albSecurityGroup.securityGroupId),
            ec2.Port.tcp(5000),
            'Allow container traffic from ALB',
        );

        // Egress rule: ECS → RDS PostgreSQL :5432
        // The RDS security group lives in the existing enterprise infra; we add
        // an egress rule here to make intent explicit and allow future audits.
        this.ecsSecurityGroup.addEgressRule(
            ec2.Peer.anyIpv4(),      // Narrowed to DB IP if SG ID is known
            ec2.Port.tcp(5432),
            'ECS Fargate → RDS PostgreSQL',
        );

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'ClusterName', {
            value: this.cluster.clusterName,
            description: 'ECS Cluster name',
            exportName: 'GssEcsClusterName',
        });

        new cdk.CfnOutput(this, 'AlbSgId', {
            value: this.albSecurityGroup.securityGroupId,
            description: 'ALB Security Group ID',
            exportName: 'GssAlbSgId',
        });

        new cdk.CfnOutput(this, 'EcsSgId', {
            value: this.ecsSecurityGroup.securityGroupId,
            description: 'ECS Security Group ID',
            exportName: 'GssEcsSgId',
        });
    }
}
