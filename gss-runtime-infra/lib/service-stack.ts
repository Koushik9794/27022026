import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// ServiceStack — CDK-Managed IAM Roles, Bootstrap-Free, Control Tower Compliant
//
// IAM Strategy:
//   CDK creates the Task Execution Role and Task Role directly in this stack.
//   No external role ARNs are imported. No iam.Role.fromRoleArn() anywhere.
//   CDK auto-generates role names → no CT naming guardrail conflicts.
//
// Task Execution Role (ECS control-plane identity):
//   - AmazonECSTaskExecutionRolePolicy (AWS managed — ECR auth + CW Logs)
//   - ecr:GetDownloadUrlForLayer / BatchGetImage / BatchCheckLayerAvailability
//       scoped to the specific cross-account ECR repo ARN
//   - ecr:GetAuthorizationToken on * (AWS-mandated — cannot be further scoped)
//   - logs:CreateLogStream / PutLogEvents scoped to this stack's log group
//
// Task Role (container runtime identity):
//   - Zero permissions by default (least-privilege baseline)
//   - Add SSM / S3 / SQS inline policies here as the application grows
//
// Bootstrap-free design:
//   • ContainerImage.fromRegistry(uri) — plain string in CloudFormation
//   • No DockerImageAsset, no CDK toolkit bucket, no bootstrap stack
//   • cdk synth produces a fully self-contained CloudFormation template
//
// Control Tower compliance:
//   • assignPublicIp: false — mandatory
//   • No explicit role/resource names — CDK auto-generates (CT guardrail safe)
//   • No S3 buckets, no ACLs
//   • Security group egress scoped in NetworkImportStack
// ─────────────────────────────────────────────────────────────────────────────

// ── Constants ─────────────────────────────────────────────────────────────────
const DB_HOST = 'gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com';
const DB_PORT = '5432';
const DB_NAME = 'postgres';
const DB_USER = 'postgres';
const DB_PASSWORD = 'SecureKey_7788';   // ← TODO: migrate to Secrets Manager
const JWT_SECRET = 'temp-dev-secret'; // ← TODO: rotate before production

// Full ECR image URI — plain string, zero CDK asset, zero bootstrap access.
// CodeBuild pushes :latest before CDK deploy runs. CDK writes this string
// directly into the CloudFormation task definition JSON.
const ECR_IMAGE_URI = '771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend:latest';

// ECR repo ARN — used only to scope the IAM pull policy (no ECR L2 object).
// No account ID hardcoded in CDK logic — only in this ARN string for IAM scoping.
const ECR_REPO_ARN = 'arn:aws:ecr:ap-south-1:771355239036:repository/gss-backend';

// ─────────────────────────────────────────────────────────────────────────────

export interface ServiceStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Private app subnets — ECS tasks ONLY (172.19.142.0/25 & 172.19.143.0/25) */
    readonly privateAppSubnets: ec2.ISubnet[];
    /** ECS security group (allowAllOutbound:false, scoped egress in NetworkImportStack) */
    readonly ecsSecurityGroup: ec2.SecurityGroup;
    readonly cluster: ecs.Cluster;
    readonly targetGroup: elbv2.ApplicationTargetGroup;
}

export class ServiceStack extends cdk.Stack {

    /** The Fargate service */
    public readonly fargateService: ecs.FargateService;

