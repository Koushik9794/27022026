import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// NetworkStack
//
// Creates:
//   • VPC with 2 public + 2 private subnets across 2 AZs
//   • 1 NAT Gateway (cost-optimised — single NAT for non-prod)
//   • ALB Security Group  → accepts HTTP :80 from the internet
//   • ECS Security Group  → accepts :80 from ALB only
//   • RDS Security Group  → accepts :5432 from ECS only
// ─────────────────────────────────────────────────────────────────────────────

export interface NetworkStackProps extends cdk.StackProps {
    tags: Record<string, string>;
}

export class NetworkStack extends cdk.Stack {

    public readonly vpc: ec2.Vpc;
    public readonly albSecurityGroup: ec2.SecurityGroup;
    public readonly ecsSecurityGroup: ec2.SecurityGroup;
    public readonly rdsSecurityGroup: ec2.SecurityGroup;

    constructor(scope: Construct, id: string, props: NetworkStackProps) {
        super(scope, id, props);

        // ── VPC ────────────────────────────────────────────────────────────────────
        this.vpc = new ec2.Vpc(this, 'GssVpc', {
            vpcName: 'gss-vpc',
            maxAzs: 2,
            natGateways: 1,           // one NAT gateway — sufficient for deployment

            subnetConfiguration: [
                {
                    cidrMask: 24,
                    name: 'Public',
                    subnetType: ec2.SubnetType.PUBLIC,
                },
                {
                    cidrMask: 24,
                    name: 'Private',
                    subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS,
                },
            ],

            // Enable DNS so RDS endpoint resolves inside VPC
            enableDnsHostnames: true,
            enableDnsSupport: true,
        });

        // ── ALB Security Group ─────────────────────────────────────────────────────
        // Internet-facing — accepts HTTP :80 from anywhere
        this.albSecurityGroup = new ec2.SecurityGroup(this, 'AlbSg', {
            vpc: this.vpc,
            securityGroupName: 'gss-alb-sg',
            description: 'ALB: allow inbound HTTP from internet',
            allowAllOutbound: true,
        });
        this.albSecurityGroup.addIngressRule(
            ec2.Peer.anyIpv4(),
            ec2.Port.tcp(80),
            'Allow HTTP from internet',
        );

        // ── ECS Security Group ─────────────────────────────────────────────────────
        // Fargate tasks — only accepts traffic from ALB on container port :80
        this.ecsSecurityGroup = new ec2.SecurityGroup(this, 'EcsSg', {
            vpc: this.vpc,
            securityGroupName: 'gss-ecs-sg',
            description: 'ECS Fargate tasks: allow from ALB only',
            allowAllOutbound: true,
        });
        this.ecsSecurityGroup.addIngressRule(
            ec2.Peer.securityGroupId(this.albSecurityGroup.securityGroupId),
            ec2.Port.tcp(80),
            'Allow traffic from ALB',
        );

        // ── RDS Security Group ─────────────────────────────────────────────────────
        // RDS — only accepts :5432 from ECS tasks
        this.rdsSecurityGroup = new ec2.SecurityGroup(this, 'RdsSg', {
            vpc: this.vpc,
            securityGroupName: 'gss-rds-sg',
            description: 'RDS: allow from ECS Fargate only',
            allowAllOutbound: false,
        });
        this.rdsSecurityGroup.addIngressRule(
            ec2.Peer.securityGroupId(this.ecsSecurityGroup.securityGroupId),
            ec2.Port.tcp(5432),
            'Allow PostgreSQL from ECS tasks',
        );

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'VpcId', {
            value: this.vpc.vpcId,
            description: 'VPC ID',
            exportName: 'GssVpcId',
        });

        new cdk.CfnOutput(this, 'AlbSgId', {
            value: this.albSecurityGroup.securityGroupId,
            exportName: 'GssAlbSgId',
        });

        new cdk.CfnOutput(this, 'EcsSgId', {
            value: this.ecsSecurityGroup.securityGroupId,
            exportName: 'GssEcsSgId',
        });
    }
}
