import * as cdk from 'aws-cdk-lib';
import * as codecommit from 'aws-cdk-lib/aws-codecommit';
import * as codebuild from 'aws-cdk-lib/aws-codebuild';
import * as codepipeline from 'aws-cdk-lib/aws-codepipeline';
import * as codepipeline_actions from 'aws-cdk-lib/aws-codepipeline-actions';
import * as ecr from 'aws-cdk-lib/aws-ecr';
import * as ecs from 'aws-cdk-lib/aws-ecs';
import * as iam from 'aws-cdk-lib/aws-iam';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

// ─────────────────────────────────────────────────────────────────────────────
// PipelineStack
//
// CI/CD flow:
//
//   Developer git push
//     └── CodeCommit (gss-admin-service, branch: main)
//           └── CodePipeline triggers automatically
//                 └── CodeBuild stage
//                       ├── docker build -t <ECR_URI>:$CODEBUILD_RESOLVED_SOURCE_VERSION
//                       ├── docker push to ECR
//                       ├── writes imagedefinitions.json
//                       └── ECS deploy stage
//                             └── ECS rolling update with new image
//
// BuildSpec phases:
//   install   → nothing (base image has docker + aws cli)
//   pre_build → ecr get-login-password
//   build     → docker build + tag
//   post_build → docker push + write imagedefinitions.json artifact
// ─────────────────────────────────────────────────────────────────────────────

export interface PipelineStackProps extends cdk.StackProps {
    tags: Record<string, string>;
    ecrRepository: ecr.Repository;
    ecsService: ecs.FargateService;
    ecsCluster: ecs.Cluster;
}

export class PipelineStack extends cdk.Stack {

    public readonly repository: codecommit.Repository;