    constructor(scope: Construct, id: string, props: ServiceStackProps) {
        super(scope, id, props);

        // ── CloudWatch Log Group ───────────────────────────────────────────────────
        const logGroup = new logs.LogGroup(this, 'AdminServiceLogs', {
            logGroupName: '/ecs/gss/admin-service',
            retention: logs.RetentionDays.ONE_WEEK,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });

        // ── Task Execution Role ────────────────────────────────────────────────────
        // Used by the ECS CONTROL PLANE (not the container) to:
        //   • Pull the container image from ECR
        //   • Write container stdout/stderr to CloudWatch Logs
        //
        // roleName omitted — CDK auto-generates a unique name.
        // Explicit names in Control Tower accounts trigger naming guardrail SCPs.
        const executionRole = new iam.Role(this, 'TaskExecutionRole', {
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
            description: 'ECS Fargate task execution role — ECR pull + CloudWatch Logs',
            managedPolicies: [
                // AWS managed: covers basic ECR auth token + CloudWatch Logs write
                iam.ManagedPolicy.fromAwsManagedPolicyName(
                    'service-role/AmazonECSTaskExecutionRolePolicy',
                ),
            ],
        });

        // Scoped ECR pull: only the specific cross-account repo (less permissive than *)
        // ECR account 771355239036 ≠ runtime account 771355239306 — cross-account pull.
        // The ECR repo itself also needs a resource-based policy allowing this role's account.
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'CrossAccountECRPull',
            effect: iam.Effect.ALLOW,
            actions: [
                'ecr:GetDownloadUrlForLayer',
                'ecr:BatchGetImage',
                'ecr:BatchCheckLayerAvailability',
            ],
            resources: [ECR_REPO_ARN],   // scoped to exact repo — not wildcard
        }));

        // ecr:GetAuthorizationToken is account-level — AWS does not allow scoping to a repo ARN.
        // This is a documented AWS limitation, not a misconfiguration.
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'ECRAuthToken',
            effect: iam.Effect.ALLOW,
            actions: ['ecr:GetAuthorizationToken'],
            resources: ['*'],   // unavoidable — AWS constraint
        }));

        // Scoped CloudWatch Logs write — only this service's log group (not *)
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'CloudWatchLogsWrite',
            effect: iam.Effect.ALLOW,
            actions: [
                'logs:CreateLogStream',
                'logs:PutLogEvents',
            ],
            resources: [logGroup.logGroupArn],   // scoped to this log group only
        }));

        // ── Task Role ─────────────────────────────────────────────────────────────
        // Used by the RUNNING CONTAINER for any AWS SDK calls the application makes.
        // Starts with ZERO permissions — least-privilege baseline.
        //
        // To add permissions as the application grows, use:
        //   taskRole.addToPolicy(new iam.PolicyStatement({ ... }));
        //
        // Examples when needed:
        //   S3 read:   s3:GetObject on arn:aws:s3:::my-bucket/*
        //   SQS send:  sqs:SendMessage on specific queue ARN
        //   SSM read:  ssm:GetParameter on specific parameter path
        const taskRole = new iam.Role(this, 'TaskRole', {
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
            description: 'ECS Fargate task role — runtime container AWS SDK identity',
            // Zero managed policies — add inline policies below as features require them
        });

        // ── Fargate Task Definition ────────────────────────────────────────────────
        // family omitted — CDK auto-generates (CT naming guardrail safe).
        // Image is referenced as a plain string — no CDK asset pipeline triggered.
        const taskDef = new ecs.FargateTaskDefinition(this, 'AdminTaskDef', {
            cpu: 512,    // 0.5 vCPU — scale to 1024 under load
            memoryLimitMiB: 1024,   // 1 GiB   — scale to 2048 under load
            executionRole,          // CDK-created role — no ARN import
            taskRole,               // CDK-created role — no ARN import
            runtimePlatform: {
                operatingSystemFamily: ecs.OperatingSystemFamily.LINUX,
                cpuArchitecture: ecs.CpuArchitecture.X86_64,
            },
        });

        // ── Container Definition ───────────────────────────────────────────────────
        // ContainerImage.fromRegistry(string) → pure CloudFormation string substitution.
        // No Docker build, no asset upload, no CDK toolkit bucket interaction.
        const container = taskDef.addContainer('AdminServiceContainer', {
            containerName: 'admin-service',

            image: ecs.ContainerImage.fromRegistry(ECR_IMAGE_URI),

            environment: {
                ASPNETCORE_ENVIRONMENT: 'Production',
                ASPNETCORE_URLS: 'http://+:5000',
                DB_HOST,
                DB_PORT,
                DB_NAME,
                DB_USER,
                DB_PASSWORD,
                JWT_SECRET,
            },

            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'admin-service',
                logGroup,
            }),

            // Health check: /swagger matches ALB target group path.
            // startPeriod: 90s covers .NET cold start + DI wiring time.
            healthCheck: {
                command: ['CMD-SHELL', 'curl -sf http://localhost:5000/swagger || exit 1'],
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                retries: 5,
                startPeriod: cdk.Duration.seconds(90),
            },
        });

        container.addPortMappings({
            containerPort: 5000,
            protocol: ecs.Protocol.TCP,
        });

        // ── Fargate Service ────────────────────────────────────────────────────────
        // Control Tower requirements:
        //   • vpcSubnets: ONLY private app subnets (no public placement)
        //   • assignPublicIp: false — MANDATORY in CT account
        //   • securityGroups: ECS SG (allowAllOutbound:false, defined in NetworkImportStack)
        // serviceName omitted — CDK auto-generates (CT naming guardrail safe).
        this.fargateService = new ecs.FargateService(this, 'AdminFargateService', {
            cluster: props.cluster,
            taskDefinition: taskDef,
            desiredCount: 1,

            securityGroups: [props.ecsSecurityGroup],

            vpcSubnets: {
                subnets: props.privateAppSubnets,
            },

            assignPublicIp: false,   // CT REQUIRED — tasks must never have public IPs

            circuitBreaker: { rollback: true },

            // Grace period must exceed container startPeriod (90s)
            healthCheckGracePeriod: cdk.Duration.seconds(150),

            minHealthyPercent: 0,
            maxHealthyPercent: 200,
        });

        this.fargateService.attachToApplicationTargetGroup(props.targetGroup);

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'ServiceName', {
            value: this.fargateService.serviceName,
            description: 'Fargate service name',
            exportName: 'GssEcsServiceName',
        });

        new cdk.CfnOutput(this, 'TaskDefinitionArn', {
            value: taskDef.taskDefinitionArn,
            description: 'Task definition ARN — latest active revision',
            exportName: 'GssTaskDefArn',
        });

        new cdk.CfnOutput(this, 'TaskExecutionRoleArn', {
            value: executionRole.roleArn,
            description: 'CDK-created task execution role ARN',
            exportName: 'GssTaskExecutionRoleArn',
        });

        new cdk.CfnOutput(this, 'TaskRoleArn', {
            value: taskRole.roleArn,
            description: 'CDK-created task role ARN',
            exportName: 'GssTaskRoleArn',
        });

        new cdk.CfnOutput(this, 'LogGroupName', {
            value: logGroup.logGroupName,
            description: 'CloudWatch log group — tail: aws logs tail /ecs/gss/admin-service --follow',
            exportName: 'GssLogGroup',
        });

        new cdk.CfnOutput(this, 'EcrImageUri', {
            value: ECR_IMAGE_URI,
            description: 'ECR image URI deployed by this task definition',
            exportName: 'GssEcrImageUri',
        });
    }
}
