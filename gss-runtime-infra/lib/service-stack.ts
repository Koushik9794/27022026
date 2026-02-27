import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// ServiceStack — Control Tower Compliant
//
// Creates:
//   • CloudWatch Log Group      /ecs/gss/admin-service
//   • Task Execution Role       gss-admin-task-execution-role
//   • Task Role                 gss-admin-task-role
//   • Fargate Task Definition   family: gss-admin-service
//   • Fargate Service           in private app subnets, NO public IP
//
// Imports (not created):
//   • ECR repository  771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend
//
// Control Tower rules satisfied:
//   • assignPublicIp: DISABLED — tasks run in private subnets only
//   • ECS tasks bound to ECS SG (allowAllOutbound:false defined in EcsClusterStack)
//   • No admin roles created — limited least-privilege only
//   • No S3 buckets — no ACL risk
//   • Health check: /swagger (matches ALB target group)
//
// Environment variables injected into container:
//   ASPNETCORE_ENVIRONMENT  Production
//   ASPNETCORE_URLS         http://+:5000
//   DB_HOST                 gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com
//   DB_PORT                 5432
//   DB_NAME                 postgres
//   DB_USER                 postgres
//   DB_PASSWORD             SecureKey_7788  (move to Secrets Manager before prod)
//   JWT_SECRET              temp-dev-secret (rotate before prod)
// ─────────────────────────────────────────────────────────────────────────────

// ── Constants ─────────────────────────────────────────────────────────────────
const DB_HOST = 'gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com';
const DB_PORT = '5432';
const DB_NAME = 'postgres';
const DB_USER = 'postgres';
const DB_PASSWORD = 'SecureKey_7788';   // ← migrate to Secrets Manager before production
const JWT_SECRET = 'temp-dev-secret';

// ECR is in account 771355239036 (note: different from runtime 771355239306)
const ECR_ACCOUNT = '771355239036';
const ECR_REGION = 'ap-south-1';
const ECR_REPO = 'gss-backend';

