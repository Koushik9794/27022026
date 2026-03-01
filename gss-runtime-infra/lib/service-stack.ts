import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// ServiceStack — Bootstrap-Free, Control Tower Compliant
//
// ZERO IAM RESOURCES:
//   This stack creates NO AWS::IAM::Role, AWS::IAM::Policy, or
//   AWS::IAM::ManagedPolicy resources. All roles are pre-provisioned by the
//   platform team and imported by ARN via fromRoleArn (mutable: false).
//   Policy mutations from CDK are fully disabled.
//
// Pre-provisioned roles required (platform team creates these once):
//
//   taskExecutionRoleArn  (passed via ServiceStackProps)
//     Trust:    ecs-tasks.amazonaws.com
//     Policies: AmazonECSTaskExecutionRolePolicy (AWS managed)
//               ecr:GetAuthorizationToken (*)
//               ecr:GetDownloadUrlForLayer, BatchGetImage, BatchCheckLayerAvailability
//                 (on ECR repo arn:aws:ecr:ap-south-1:771355239036:repository/gss-backend)
//               logs:CreateLogStream, logs:PutLogEvents
//                 (on /ecs/gss/admin-service log group)
//
//   taskRoleArn  (passed via ServiceStackProps)
//     Trust:    ecs-tasks.amazonaws.com
//     Policies: (none required at launch; add SSM/S3/SQS as app grows)
//
// Bootstrap-free design:
//   • NO DockerImageAsset / Asset bundling
//   • NO ECR L2 repository object
//   • Image referenced as a plain registry string — CDK generates no assets
//   • cdk synth produces a self-contained CloudFormation template
//   • No CDK toolkit bucket required
//   • No bootstrap stack required
//
// Creates:
//   • CloudWatch Log Group
//   • Fargate Task Definition (auto-named family)
//   • Fargate Service in private subnets, NO public IP
//
// Does NOT create:
//   ✗ AWS::IAM::Role
//   ✗ AWS::IAM::Policy
//   ✗ AWS::IAM::ManagedPolicy
//
// Image pulled from (pre-built by CodeBuild before cdk deploy):
//   771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend:latest
//
// Control Tower rules satisfied:
//   • assignPublicIp: DISABLED
//   • Zero IAM resources — CT IAM guardrails cannot block the stack
//   • No S3 buckets — no ACL risk
//   • No asset uploads — no bootstrap bucket needed
// ─────────────────────────────────────────────────────────────────────────────

// ── Constants ─────────────────────────────────────────────────────────────────
const DB_HOST = 'gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com';
const DB_PORT = '5432';
const DB_NAME = 'postgres';
const DB_USER = 'postgres';
const DB_PASSWORD = 'SecureKey_7788';   // ← migrate to Secrets Manager before production
const JWT_SECRET = 'temp-dev-secret';

// Full ECR image URI — plain string, no CDK asset or API call.
// The pipeline (CodeBuild) builds + pushes :latest before cdk deploy runs.
// CDK writes this string directly into the CloudFormation task definition.
const ECR_IMAGE_URI = '771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend:latest';

// ─────────────────────────────────────────────────────────────────────────────

export interface ServiceStackProps extends cdk.StackProps {
    readonly tags: Record<string, string>;
    readonly vpc: ec2.IVpc;
    /** Private app subnets — ECS tasks ONLY (172.19.142.0/25 & 172.19.143.0/25) */
    readonly privateAppSubnets: ec2.ISubnet[];
    /** ECS security group (allowAllOutbound:false, scoped egress to RDS + :443) */
    readonly ecsSecurityGroup: ec2.SecurityGroup;
    readonly cluster: ecs.Cluster;
    readonly targetGroup: elbv2.ApplicationTargetGroup;

    /**
     * ARN of the pre-existing ECS Task Execution Role.
     * Must be pre-provisioned by the platform team with:
     *   - AmazonECSTaskExecutionRolePolicy (AWS managed)
     *   - ECR pull from 771355239036 (cross-account)
     *   - CloudWatch Logs write on /ecs/gss/admin-service
     *
     * Example: arn:aws:iam::771355239306:role/gss-admin-task-execution-role
     */
    readonly taskExecutionRoleArn: string;

    /**
     * ARN of the pre-existing ECS Task Role.
     * Trust principal: ecs-tasks.amazonaws.com
     *
     * Example: arn:aws:iam::771355239306:role/gss-admin-task-role
     */
    readonly taskRoleArn: string;
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

        // ── Import pre-existing IAM Roles (NO IAM resources created) ──────────────
        //
        // mutable: false — CDK will NOT attach any policies to these roles.
        // All permissions are pre-configured on the roles by the platform team.
        // This produces zero AWS::IAM::* resources in the CloudFormation template.
        const executionRole = iam.Role.fromRoleArn(
            this,
            'TaskExecutionRole',
            props.taskExecutionRoleArn,
            { mutable: false },
        );

        const taskRole = iam.Role.fromRoleArn(
            this,
            'TaskRole',
            props.taskRoleArn,
            { mutable: false },
        );

        // ── Fargate Task Definition ────────────────────────────────────────────────
        // family omitted — CDK auto-generates (CT naming guardrail safe).
        // No Docker image is built here. CDK only writes the ECR URI string into
        // the CloudFormation template — zero asset, zero bootstrap bucket access.
        const taskDef = new ecs.FargateTaskDefinition(this, 'AdminTaskDef', {
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
        // ContainerImage.fromRegistry() accepts any fully-qualified image URI.
        // This is a plain string substitution in CloudFormation — NO asset pipeline,
        // NO CDK toolkit bucket, NO bootstrap required.
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

            // Container health check — /swagger path matches ALB target group.
            // startPeriod: 90s covers .NET startup + DI initialization time.
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
        // CT requirements:
        //   • vpcSubnets: ONLY private app subnets
        //   • assignPublicIp: false — MANDATORY
        //   • securityGroups: ECS SG (allowAllOutbound:false — defined in NetworkStack)
        this.fargateService = new ecs.FargateService(this, 'AdminFargateService', {
            // serviceName omitted — CDK auto-generates (CT naming guardrail safe).
            cluster: props.cluster,
            taskDefinition: taskDef,
            desiredCount: 1,

            securityGroups: [props.ecsSecurityGroup],

            vpcSubnets: {
                subnets: props.privateAppSubnets,
            },

            assignPublicIp: false,   // CT REQUIRED — no public IP on tasks

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

        new cdk.CfnOutput(this, 'LogGroupName', {
            value: logGroup.logGroupName,
            description: 'CloudWatch log group',
            exportName: 'GssLogGroup',
        });

        new cdk.CfnOutput(this, 'EcrImageUri', {
            value: ECR_IMAGE_URI,
            description: 'ECR image URI deployed by this task definition',
            exportName: 'GssEcrImageUri',
        });

        new cdk.CfnOutput(this, 'TaskExecutionRoleArn', {
            value: props.taskExecutionRoleArn,
            description: 'Imported (pre-existing) task execution role ARN',
            exportName: 'GssTaskExecutionRoleArn',
        });
    }
}
