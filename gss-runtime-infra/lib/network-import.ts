import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// NetworkStack — Static VPC Import + Security Groups
//
// What this stack does:
//   1. Imports existing enterprise VPC (no API calls — static IDs only)
//   2. Imports existing subnets by known IDs (no API calls)
//   3. Imports existing RDS Security Group (no new SG created)
//   4. Creates ALB Security Group
//   5. Creates ECS Security Group (egress scoped to imported RDS SG)
//
// What this stack does NOT do:
//   ✗ Does NOT create a VPC
//   ✗ Does NOT create subnets
//   ✗ Does NOT create NAT gateways
//   ✗ Does NOT create an RDS security group
//   ✗ Does NOT call any AWS APIs during cdk synth
//
// Control Tower compliance:
//   • No new networking resources created beyond ALB SG + ECS SG
//   • All created SGs use allowAllOutbound: false (scoped egress)
//   • No explicit securityGroupName (avoids CT naming guardrail)
//   • Vpc.fromVpcAttributes = zero EC2 API calls at synth time
//   • Works in cross-account CodeBuild without runtime account credentials
//
// ⚠️  REPLACE all placeholder IDs below with real values.
//     AWS Console → VPC → Subnets → filter by vpc-0146f76f9e738f1e3f
//     AWS Console → VPC → Security Groups → find the RDS SG
// ─────────────────────────────────────────────────────────────────────────────

// ── Known static network IDs ──────────────────────────────────────────────────
const VPC_ID = 'vpc-0146f76f9e738f1e3f';
const AZ_1A = 'ap-south-1a';
const AZ_1B = 'ap-south-1b';

// Public subnets — ALB only (172.19.142.128/28 & 172.19.143.128/28)
const SUBNET_PUBLIC_1A = 'subnet-public-1a';   // ← replace with real subnet ID
const SUBNET_PUBLIC_1B = 'subnet-public-1b';   // ← replace with real subnet ID

// Private App subnets — ECS Fargate tasks (172.19.142.0/25 & 172.19.143.0/25)
const SUBNET_APP_1A = 'subnet-app-1a';         // ← replace with real subnet ID
const SUBNET_APP_1B = 'subnet-app-1b';         // ← replace with real subnet ID

// Private DB subnets — referenced in VPC attributes only (not used for egress rules)
const SUBNET_DB_1A = 'subnet-db-1a';           // ← replace with real subnet ID
const SUBNET_DB_1B = 'subnet-db-1b';           // ← replace with real subnet ID

// Existing RDS Security Group — import only, do NOT create a new one.
// Find it: AWS Console → VPC → Security Groups → filter by vpc-0146f76f9e738f1e3f
//          or: aws ec2 describe-security-groups --filters Name=vpc-id,Values=vpc-0146f76f9e738f1e3f
const RDS_SG_ID = 'sg-rds-existing';           // ← replace with real RDS SG ID (sg-xxxxxxxx)

// ─────────────────────────────────────────────────────────────────────────────

export interface NetworkStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
}

export class NetworkImportStack extends cdk.Stack {

    /** Imported enterprise VPC — all constructs attach here */
    public readonly vpc: ec2.IVpc;

    /** Public subnets — ALB placement only */
    public readonly publicSubnets: ec2.ISubnet[];

    /** Private app subnets — ECS Fargate tasks (assignPublicIp: false) */
    public readonly privateAppSubnets: ec2.ISubnet[];

    /** ALB Security Group — HTTP/HTTPS inbound, scoped egress to ECS only */
    public readonly albSecurityGroup: ec2.SecurityGroup;

    /** ECS Security Group — inbound from ALB only, scoped egress to RDS SG + AWS services */
    public readonly ecsSecurityGroup: ec2.SecurityGroup;

    /** IMPORTED existing RDS Security Group — not created by CDK */
    public readonly rdsSecurityGroup: ec2.ISecurityGroup;

