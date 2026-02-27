import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// EcsClusterStack — ECS Cluster Only
//
// Creates:
//   • ECS Cluster with Fargate capacity providers
//
// Does NOT create:
//   ✗ Security groups  (owned by NetworkImportStack)
//   ✗ VPC / subnets    (imported by NetworkImportStack)
//   ✗ ALB / listeners  (owned by AlbStack)
//
// Security groups are passed in from NetworkImportStack and forwarded to
// AlbStack and ServiceStack via the cluster stack's public properties.
// ─────────────────────────────────────────────────────────────────────────────

export interface EcsClusterStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    /** Imported enterprise VPC */
    readonly vpc: ec2.IVpc;
    /** ALB SG — created by NetworkImportStack, forwarded to AlbStack */
    readonly albSecurityGroup: ec2.SecurityGroup;
    /** ECS SG — created by NetworkImportStack, forwarded to ServiceStack */
    readonly ecsSecurityGroup: ec2.SecurityGroup;
}

export class EcsClusterStack extends cdk.Stack {

    /** ECS Cluster — Fargate tasks attach here */
    public readonly cluster: ecs.Cluster;

    /** Forwarded ALB Security Group (from NetworkImportStack → AlbStack) */
    public readonly albSecurityGroup: ec2.SecurityGroup;

    /** Forwarded ECS Security Group (from NetworkImportStack → ServiceStack) */
    public readonly ecsSecurityGroup: ec2.SecurityGroup;

    constructor(scope: Construct, id: string, props: EcsClusterStackProps) {
        super(scope, id, props);

        // ── Forward security groups ────────────────────────────────────────────────
        // SGs are created in NetworkImportStack and passed through here so that
        // downstream stacks (AlbStack, ServiceStack) only need a single dependency
        // on the cluster stack rather than two separate stacks.
        this.albSecurityGroup = props.albSecurityGroup;
        this.ecsSecurityGroup = props.ecsSecurityGroup;

        // ── ECS Cluster ────────────────────────────────────────────────────────────
        // clusterName omitted — CDK auto-generates a unique name (CT naming guardrail safe).
        // containerInsights: enabled for CloudWatch metrics visibility.
        this.cluster = new ecs.Cluster(this, 'GssCluster', {
            vpc: props.vpc,
            enableFargateCapacityProviders: true,
            containerInsights: true,
        });

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'ClusterName', {
            value: this.cluster.clusterName,
            description: 'ECS Cluster name — used by aws ecs update-service for redeployments',
            exportName: 'GssEcsClusterName',
        });

        new cdk.CfnOutput(this, 'ClusterArn', {
            value: this.cluster.clusterArn,
            description: 'ECS Cluster ARN',
            exportName: 'GssEcsClusterArn',
        });
    }
}
