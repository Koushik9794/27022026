import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// ServiceStack
//
// Deploys INTO: runtime account 771355239306
//
// Creates:
//   • Task Execution Role  — ECS control plane: pull image from ECR, write logs
//   • Task Role            — running container: (no extra AWS calls needed now)
//   • Fargate Task Definition (512 CPU / 1024 MiB)
//   • Container Definition with ALL required environment variables
//   • CloudWatch Log Group
//   • Fargate Service in private app subnets, attached to existing ALB target group
//
// Imports:
//   • Existing ECR repository  771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend
//
// Does NOT create: VPC, RDS, ECR, CodePipeline, CodeBuild, CodeCommit
//
// Environment variables injected into ECS task:
//   ASPNETCORE_ENVIRONMENT  = Production
//   DB_HOST                 = gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com
//   DB_PORT                 = 5432
//   DB_NAME                 = postgres
//   DB_USER                 = postgres
//   DB_PASSWORD             = SecureKey_7788          (plaintext for now — see note)
//   JWT_SECRET              = temp-dev-secret
//   ASPNETCORE_URLS         = http://+:5000
// ─────────────────────────────────────────────────────────────────────────────

// ── RDS / DB Constants ────────────────────────────────────────────────────────
// These are injected as plain env vars.
// Recommendation: move DB_PASSWORD to AWS Secrets Manager before go-live.
const DB_HOST = 'gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com';
const DB_PORT = '5432';
const DB_NAME = 'postgres';
const DB_USER = 'postgres';
const DB_PASSWORD = 'SecureKey_7788';   // ← move to Secrets Manager in production
const JWT_SECRET = 'temp-dev-secret';

// ── ECR ───────────────────────────────────────────────────────────────────────
// Account that owns the ECR repo.  Note: different from runtime account (typo in spec
// has 771355239036, runtime account is 771355239306 — used as-is from spec).
const ECR_ACCOUNT = '771355239036';
const ECR_REGION = 'ap-south-1';
const ECR_REPO = 'gss-backend';

export interface ServiceStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Private app subnets for ECS tasks (172.19.142.0/25 & 172.19.143.0/25) */
    readonly privateAppSubnets: ec2.ISubnet[];
    readonly ecsSecurityGroup: ec2.SecurityGroup;
    readonly cluster: ecs.Cluster;
    readonly targetGroup: elbv2.ApplicationTargetGroup;
}

export class ServiceStack extends cdk.Stack {

    /** The Fargate service — used by deploy scripts to force new deployment. */
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
        // ECS uses this role to pull the image from ECR and write container logs.
        const executionRole = new iam.Role(this, 'TaskExecutionRole', {
            roleName: 'gss-admin-task-execution-role',
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
            managedPolicies: [
                iam.ManagedPolicy.fromAwsManagedPolicyName(
                    'service-role/AmazonECSTaskExecutionRolePolicy',
                ),
            ],
        });

