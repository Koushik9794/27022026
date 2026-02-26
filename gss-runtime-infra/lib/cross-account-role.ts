import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// CrossAccountRoleStack
//
// Deploys INTO: runtime account 771355239306
//
// The cross-account deploy role already exists as a pre-provisioned enterprise
// role in account 771355239036:
//
//   arn:aws:iam::771355239036:role/Infra_setup
//
// This stack does NOT create any IAM role.
// It exists solely to export the known role ARN as a CloudFormation output
// so downstream tooling (CodeBuild, scripts) can reference it consistently.
//
// Trust chain (preconfigured by enterprise team):
//   CI/CD account (793912575116) → AssumeRole → Infra_setup (771355239036)
//   CDK executes CloudFormation changes under this role.
//
// Usage (CI/CD account):
//   aws sts assume-role \
//     --role-arn arn:aws:iam::771355239036:role/Infra_setup \
//     --role-session-name cdk-deploy
// ─────────────────────────────────────────────────────────────────────────────

export interface CrossAccountRoleProps extends cdk.StackProps {
    readonly cicdAccountId: string;
}

export class CrossAccountRoleStack extends cdk.Stack {

    /** ARN of the pre-existing cross-account deploy role. */
    public readonly deployRoleArn: string;

    constructor(scope: Construct, id: string, props: CrossAccountRoleProps) {
        super(scope, id, props);

        // ── Pre-existing enterprise deploy role ────────────────────────────────────
        // This role was provisioned outside CDK by the enterprise infra team.
        // We reference it here so all stacks and tooling use a single source of truth.
        this.deployRoleArn = 'arn:aws:iam::771355239036:role/Infra_setup';

        // ── Outputs ───────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'InfraSetupRoleArn', {
            value: this.deployRoleArn,
            description: 'Pre-existing deploy role ARN — pass to: cdk deploy --role-arn <this>',
            exportName: 'GssDeployRoleArn',
        });

        new cdk.CfnOutput(this, 'AssumeRoleCommand', {
            value: [
                'aws sts assume-role',
                `--role-arn ${this.deployRoleArn}`,
                '--role-session-name cdk-deploy',
                '--query "Credentials.[AccessKeyId,SecretAccessKey,SessionToken]"',
                '--output text',
            ].join(' '),
            description: 'One-liner to assume the deploy role from your CI/CD account',
        });
    }
}
