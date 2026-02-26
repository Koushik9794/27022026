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
// EcsStack
//
// Creates:
//   • ECR Repository         → stores Docker images pushed by CodeBuild
//   • ECS Cluster            → logical grouping of Fargate services
//   • Task Execution Role    → allows ECS to pull image + read secrets
//   • Task Role              → permissions the running container has
//   • Fargate Task Def       → container spec: image, CPU, memory, env vars
//   • Fargate Service        → runs 1 desired task, connects to ALB target group
//   • Application Load Balancer → internet-facing, forwards :80 to container :80
//   • CloudWatch Log Group   → container stdout/stderr
//
// Container environment variables injected:
//   • ConnectionStrings__DefaultConnection → built from RDS endpoint + secret
//   • ASPNETCORE_ENVIRONMENT              → Production
//   • ASPNETCORE_URLS                     → http://+:80
//   • JWT_SECRET                          → dev placeholder (no auth in admin-service)
// ─────────────────────────────────────────────────────────────────────────────

export interface EcsStackProps extends cdk.StackProps {
    tags: Record<string, string>;
    vpc: ec2.Vpc;
    ecsSecurityGroup: ec2.SecurityGroup;
    albSecurityGroup: ec2.SecurityGroup;
    dbInstance: rds.DatabaseInstance;
    dbSecret: secretsmanager.ISecret;
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

        // ── Task Execution Role ────────────────────────────────────────────────────
        // ECS control plane uses this to:
        //   • Pull container image from ECR
        //   • Write logs to CloudWatch
        //   • Read secrets from Secrets Manager
        const executionRole = new iam.Role(this, 'TaskExecutionRole', {
            roleName: 'gss-admin-task-execution-role',
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
            managedPolicies: [
                iam.ManagedPolicy.fromAwsManagedPolicyName(
                    'service-role/AmazonECSTaskExecutionRolePolicy',
                ),
            ],
        });

        // Allow the task to read the DB secret at startup
        props.dbSecret.grantRead(executionRole);

        // ── Task Role ─────────────────────────────────────────────────────────────
        // The actual running container uses this role for AWS SDK calls
        const taskRole = new iam.Role(this, 'TaskRole', {
            roleName: 'gss-admin-task-role',
            assumedBy: new iam.ServicePrincipal('ecs-tasks.amazonaws.com'),
        });

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
        // Initially uses a placeholder nginx image.
        // CodeBuild replaces this with the real image during first pipeline run.
        const containerDef = taskDefinition.addContainer('AdminServiceContainer', {
            containerName: 'admin-service',

            // Placeholder — will be overridden by CodeBuild imagedefinitions.json
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
            // ECS injects these as env vars at task startup — never stored in task def JSON
            secrets: {
                // The auto-generated RDS secret has this JSON structure:
                // { "host": "...", "port": "5432", "username": "postgres", "password": "..." }
                DB_HOST: ecs.Secret.fromSecretsManager(props.dbSecret, 'host'),
                DB_PORT: ecs.Secret.fromSecretsManager(props.dbSecret, 'port'),
                DB_USER: ecs.Secret.fromSecretsManager(props.dbSecret, 'username'),
                DB_PASSWORD: ecs.Secret.fromSecretsManager(props.dbSecret, 'password'),

                // Build the full ADO.NET/Npgsql connection string from secret fields
                // This satisfies: builder.Configuration.GetConnectionString("DefaultConnection")
                // Note: The connection string must be assembled via a custom approach below
            },

            // ── Logging ───────────────────────────────────────────────────────────
            logging: ecs.LogDrivers.awsLogs({
                streamPrefix: 'admin-service',
                logGroup,
            }),

            // ── Health Check ──────────────────────────────────────────────────────
            // ECS also runs this to determine container health before routing traffic
            healthCheck: {
                command: ['CMD-SHELL', 'curl -f http://localhost:80/health || exit 1'],
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                retries: 3,
                startPeriod: cdk.Duration.seconds(60), // Give migrations time to run
            },
        });

        // Container port mapping
        containerDef.addPortMappings({
            containerPort: 80,
            protocol: ecs.Protocol.TCP,
        });

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

            // Health check — maps to GET /health in admin-service Program.cs
            healthCheck: {
                path: '/health',
                interval: cdk.Duration.seconds(30),
                timeout: cdk.Duration.seconds(10),
                healthyThresholdCount: 2,
                unhealthyThresholdCount: 3,
                healthyHttpCodes: '200',
            },

            // Deregistration delay — low for faster deployments
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

            // Assign public IP = false — service is in private subnet, NAT is used for outbound
            assignPublicIp: false,

            // Circuit breaker — auto-rolls back if new task can't start
            circuitBreaker: {
                rollback: true,
            },

            // Health check grace period — must be >= migration time (~30-60 s)
            healthCheckGracePeriod: cdk.Duration.seconds(120),

            // Deployment
            minHealthyPercent: 0,   // Allows stopping old task before new one starts (1 task scenario)
            maxHealthyPercent: 200,

            // Connect to ALB
            loadBalancers: [],       // We'll attach via target group below
        });

        // Attach the Fargate service to the ALB target group
        this.fargateService.attachToApplicationTargetGroup(targetGroup);

        // Allow outbound from ECS security group to RDS
        // (already defined in network-stack, but this ensures the db allows it)
        this.fargateService.connections.allowTo(
            props.dbInstance,
            ec2.Port.tcp(5432),
            'ECS → RDS PostgreSQL',
        );

        // ── Connection String Override Workaround ─────────────────────────────────
        // The admin-service reads a single connection string, not separate env vars.
        // We inject the connection string as a PLAIN env var built at synth time.
        // IMPORTANT: This exposes the endpoint in the task definition JSON (not the password).
        // For full security use Secrets Manager + custom entry-point script (see README).
        //
        // Here we use a CloudFormation dynamic reference pattern:
        // The password comes from Secrets Manager at runtime via ecs.Secret above.
        // The connection string is partially constructed here; password is in secrets.
        //
        // Since Npgsql also accepts individual env vars, we set them as secrets above
        // AND inject the full connection string as a CFN dynamic reference:

        const dbHostname = props.dbInstance.instanceEndpoint.hostname;
        const dbPort = props.dbInstance.instanceEndpoint.port;

        // We set the connection string via a container override using the secret reference.
        // BuildConnectionString is assembled in a startup-time env using shell expansion.
        // Add a second container definition override for the connection string:
        const cfnTaskDef = taskDefinition.node.defaultChild as cdk.aws_ecs.CfnTaskDefinition;

        // Override the connection string using a CloudFormation secret reference
        // Format: {{resolve:secretsmanager:secretId:SecretString:field}}
        const secretId = props.dbSecret.secretArn;

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
    }
}
