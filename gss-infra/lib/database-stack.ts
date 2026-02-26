import * as cdk from 'aws-cdk-lib';
import * as ec2 from 'aws-cdk-lib/aws-ec2';
import * as rds from 'aws-cdk-lib/aws-rds';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// DatabaseStack
//
// Creates:
//   • Secrets Manager secret → stores DB master password (auto-generated)
//   • RDS Subnet Group       → private subnets only
//   • RDS PostgreSQL 16      → Single-AZ (t3.micro) in private subnet
//   • admin_service database is created automatically by the app on first boot
//     (DatabaseHelper.EnsureDatabase runs before FluentMigrator)
// ─────────────────────────────────────────────────────────────────────────────

export interface DatabaseStackProps extends cdk.StackProps {
    tags: Record<string, string>;
    vpc: ec2.Vpc;
    ecsSecurityGroup: ec2.SecurityGroup;
}

export class DatabaseStack extends cdk.Stack {

    public readonly dbInstance: rds.DatabaseInstance;

    // The CDK-managed secret — passed to ECS so the task can read the password
    public readonly dbSecret: secretsmanager.ISecret;

    constructor(scope: Construct, id: string, props: DatabaseStackProps) {
        super(scope, id, props);

        // ── RDS Security Group ─────────────────────────────────────────────────────
        // Defined here (separate from network-stack) so DatabaseStack is self-contained
        const rdsSecurityGroup = new ec2.SecurityGroup(this, 'RdsSg', {
            vpc: props.vpc,
            securityGroupName: 'gss-rds-sg',
            description: 'RDS PostgreSQL: allow from ECS only',
            allowAllOutbound: false,
        });
        rdsSecurityGroup.addIngressRule(
            ec2.Peer.securityGroupId(props.ecsSecurityGroup.securityGroupId),
            ec2.Port.tcp(5432),
            'Allow PostgreSQL access from ECS Fargate tasks',
        );

        // ── DB Subnet Group ────────────────────────────────────────────────────────
        const subnetGroup = new rds.SubnetGroup(this, 'DbSubnetGroup', {
            description: 'GSS RDS subnet group — private subnets',
            vpc: props.vpc,
            vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS },
            subnetGroupName: 'gss-db-subnet-group',
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });

        // ── RDS PostgreSQL Instance ────────────────────────────────────────────────
        this.dbInstance = new rds.DatabaseInstance(this, 'GssPostgres', {
            // Engine
            engine: rds.DatabaseInstanceEngine.postgres({
                version: rds.PostgresEngineVersion.VER_16,
            }),

            // DB identifier shown in RDS console
            instanceIdentifier: 'gss-admin-db',

            // Credentials — CDK auto-generates a random password, stores in Secrets Manager
            credentials: rds.Credentials.fromGeneratedSecret('postgres', {
                secretName: 'gss/admin-service/db-credentials',
            }),

            // Instance size — t3.micro is Free Tier eligible
            instanceType: ec2.InstanceType.of(
                ec2.InstanceClass.T3,
                ec2.InstanceSize.MICRO,
            ),

            // Storage
            allocatedStorage: 20,
            storageType: rds.StorageType.GP2,
            storageEncrypted: false,   // set true for production

            // Network
            vpc: props.vpc,
            subnetGroup,
            securityGroups: [rdsSecurityGroup],
            vpcSubnets: { subnetType: ec2.SubnetType.PRIVATE_WITH_EGRESS },
            publiclyAccessible: false,

            // Availability
            multiAz: false,  // set true for production
            autoMinorVersionUpgrade: true,
            allowMajorVersionUpgrade: false,
            deleteAutomatedBackups: true,

            // Maintenance
            backupRetention: cdk.Duration.days(1),
            deletionProtection: false,          // set true for production

            // Cleanup — allows stack destroy during development
            removalPolicy: cdk.RemovalPolicy.DESTROY,

            // CloudWatch logs
            cloudwatchLogsExports: ['postgresql'],
            cloudwatchLogsRetention: cdk.aws_logs.RetentionDays.ONE_WEEK,

            // Parameter group — use default
            parameterGroup: rds.ParameterGroup.fromParameterGroupName(
                this,
                'PgParamGroup',
                'default.postgres16',
            ),
        });

        // Expose the auto-generated secret so ECS stack can reference it
        this.dbSecret = this.dbInstance.secret!;

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'DbEndpoint', {
            value: this.dbInstance.instanceEndpoint.hostname,
            description: 'RDS PostgreSQL endpoint — use as DB_HOST in ECS task',
            exportName: 'GssDbEndpoint',
        });

        new cdk.CfnOutput(this, 'DbPort', {
            value: this.dbInstance.instanceEndpoint.port.toString(),
            exportName: 'GssDbPort',
        });

        new cdk.CfnOutput(this, 'DbSecretArn', {
            value: this.dbSecret.secretArn,
            description: 'Secrets Manager ARN containing DB credentials',
            exportName: 'GssDbSecretArn',
        });
    }
}
