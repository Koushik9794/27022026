import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as elbv2 from 'aws-cdk-lib/aws-elasticloadbalancingv2';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// EcsStack — Control Tower Compliant (Zero IAM resource creation)
//
// IAM POLICY:
//   This stack creates ZERO AWS::IAM::* CloudFormation resources.
//   All IAM roles are pre-provisioned by the platform/enterprise team and
//   imported by ARN. CDK never mutates them.
//
// Role ARNs are passed via props (read from CDK context in gss-infra.ts).
// Set them in cdk.json context or export as env vars before deploying.
//
// Pre-provisioned roles required (platform team must create these):
//
//   taskExecutionRoleArn
//     Trust:    ecs-tasks.amazonaws.com
//     Policies: AmazonECSTaskExecutionRolePolicy (AWS managed)
//               ecr:GetAuthorizationToken (*)
//               ecr:GetDownloadUrlForLayer, BatchGetImage, BatchCheckLayerAvailability (ECR repo)
//               secretsmanager:GetSecretValue (DB secret ARN)
//               logs:CreateLogStream, logs:PutLogEvents (log group ARN)
//
//   taskRoleArn
//     Trust:    ecs-tasks.amazonaws.com
//     Policies: (none required at launch — add SSM/S3/SQS as app grows)
//
// Creates:
//   • ECR Repository         → stores Docker images pushed by CodeBuild
//   • ECS Cluster            → logical grouping of Fargate services
//   • Fargate Task Def       → container spec: image, CPU, memory, env vars
//   • Fargate Service        → runs 1 desired task, connects to ALB target group
//   • Application Load Balancer → internet-facing, forwards :80 to container :80
//   • CloudWatch Log Group   → container stdout/stderr
//
// Does NOT create:
//   ✗ AWS::IAM::Role
//   ✗ AWS::IAM::Policy
//   ✗ AWS::IAM::ManagedPolicy
// ─────────────────────────────────────────────────────────────────────────────

export interface EcsStackProps extends cdk.StackProps {
    tags: Record<string, string>;
    vpc: ec2.Vpc;
    ecsSecurityGroup: ec2.SecurityGroup;
    albSecurityGroup: ec2.SecurityGroup;
    dbInstance: rds.DatabaseInstance;
    dbSecret: secretsmanager.ISecret;

    /**
     * ARN of the pre-existing ECS Task Execution Role.
     * Must be pre-provisioned by the platform team with:
     *   - AmazonECSTaskExecutionRolePolicy (AWS managed)
     *   - ECR pull permissions
     *   - Secrets Manager read on the DB secret
     *
     * Example: arn:aws:iam::<ACCOUNT_ID>:role/gss-admin-task-execution-role
     */
    readonly taskExecutionRoleArn: string;

    /**
     * ARN of the pre-existing ECS Task Role.
     * Must be pre-provisioned by the platform team.
     * Trust principal: ecs-tasks.amazonaws.com
     *
     * Example: arn:aws:iam::<ACCOUNT_ID>:role/gss-admin-task-role
     */
    readonly taskRoleArn: string;
}

export class EcsStack extends cdk.Stack {

    public readonly ecrRepository: ecr.Repository;
    public readonly cluster: ecs.Cluster;
    public readonly fargateService: ecs.FargateService;