    constructor(scope: Construct, id: string, props: NetworkStackProps) {
        super(scope, id, props);

        // ── Static VPC Import ─────────────────────────────────────────────────────
        // Vpc.fromVpcAttributes generates pure CloudFormation literal references.
        // Zero EC2 API calls — works in cross-account CodeBuild pipelines.
        this.vpc = ec2.Vpc.fromVpcAttributes(this, 'EnterpriseVpc', {
            vpcId: VPC_ID,
            availabilityZones: [AZ_1A, AZ_1B],
            publicSubnetIds: [SUBNET_PUBLIC_1A, SUBNET_PUBLIC_1B],
            privateSubnetIds: [SUBNET_APP_1A, SUBNET_APP_1B],
            isolatedSubnetIds: [SUBNET_DB_1A, SUBNET_DB_1B],
        });

        // ── Static Subnet References ──────────────────────────────────────────────
        // fromSubnetId = no DescribeSubnets call — fully static reference.
        this.publicSubnets = [
            ec2.Subnet.fromSubnetId(this, 'PublicSubnet1a', SUBNET_PUBLIC_1A),
            ec2.Subnet.fromSubnetId(this, 'PublicSubnet1b', SUBNET_PUBLIC_1B),
        ];

        this.privateAppSubnets = [
            ec2.Subnet.fromSubnetId(this, 'AppSubnet1a', SUBNET_APP_1A),
            ec2.Subnet.fromSubnetId(this, 'AppSubnet1b', SUBNET_APP_1B),
        ];

        // ── Import Existing RDS Security Group ────────────────────────────────────
        // fromSecurityGroupId imports a reference to the pre-existing RDS SG.
        // CDK does NOT create, modify, or delete this security group.
        // The ECS SG egress rule below references this ID so the DB instance
        // accepts connections from ECS tasks automatically (no new rules needed
        // on the existing RDS SG itself for the egress side).
        //
        // ⚠️  You MAY still need to add an inbound :5432 rule to the existing
        //     RDS SG from the ECS SG ID, if that rule doesn't already exist.
        //     Do this manually in the AWS Console or via the CLI:
        //       aws ec2 authorize-security-group-ingress \
        //         --group-id <RDS_SG_ID> \
        //         --protocol tcp --port 5432 \
        //         --source-group <ECS_SG_ID>
        this.rdsSecurityGroup = ec2.SecurityGroup.fromSecurityGroupId(
            this,
            'ExistingRdsSg',
            RDS_SG_ID,
        );

        // ── ALB Security Group ────────────────────────────────────────────────────
        // Internet-facing load balancer: accepts HTTP and HTTPS from the public internet.
        // Outbound: scoped to ECS SG on port 5000 only (wired below after ECS SG exists).
        // allowAllOutbound: false — Control Tower egress guardrail.
        this.albSecurityGroup = new ec2.SecurityGroup(this, 'AlbSg', {
            vpc: this.vpc,
            description: 'GSS ALB — HTTP/HTTPS inbound from internet, egress to ECS :5000 only',
            allowAllOutbound: false,
        });

        this.albSecurityGroup.addIngressRule(
            ec2.Peer.anyIpv4(),
            ec2.Port.tcp(80),
            'HTTP inbound from internet',
        );
        this.albSecurityGroup.addIngressRule(
            ec2.Peer.anyIpv4(),
            ec2.Port.tcp(443),
            'HTTPS inbound from internet',
        );

        // ── ECS Security Group ────────────────────────────────────────────────────
        // Inbound:  container port 5000 from ALB SG only (no direct internet access).
        // Outbound: SG-based rule to EXISTING RDS SG :5432 + HTTPS :443 for AWS services.
        // allowAllOutbound: false — Control Tower egress guardrail.
        this.ecsSecurityGroup = new ec2.SecurityGroup(this, 'EcsSg', {
            vpc: this.vpc,
            description: 'GSS ECS — inbound from ALB :5000, egress to existing RDS SG :5432 + AWS :443',
            allowAllOutbound: false,
        });

        // Inbound: container port 5000 from ALB SG only
        this.ecsSecurityGroup.addIngressRule(
            ec2.Peer.securityGroupId(this.albSecurityGroup.securityGroupId),
            ec2.Port.tcp(5000),
            'Inbound from ALB SG on container port 5000',
        );

        // Egress: ECS → existing RDS SG :5432
        // References the imported RDS SG ID directly — precise, SG-based, not CIDR.
        this.ecsSecurityGroup.addEgressRule(
            ec2.Peer.securityGroupId(this.rdsSecurityGroup.securityGroupId),
            ec2.Port.tcp(5432),
            'ECS → existing RDS SG :5432',
        );

        // Egress: ECS → AWS managed services :443
        // Required for ECR image pull, CloudWatch Logs, Secrets Manager.
        // Remove if VPC Interface Endpoints for ECR + Logs exist in this account.
        this.ecsSecurityGroup.addEgressRule(
            ec2.Peer.anyIpv4(),
            ec2.Port.tcp(443),
            'ECS → AWS services (ECR + CloudWatch) :443',
        );

        // ── ALB → ECS egress (wired after ECS SG is defined) ─────────────────────
        this.albSecurityGroup.addEgressRule(
            ec2.Peer.securityGroupId(this.ecsSecurityGroup.securityGroupId),
            ec2.Port.tcp(5000),
            'ALB → ECS container port 5000',
        );

        // ── Outputs ───────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'VpcId', {
            value: VPC_ID,
            description: 'Imported enterprise VPC',
            exportName: 'GssRuntimeVpcId',
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
        new cdk.CfnOutput(this, 'RdsSgId', {
            value: this.rdsSecurityGroup.securityGroupId,
            description: 'Imported RDS Security Group ID',
            exportName: 'GssRdsSgId',
        });
    }
}