export interface ServiceStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Private app subnets — ECS tasks ONLY (172.19.142.0/25 & 172.19.143.0/25) */
    readonly privateAppSubnets: ec2.ISubnet[];
    /** ECS security group (allowAllOutbound:false, scoped egress to RDS + :443) */
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
        // Retention: 1 week (cost-controlled). Increase for audit compliance.
        const logGroup = new logs.LogGroup(this, 'AdminServiceLogs', {
            logGroupName: '/ecs/gss/admin-service',
            retention: logs.RetentionDays.ONE_WEEK,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });

        // ── Task Execution Role ────────────────────────────────────────────────────
        // Least-privilege: only what ECS control plane needs.
        //   • AmazonECSTaskExecutionRolePolicy → CloudWatch Logs write, ECR GetAuthorizationToken
        //   • Explicit ECR pull on the specific repo ARN
        // Control Tower: no AdministratorAccess, no wildcard resource on sensitive actions.
        const executionRole = new iam.Role(this, 'TaskExecutionRole', {
            // roleName omitted — CDK auto-generates a unique name.
            // Explicit names in Control Tower accounts can be blocked by IAM
            // naming guardrails. Auto-generated names always succeed.
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
            managedPolicies: [
                iam.ManagedPolicy.fromAwsManagedPolicyName(
                    'service-role/AmazonECSTaskExecutionRolePolicy',
                ),
            ],
        });

        // Explicit ECR pull from the cross-account ECR repository
        // (ECR acct 771355239036 ≠ runtime acct 771355239306 — ECR repo policy also needed)
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'AllowCrossAccountECRPull',
            effect: iam.Effect.ALLOW,
            actions: [
                'ecr:GetDownloadUrlForLayer',
                'ecr:BatchGetImage',
                'ecr:BatchCheckLayerAvailability',
            ],
            resources: [
                `arn:aws:ecr:${ECR_REGION}:${ECR_ACCOUNT}:repository/${ECR_REPO}`,
            ],
        }));

        // GetAuthorizationToken is account-scoped (cannot be scoped to a repo ARN)
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'AllowECRAuthToken',
            effect: iam.Effect.ALLOW,
            actions: ['ecr:GetAuthorizationToken'],
            resources: ['*'],
        }));

        // ── Task Role ─────────────────────────────────────────────────────────────
        // Runtime container role — no permissions currently needed.
        // Add SSM / S3 / SQS here as features require them (scoped to specific resources).
        const taskRole = new iam.Role(this, 'TaskRole', {
            // roleName omitted — CDK auto-generates a unique name (CT safe).
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
        });

        // ── Import Existing ECR Repository ─────────────────────────────────────────
        // fromRepositoryAttributes — zero impact on the existing repo.
        const ecrRepo = ecr.Repository.fromRepositoryAttributes(this, 'GssEcrRepo', {
            repositoryName: ECR_REPO,
            repositoryArn: `arn:aws:ecr:${ECR_REGION}:${ECR_ACCOUNT}:repository/${ECR_REPO}`,
        });

        // ── Fargate Task Definition ────────────────────────────────────────────────
        const taskDef = new ecs.FargateTaskDefinition(this, 'AdminTaskDef', {
            family: 'gss-admin-service',
            cpu: 512,    // 0.5 vCPU
            memoryLimitMiB: 1024,   // 1 GiB
            executionRole,
            taskRole,

            runtimePlatform: {
                operatingSystemFamily: ecs.OperatingSystemFamily.LINUX,
                cpuArchitecture: ecs.CpuArchitecture.X86_64,
            },
        });

        // ── Container Definition ───────────────────────────────────────────────────
        const container = taskDef.addContainer('AdminServiceContainer', {
            containerName: 'admin-service',

            // Pull latest image from existing ECR repo
            image: ecs.ContainerImage.fromEcrRepository(ecrRepo, 'latest'),

            // ── Environment Variables ──────────────────────────────────────────────
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

            // ── CloudWatch Logging ────────────────────────────────────────────────
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'admin-service',
                logGroup,
            }),

            // ── Container Health Check ────────────────────────────────────────────
            // Path /swagger matches ALB target group health check — must be consistent.
            // startPeriod: 90s covers .NET app startup + DI initialization time.
            healthCheck: {
                command: ['CMD-SHELL', 'curl -sf http://localhost:5000/swagger || exit 1'],
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                retries: 5,                               // more retries for CT grace
                startPeriod: cdk.Duration.seconds(90),        // allow app to fully start
            },
        });

        // Container → host port mapping: 5000/tcp
        container.addPortMappings({
            containerPort: 5000,
            protocol: ecs.Protocol.TCP,
        });

        // ── Fargate Service ────────────────────────────────────────────────────────
        // Control Tower requirements enforced here:
        //   • vpcSubnets: ONLY private app subnets (172.19.142.0/25 & 172.19.143.0/25)
        //   • assignPublicIp: false — tasks MUST NOT have public IPs
        //   • securityGroups: ECS SG (allowAllOutbound:false, scoped egress)
        this.fargateService = new ecs.FargateService(this, 'AdminFargateService', {
            // serviceName omitted — CDK auto-generates (CT naming guardrail safe).
            cluster: props.cluster,
            taskDefinition: taskDef,
            desiredCount: 1,

            securityGroups: [props.ecsSecurityGroup],

            // PRIVATE subnets only — Control Tower blocks ECS in public subnets
            vpcSubnets: {
                subnets: props.privateAppSubnets,
            },

            // NO public IP — mandatory for CT compliance
            assignPublicIp: false,

            // Auto-rollback if new task fails to become healthy
            circuitBreaker: { rollback: true },

            // Grace period: must be > container startPeriod (90s) to avoid premature kills
            healthCheckGracePeriod: cdk.Duration.seconds(150),

            // Rolling update config: 0% min allows single-task replacement without over-provisioning
            minHealthyPercent: 0,
            maxHealthyPercent: 200,
        });

        // ── Attach service to ALB target group ────────────────────────────────────
        this.fargateService.attachToApplicationTargetGroup(props.targetGroup);

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'ServiceName', {
            value: this.fargateService.serviceName,
            description: 'Fargate service name',
            exportName: 'GssEcsServiceName',
        });

        new cdk.CfnOutput(this, 'TaskDefinitionFamily', {
            value: taskDef.family,
            description: 'Task definition family — new revision registered on each deploy',
            exportName: 'GssTaskDefFamily',
        });

        new cdk.CfnOutput(this, 'LogGroupName', {
            value: logGroup.logGroupName,
            description: 'CloudWatch log group — tail: aws logs tail /ecs/gss/admin-service --follow',
            exportName: 'GssLogGroup',
        });

        new cdk.CfnOutput(this, 'EcrRepositoryUri', {
            value: ecrRepo.repositoryUri,
            description: 'ECR URI used by CodeBuild to push images',
            exportName: 'GssEcrRepositoryUri',
        });
    }
}