    constructor(scope: Construct, id: string, props: EcsStackProps) {
        super(scope, id, props);

        // ── ECR Repository ─────────────────────────────────────────────────────────
        // CodeBuild will push images here; ECS will pull from here on deploy
        this.ecrRepository = new ecr.Repository(this, 'AdminServiceRepo', {
            repositoryName: 'gss/admin-service',
            imageScanOnPush: true,
            encryption: ecr.RepositoryEncryption.AES_256,

            // Keep only last 10 images to control storage cost
            lifecycleRules: [
                {
                    rulePriority: 1,
                    description: 'Keep last 10 images',
                    tagStatus: ecr.TagStatus.ANY,
                    maxImageCount: 10,
                },
            ],

            removalPolicy: cdk.RemovalPolicy.DESTROY,
            emptyOnDelete: true,
        });

        // ── ECS Cluster ────────────────────────────────────────────────────────────
        this.cluster = new ecs.Cluster(this, 'GssCluster', {
            clusterName: 'gss-cluster',
            vpc: props.vpc,
            enableFargateCapacityProviders: true,
            containerInsights: true,
        });

        // ── CloudWatch Log Group ───────────────────────────────────────────────────
        const logGroup = new logs.LogGroup(this, 'AdminServiceLogs', {
            logGroupName: '/ecs/gss/admin-service',
            retention: logs.RetentionDays.ONE_WEEK,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });

        // ── Import pre-existing IAM Roles (NO IAM resources created) ──────────────
        //
        // mutable: false — CDK will NOT attempt to attach any policies to these
        // roles. All permissions must be pre-configured on the roles by the
        // platform team. This produces zero AWS::IAM::* resources in the template.
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
        const taskDefinition = new ecs.FargateTaskDefinition(this, 'AdminTaskDef', {
            family: 'gss-admin-service',
            cpu: 512,    // 0.5 vCPU  — increase to 1024 for load
            memoryLimitMiB: 1024,  // 1 GB      — increase to 2048 for load
            executionRole,
            taskRole,

            runtimePlatform: {
                operatingSystemFamily: ecs.OperatingSystemFamily.LINUX,
                cpuArchitecture: ecs.CpuArchitecture.X86_64,
            },
        });

        // ── Container Definition ───────────────────────────────────────────────────
        // Initially uses the ECR image. CodeBuild replaces this during first run.
        const containerDef = taskDefinition.addContainer('AdminServiceContainer', {
            containerName: 'admin-service',

            image: ecs.ContainerImage.fromEcrRepository(this.ecrRepository, 'latest'),

            // ── Environment Variables ──────────────────────────────────────────────
            // Plain text — non-sensitive configuration only
            environment: {
                ASPNETCORE_ENVIRONMENT: 'Production',
                ASPNETCORE_URLS: 'http://+:80',
                // JWT_SECRET is not used by admin-service (no auth middleware there)
                // but included as a safety net for future integrations
                JWT_SECRET: 'dev-secret-key-replace-before-production',
            },

            // ── Secrets (from Secrets Manager) ────────────────────────────────────
            // ECS injects these as env vars at task startup — never stored in task def JSON.
            // NOTE: The task execution role must have secretsmanager:GetSecretValue
            // on this secret ARN (pre-configured by platform team, NOT added here).
            secrets: {
                DB_HOST: ecs.Secret.fromSecretsManager(props.dbSecret, 'host'),
                DB_PORT: ecs.Secret.fromSecretsManager(props.dbSecret, 'port'),
                DB_USER: ecs.Secret.fromSecretsManager(props.dbSecret, 'username'),
                DB_PASSWORD: ecs.Secret.fromSecretsManager(props.dbSecret, 'password'),
            },

            // ── Logging ───────────────────────────────────────────────────────────
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'admin-service',
                logGroup,
            }),

            // ── Health Check ──────────────────────────────────────────────────────
            healthCheck: {
                command: ['CMD-SHELL', 'curl -f http://localhost:80/health || exit 1'],
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                retries: 3,
                startPeriod: cdk.Duration.seconds(60),
            },
        });

        // Container port mapping
        containerDef.addPortMappings({
            containerPort: 80,
            protocol: ecs.Protocol.TCP,
        });

        // ── Connection String (Fn::Join — no IAM impact) ──────────────────────────
        // Assembles the Npgsql connection string from RDS endpoint + Secrets Manager
        // references at CloudFormation deployment time. No IAM resource is created.
        const dbHostname = props.dbInstance.instanceEndpoint.hostname;
        const dbPort = props.dbInstance.instanceEndpoint.port;
        const secretId = props.dbSecret.secretArn;

        const cfnTaskDef = taskDefinition.node.defaultChild as cdk.aws_ecs.CfnTaskDefinition;
        cfnTaskDef.addPropertyOverride(
            'ContainerDefinitions.0.Environment',
            [
                { Name: 'ASPNETCORE_ENVIRONMENT', Value: 'Production' },
                { Name: 'ASPNETCORE_URLS', Value: 'http://+:80' },
                { Name: 'JWT_SECRET', Value: 'dev-secret-key-replace-before-production' },
                {
                    Name: 'ConnectionStrings__DefaultConnection',
                    Value: {
                        'Fn::Join': [
                            '',
                            [
                                'Host=',
                                dbHostname,
                                ';Port=',
                                dbPort.toString(),
                                ';Database=admin_service;Username={{resolve:secretsmanager:',
                                secretId,
                                ':SecretString:username}};Password={{resolve:secretsmanager:',
                                secretId,
                                ':SecretString:password}};Ssl Mode=Require;Trust Server Certificate=true;',
                            ],
                        ],
                    },
                },
            ],
        );

        // ── Application Load Balancer ──────────────────────────────────────────────
        const alb = new elbv2.ApplicationLoadBalancer(this, 'GssAlb', {
            loadBalancerName: 'gss-admin-alb',
            vpc: props.vpc,
            internetFacing: true,
            securityGroup: props.albSecurityGroup,
            vpcSubnets: { subnetType: ec2.SubnetType.PUBLIC },
        });

        // HTTP listener on port 80
        const listener = alb.addListener('HttpListener', {
            port: 80,
            open: false,        // Security group controls access, not listener
            defaultAction: elbv2.ListenerAction.fixedResponse(503, {
                contentType: 'text/plain',
                messageBody: 'Service starting...',
            }),
        });

        // ── Target Group ──────────────────────────────────────────────────────────
        const targetGroup = new elbv2.ApplicationTargetGroup(this, 'AdminTargetGroup', {
            targetGroupName: 'gss-admin-tg',
            vpc: props.vpc,
            port: 80,
            protocol: elbv2.ApplicationProtocol.HTTP,
            targetType: elbv2.TargetType.IP,  // Required for Fargate

            healthCheck: {
                path: '/health',
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                healthyThresholdCount: 2,
                unhealthyThresholdCount: 3,
                healthyHttpCodes: '200',
            },

            deregistrationDelay: cdk.Duration.seconds(30),
        });

        // Add target group to listener
        listener.addTargetGroups('AdminTargetGroup', {
            targetGroups: [targetGroup],
        });

        // ── Fargate Service ────────────────────────────────────────────────────────
        this.fargateService = new ecs.FargateService(this, 'AdminFargateService', {
            serviceName: 'gss-admin-service',
            cluster: this.cluster,
            taskDefinition,
            desiredCount: 1,
            securityGroups: [props.ecsSecurityGroup],
            vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS },

            assignPublicIp: false,

            circuitBreaker: {
                rollback: true,
            },

            healthCheckGracePeriod: cdk.Duration.seconds(120),

            minHealthyPercent: 0,
            maxHealthyPercent: 200,

            loadBalancers: [],
        });

        // Attach the Fargate service to the ALB target group
        this.fargateService.attachToApplicationTargetGroup(targetGroup);

        // Allow outbound from ECS security group to RDS (SG rule only — no IAM)
        this.fargateService.connections.allowTo(
            props.dbInstance,
            ec2.Port.tcp(5432),
            'ECS → RDS PostgreSQL',
        );

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'AlbDnsName', {
            value: alb.loadBalancerDnsName,
            description: 'ALB DNS — access admin-service at http://<this>/health',
            exportName: 'GssAlbDns',
        });

        new cdk.CfnOutput(this, 'EcrRepositoryUri', {
            value: this.ecrRepository.repositoryUri,
            description: 'ECR URI — CodeBuild pushes here',
            exportName: 'GssEcrUri',
        });

        new cdk.CfnOutput(this, 'EcsClusterName', {
            value: this.cluster.clusterName,
            exportName: 'GssEcsCluster',
        });

        new cdk.CfnOutput(this, 'EcsServiceName', {
            value: this.fargateService.serviceName,
            exportName: 'GssEcsService',
        });

        new cdk.CfnOutput(this, 'TaskExecutionRoleArn', {
            value: props.taskExecutionRoleArn,
            description: 'Imported (pre-existing) task execution role ARN',
            exportName: 'GssTaskExecutionRole',
        });
    }
}
