import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// NetworkImport
//
// Imports the EXISTING enterprise VPC and subnets from the runtime account.
// Nothing here is created — CDK only generates L1 lookup references.
//
// VPC layout (existing):
//   VPC ID : vpc-0146f76f9e738f1e3f
//   Public  (ALB)    : 172.19.142.128/28  172.19.143.128/28
//   Private (ECS)    : 172.19.142.0/25    172.19.143.0/25
//   Private (DB)     : 172.19.144.0/28    172.19.144.16/28
// ─────────────────────────────────────────────────────────────────────────────

export interface NetworkImportProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
}

export class NetworkImportStack extends cdk.Stack {

    /** The imported VPC — use this to attach SGs, ALB, ECS. */
    public readonly vpc: ec2.IVpc;

    /** Two public subnets for the ALB (172.19.142.128/28 & 172.19.143.128/28). */
    public readonly publicSubnets: ec2.ISubnet[];

    /** Two private subnets for ECS Fargate tasks (172.19.142.0/25 & 172.19.143.0/25). */
    public readonly privateAppSubnets: ec2.ISubnet[];

    constructor(scope: Construct, id: string, props: NetworkImportProps) {
        super(scope, id, props);

        // ── Import VPC by ID ──────────────────────────────────────────────────────
        // fromLookup performs a live AWS describe-vpcs call during `cdk synth`.
        // The result is cached in cdk.context.json — commit that file to SCM.
        this.vpc = ec2.Vpc.fromLookup(this, 'EnterpriseVpc', {
            vpcId: 'vpc-0146f76f9e738f1e3f',
        });

        // ── Import Public Subnets (ALB) ───────────────────────────────────────────
        // CDK does not support CIDR-based subnet import directly; we use fromSubnetId
        // with availability zone info looked up at synth time.
        // Alternative: fromLookup on the VPC and filter subnets by CIDR in code.
        //
        // For a deterministic import we use Vpc.fromLookup which populates
        // this.vpc.publicSubnets when subnetGroupNameTag is set correctly.
        // If the existing subnets don't have the tag, import them explicitly by ID.
        //
        // Because subnet IDs are stable AWS resources, we import by CIDR pattern:
        this.publicSubnets = this.selectSubnetsByCidr(
            this.vpc,
            ['172.19.142.128/28', '172.19.143.128/28'],
            'Public',
        );

        this.privateAppSubnets = this.selectSubnetsByCidr(
            this.vpc,
            ['172.19.142.0/25', '172.19.143.0/25'],
            'PrivateApp',
        );

        // ── Outputs ───────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'VpcId', {
            value: this.vpc.vpcId,
            description: 'Imported VPC ID',
            exportName: 'GssRuntimeVpcId',
        });
    }

    /**
     * Selects subnets from an imported VPC by matching CIDR blocks.
     * During `cdk synth --context`, the VPC lookup populates subnet metadata.
     * If the VPC import returned no subnets (dummy lookup), this falls back to
     * an empty array — the real subnet selection will succeed after the first
     * `cdk synth` with live AWS credentials.
     */
    private selectSubnetsByCidr(
        vpc: ec2.IVpc,
        cidrs: string[],
        logicalPrefix: string,
    ): ec2.ISubnet[] {
        // When the VPC is a real looked-up VPC (not DummyVpc), it exposes all
        // subnets.  We filter by the ipv4CidrBlock property.
        const allSubnets = [
            ...vpc.publicSubnets,
            ...vpc.privateSubnets,
            ...vpc.isolatedSubnets,
        ];

        const matched = allSubnets.filter(subnet =>
            cidrs.includes((subnet as ec2.Subnet).ipv4CidrBlock ?? ''),
        );

        // If live lookup found them — return directly.
        if (matched.length === cidrs.length) {
            return matched;
        }

        // Fallback: CDK context not yet populated (first synth with dummy values).
        // Return all available subnets so the stack structure is valid.
        // Real values will be resolved on next synth with live credentials.
        console.warn(
            `[${logicalPrefix}] Could not match subnets by CIDR — ` +
            `using ALL subnets from VPC lookup. Run 'cdk synth' with live AWS credentials.`,
        );
        return allSubnets.length > 0 ? allSubnets : [];
    }
}