    constructor(scope: Construct, id: string, props: PipelineStackProps) {
        super(scope, id, props);

        // ── CodeCommit Repository ──────────────────────────────────────────────────
        this.repository = new codecommit.Repository(this, 'AdminServiceRepo', {
            repositoryName: 'gss-admin-service',
            description: 'GSS Admin Service source code',

            // Optional: seed the repo with a README so CodePipeline can trigger
            code: codecommit.Code.fromDirectory(
                // Points to the actual admin-service folder in your project
                '../Services/admin-service',
                'main',
            ),
        });

        // ── CodeBuild Log Group ────────────────────────────────────────────────────
        const buildLogGroup = new logs.LogGroup(this, 'BuildLogs', {
            logGroupName: '/codebuild/gss/admin-service',
            retention: logs.RetentionDays.ONE_WEEK,
            removalPolicy: cdk.RemovalPolicy.DESTROY,
        });

        // ── CodeBuild IAM Role ─────────────────────────────────────────────────────
        const buildRole = new iam.Role(this, 'CodeBuildRole', {
            roleName: 'gss-admin-codebuild-role',
            assumedBy: new iam.ServicePrincipal('codebuild.amazonaws.com'),
        });

        // Allow CodeBuild to push/pull images from ECR
        props.ecrRepository.grantPullPush(buildRole);

        // Allow CodeBuild to authenticate with ECR
        buildRole.addToPolicy(new iam.PolicyStatement({
            actions: [
                'ecr:GetAuthorizationToken',
            ],
            resources: ['*'],
        }));

        // Allow CodeBuild to update ECS service (for deploy stage)
        buildRole.addToPolicy(new iam.PolicyStatement({
            actions: [
                'ecs:DescribeServices',
                'ecs:UpdateService',
                'ecs:DescribeTaskDefinition',
                'ecs:RegisterTaskDefinition',
                'iam:PassRole',
            ],
            resources: ['*'],
        }));

        // Allow writing to CloudWatch Logs
        buildRole.addToPolicy(new iam.PolicyStatement({
            actions: [
                'logs:CreateLogGroup',
                'logs:CreateLogStream',
                'logs:PutLogEvents',
            ],
            resources: [buildLogGroup.logGroupArn],
        }));

        // ── CodeBuild Project ──────────────────────────────────────────────────────
        const buildProject = new codebuild.PipelineProject(this, 'AdminBuildProject', {
            projectName: 'gss-admin-service-build',
            role: buildRole,
            description: 'Builds and pushes the admin-service Docker image to ECR',

            environment: {
                // STANDARD_7_0 includes Docker 24, .NET 8, AWS CLI 2
                buildImage: codebuild.LinuxBuildImage.STANDARD_7_0,
                computeType: codebuild.ComputeType.SMALL,
                privileged: true,  // Required for Docker daemon access
            },

            environmentVariables: {
                // These are resolved at build time — not secrets
                AWS_ACCOUNT_ID: {
                    value: this.account,
                    type: codebuild.BuildEnvironmentVariableType.PLAINTEXT,
                },
                AWS_DEFAULT_REGION: {
                    value: this.region,
                    type: codebuild.BuildEnvironmentVariableType.PLAINTEXT,
                },
                ECR_REPOSITORY_URI: {
                    value: props.ecrRepository.repositoryUri,
                    type: codebuild.BuildEnvironmentVariableType.PLAINTEXT,
                },
                IMAGE_TAG: {
                    value: 'latest',
                    type: codebuild.BuildEnvironmentVariableType.PLAINTEXT,
                },
                CONTAINER_NAME: {
                    value: 'admin-service',   // Must match containerName in task definition
                    type: codebuild.BuildEnvironmentVariableType.PLAINTEXT,
                },
            },

            // ── BuildSpec (inline) ──────────────────────────────────────────────────
            // This is the full CI/CD script that runs inside CodeBuild.
            //
            // Phases:
            //   pre_build  → login to ECR
            //   build      → docker build with commit SHA tag + 'latest'
            //   post_build → docker push both tags + write imagedefinitions.json
            //
            // Artifacts:
            //   imagedefinitions.json → used by CodePipeline ECS deploy action
            //   imageDetail.json      → used by ECS rolling deploy
            buildSpec: codebuild.BuildSpec.fromObject({
                version: '0.2',

                phases: {

                    pre_build: {
                        commands: [
                            'echo "=== Pre-Build Phase ==="',
                            'echo "Logging into Amazon ECR..."',
                            'aws ecr get-login-password --region $AWS_DEFAULT_REGION | docker login --username AWS --password-stdin $AWS_ACCOUNT_ID.dkr.ecr.$AWS_DEFAULT_REGION.amazonaws.com',
                            'COMMIT_HASH=$(echo $CODEBUILD_RESOLVED_SOURCE_VERSION | cut -c 1-8)',
                            'IMAGE_TAG_SHA=${COMMIT_HASH:=latest}',
                            'echo "Build image: $ECR_REPOSITORY_URI:$IMAGE_TAG_SHA"',
                        ],
                    },

                    build: {
                        commands: [
                            'echo "=== Build Phase ==="',
                            'echo "Build started at $(date)"',
                            '# Run from the admin-service directory (Dockerfile is at root of service)',
                            'echo "Building Docker image..."',
                            'docker build -t $ECR_REPOSITORY_URI:$IMAGE_TAG_SHA .',
                            'docker tag  $ECR_REPOSITORY_URI:$IMAGE_TAG_SHA $ECR_REPOSITORY_URI:latest',
                            'echo "Build completed at $(date)"',
                        ],
                    },

                    post_build: {
                        commands: [
                            'echo "=== Post-Build Phase ==="',
                            'echo "Pushing image to ECR..."',
                            'docker push $ECR_REPOSITORY_URI:$IMAGE_TAG_SHA',
                            'docker push $ECR_REPOSITORY_URI:latest',
                            'echo "Writing imagedefinitions.json for ECS deploy action..."',
                            // imagedefinitions.json tells CodePipeline which container to update
                            'printf \'[{"name":"%s","imageUri":"%s"}]\' "$CONTAINER_NAME" "$ECR_REPOSITORY_URI:$IMAGE_TAG_SHA" > imagedefinitions.json',
                            'cat imagedefinitions.json',
                            'echo "Pipeline complete — new image: $ECR_REPOSITORY_URI:$IMAGE_TAG_SHA"',
                        ],
                    },
                },

                // Artifacts passed to the ECS deploy action
                artifacts: {
                    files: [
                        'imagedefinitions.json',
                    ],
                },

                // Build cache — speeds up subsequent docker builds
                cache: {
                    paths: [
                        '/root/.nuget/packages/**/*',
                    ],
                },
            }),

            // CloudWatch Logs for build output
            logging: {
                cloudWatch: {
                    logGroup: buildLogGroup,
                    prefix: 'build',
                    enabled: true,
                },
            },

            // Cache configuration
            cache: codebuild.Cache.local(codebuild.LocalCacheMode.DOCKER_LAYER),

            // Auto-timeout
            timeout: cdk.Duration.minutes(30),
        });

        // ── CodePipeline ───────────────────────────────────────────────────────────
        // Artifact buckets — CodePipeline managed
        const sourceArtifact = new codepipeline.Artifact('SourceArtifact');
        const buildArtifact = new codepipeline.Artifact('BuildArtifact');

        // Pipeline role
        const pipelineRole = new iam.Role(this, 'PipelineRole', {
            roleName: 'gss-admin-pipeline-role',
            assumedBy: new iam.ServicePrincipal('codepipeline.amazonaws.com'),
        });

        // Allow pipeline to pass role to ECS deploy action
        pipelineRole.addToPolicy(new iam.PolicyStatement({
            actions: ['iam:PassRole'],
            resources: ['*'],
        }));

        // Allow pipeline to trigger CodeBuild
        pipelineRole.addToPolicy(new iam.PolicyStatement({
            actions: [
                'codebuild:BatchGetBuilds',
                'codebuild:StartBuild',
            ],
            resources: [buildProject.projectArn],
        }));

        // Allow pipeline to update ECS service
        pipelineRole.addToPolicy(new iam.PolicyStatement({
            actions: [
                'ecs:DescribeServices',
                'ecs:DescribeTaskDefinition',
                'ecs:DescribeTasks',
                'ecs:ListTasks',
                'ecs:RegisterTaskDefinition',
                'ecs:UpdateService',
                'ecr:DescribeImages',
            ],
            resources: ['*'],
        }));

        const pipeline = new codepipeline.Pipeline(this, 'AdminPipeline', {
            pipelineName: 'gss-admin-service-pipeline',
            role: pipelineRole,
            pipelineType: codepipeline.PipelineType.V2,
            restartExecutionOnUpdate: true,

            stages: [

                // ── Stage 1: Source ──────────────────────────────────────────────────
                // Triggers automatically on push to 'main' branch
                {
                    stageName: 'Source',
                    actions: [
                        new codepipeline_actions.CodeCommitSourceAction({
                            actionName: 'CodeCommit_Source',
                            repository: this.repository,
                            branch: 'main',
                            output: sourceArtifact,
                            // Poll every minute — use EventBridge trigger in production
                            trigger: codepipeline_actions.CodeCommitTrigger.POLL,
                        }),
                    ],
                },

                // ── Stage 2: Build ───────────────────────────────────────────────────
                // Runs docker build + push, produces imagedefinitions.json
                {
                    stageName: 'Build',
                    actions: [
                        new codepipeline_actions.CodeBuildAction({
                            actionName: 'Docker_Build_Push',
                            project: buildProject,
                            input: sourceArtifact,
                            outputs: [buildArtifact],
                        }),
                    ],
                },

                // ── Stage 3: Deploy ──────────────────────────────────────────────────
                // Uses imagedefinitions.json to update ECS task definition + rolling deploy
                {
                    stageName: 'Deploy',
                    actions: [
                        new codepipeline_actions.EcsDeployAction({
                            actionName: 'ECS_Rolling_Deploy',
                            service: props.ecsService,
                            input: buildArtifact,

                            // Wait up to 10 minutes for the new task to become healthy
                            deploymentTimeout: cdk.Duration.minutes(10),
                        }),
                    ],
                },
            ],
        });

        // ── Outputs ────────────────────────────────────────────────────────────────
        new cdk.CfnOutput(this, 'CodeCommitCloneUrl', {
            value: this.repository.repositoryCloneUrlHttp,
            description: 'CodeCommit HTTPS clone URL — set this as your remote origin',
            exportName: 'GssCodeCommitUrl',
        });

        new cdk.CfnOutput(this, 'CodeCommitSshUrl', {
            value: this.repository.repositoryCloneUrlSsh,
            description: 'CodeCommit SSH clone URL',
            exportName: 'GssCodeCommitSshUrl',
        });

        new cdk.CfnOutput(this, 'PipelineName', {
            value: pipeline.pipelineName,
            description: 'CodePipeline name — monitor in AWS Console',
            exportName: 'GssPipelineName',
        });

        new cdk.CfnOutput(this, 'BuildProjectName', {
            value: buildProject.projectName,
            description: 'CodeBuild project name',
            exportName: 'GssBuildProject',
        });
    }
}