        // Allow ECS to pull from the EXISTING cross-account ECR repository.
        // The ECR repo is in account 771355239036; if this differs from the runtime
        // account, you must also add a resource-based policy on the ECR repo itself
        // allowing ecs-tasks from account 771355239306 to pull.
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'AllowECRPull',
            actions: [
                'ecr:GetDownloadUrlForLayer',
                'ecr:BatchGetImage',
                'ecr:BatchCheckLayerAvailability',
            ],
            resources: [
                `arn:aws:ecr:${ECR_REGION}:${ECR_ACCOUNT}:repository/${ECR_REPO}`,
            ],
        }));
        // GetAuthorizationToken is account-scoped (not resource ARN scoped)
        executionRole.addToPolicy(new iam.PolicyStatement({
            sid: 'AllowECRAuthToken',
            actions: ['ecr:GetAuthorizationToken'],
            resources: ['*'],
        }));

        // ── Task Role ─────────────────────────────────────────────────────────────
        // The running container uses this for AWS SDK calls.
        // No extra permissions needed right now — add S3 / SSM / SQS here as required.
        const taskRole = new iam.Role(this, 'TaskRole', {
            roleName: 'gss-admin-task-role',
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
        });

        // ── Reference Existing ECR Repository ─────────────────────────────────────
        // fromRepositoryAttributes imports without creating or modifying the repo.
        const ecrRepo = ecr.Repository.fromRepositoryAttributes(this, 'GssEcrRepo', {
            repositoryName: ECR_REPO,
            repositoryArn: `arn:aws:ecr:${ECR_REGION}:${ECR_ACCOUNT}:repository/${ECR_REPO}`,
        });

        // ── Fargate Task Definition ────────────────────────────────────────────────
        const taskDef = new ecs.FargateTaskDefinition(this, 'AdminTaskDef', {
            family: 'gss-admin-service',
            cpu: 512,    // 0.5 vCPU  — scale to 1024 under load
            memoryLimitMiB: 1024,   // 1 GiB     — scale to 2048 under load
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

            // Pull from the existing ECR repository in account 771355239036.
            // CodeBuild will update this tag on each pipeline run via imagedefinitions.json.
            image: ecs.ContainerImage.fromEcrRepository(ecrRepo, 'latest'),

            // ── Environment Variables ──────────────────────────────────────────────
            // All values are plaintext except DB_PASSWORD (acceptable for dev/staging;
            // move to Secrets Manager before production go-live).
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

            // ── Logging ───────────────────────────────────────────────────────────
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'admin-service',
                logGroup,
            }),

            // ── Container Health Check ────────────────────────────────────────────
            // ECS uses this to determine container readiness before routing traffic.
            healthCheck: {
                command: ['CMD-SHELL', 'curl -f http://localhost:5000/health || exit 1'],
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                retries: 3,
                startPeriod: cdk.Duration.seconds(60),   // Migrations time
            },
        });

        // Map container port 5000 → TCP
        container.addPortMappings({
            containerPort: 5000,
            protocol: ecs.Protocol.TCP,
        });

        // ── Fargate Service ────────────────────────────────────────────────────────
        this.fargateService = new ecs.FargateService(this, 'AdminFargateService', {
            serviceName: 'gss-admin-service',
            cluster: props.cluster,
            taskDefinition: taskDef,
            desiredCount: 1,
            securityGroups: [props.ecsSecurityGroup],

            // Place tasks in private app subnets (172.19.142.0/25 & 172.19.143.0/25)
            vpcSubnets: {
                subnets: props.privateAppSubnets,
            },

            // No public IP — tasks are in private subnets, NAT handles outbound
            assignPublicIp: false,

            // Deployment rollback circuit breaker
            circuitBreaker: { rollback: true },

            // Grace period while the app starts before health checks are enforced
            healthCheckGracePeriod: cdk.Duration.seconds(120),

            // Deployment config — 0% min allows replacing a single task without overlap
            minHealthyPercent: 0,
            maxHealthyPercent: 200,
        });

        // ── Attach to ALB Target Group ─────────────────────────────────────────────
        this.fargateService.attachToApplicationTargetGroup(props.targetGroup);

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'ServiceName', {
            value: this.fargateService.serviceName,
            description: 'ECS Fargate service name',
            exportName: 'GssEcsServiceName',
        });

        new cdk.CfnOutput(this, 'TaskDefinitionFamily', {
            value: taskDef.family,
            description: 'Task definition family — revisions registered here on each deploy',
            exportName: 'GssTaskDefFamily',
        });

        new cdk.CfnOutput(this, 'LogGroupName', {
            value: logGroup.logGroupName,
            description: 'CloudWatch log group for container stdout/stderr',
            exportName: 'GssLogGroup',
        });

        new cdk.CfnOutput(this, 'EcrRepositoryUri', {
            value: ecrRepo.repositoryUri,
            description: 'ECR URI — CodeBuild pushes images here',
            exportName: 'GssEcrRepositoryUri',
        });
    }
}
