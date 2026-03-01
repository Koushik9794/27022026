import * as cdk from 'aws-cdk-lib';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// CrossAccountRoleStack
//
// Account topology (THREE accounts — read carefully, one digit differs):
//   CI/CD Account      : 793912575116  ← CodeBuild runs here, cdk deploy invoked here
//   ECR/Deploy Account : 771355239036  ← Infra_setup role lives here + ECR repo
//   Runtime Account    : 771355239306  ← Fargate/ECS stacks deployed here
//
// This stack does NOT create any IAM role.
// The cross-account deploy role already exists as a pre-provisioned enterprise
// role in account 771355239036:
//
//   arn:aws:iam::771355239036:role/Infra_setup
//
// Full trust chain (all must be correctly configured before cdk deploy works):
//
//   CodeBuild (gss-admin-codebuild-role @ 793912575116)
//     ↓ sts:AssumeRole  ← codebuild role has sts:AssumeRole on Infra_setup (done in pipeline-stack.ts)
//   Infra_setup (@ 771355239036)
//     ↓ assumes CDK bootstrap deploy role
//   cdk-hnb659fds-deploy-role-771355239306-ap-south-1 (@ 771355239306)
//     ↓ CDK orchestrates CloudFormation
//   cdk-hnb659fds-cfn-exec-role-771355239306-ap-south-1 (@ 771355239306)
//     ↓ CloudFormation creates/updates resources
//
// ─────────────────────────────────────────────────────────────────────────────
// MANUAL STEPS REQUIRED (outside CDK, done once by enterprise infra team):
//
// STEP 1 — Infra_setup trust policy (account 771355239036) must include:
//   {
//     "Effect": "Allow",
//     "Principal": {
//       "AWS": "arn:aws:iam::793912575116:role/gss-admin-codebuild-role"
//     },
//     "Action": "sts:AssumeRole"
//   }
//
// STEP 2 — CDK bootstrap in runtime account (run once with admin creds for 771355239306):
//   cdk bootstrap aws://771355239306/ap-south-1 \
//     --trust 793912575116 \
//     --cloudformation-execution-policies arn:aws:iam::aws:policy/AdministratorAccess
//
// STEP 3 — After bootstrap, add Infra_setup to the CDK deploy-role trust policy.
//   The deploy-role is: cdk-hnb659fds-deploy-role-771355239306-ap-south-1 (in account 771355239306)
//   Its trust policy must ALSO allow:
//   {
//     "Effect": "Allow",
//     "Principal": {
//       "AWS": "arn:aws:iam::771355239036:role/Infra_setup"
//     },
//     "Action": "sts:AssumeRole"
//   }
//   NOTE: --trust 793912575116 only adds the CI/CD account to trust; Infra_setup
//   (which resides in a THIRD account 771355239036) must be added separately.
//
// STEP 4 — Verify no Control Tower SCP blocks cross-account sts:AssumeRole.
//   Check SCPs in org root and both OUs that govern 771355239036 and 771355239306.
// ─────────────────────────────────────────────────────────────────────────────
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
