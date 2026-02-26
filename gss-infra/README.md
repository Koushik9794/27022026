# GSS Infrastructure — AWS CDK Deployment Guide

## Architecture Overview

```
Internet
   │
   ▼
[Application Load Balancer]   ← public subnet
   │  port 80
   ▼
[ECS Fargate — admin-service] ← private subnet
   │  port 5432
   ▼
[RDS PostgreSQL 16]           ← private subnet
```

## CI/CD Flow

```
git push (main branch)
   │
   ▼
[CodeCommit — gss-admin-service]
   │  triggers on push
   ▼
[CodePipeline — gss-admin-service-pipeline]
   │
   ├── Stage 1: Source   → checkout code
   ├── Stage 2: Build    → docker build → docker push ECR
   └── Stage 3: Deploy   → ECS rolling update
```

## Prerequisites

```bash
# 1. Install Node.js ≥ 18
node --version

# 2. Install AWS CDK globally
npm install -g aws-cdk@2

# 3. Configure AWS credentials
aws configure
# Or via environment variables:
export AWS_ACCESS_KEY_ID=<your-key>
export AWS_SECRET_ACCESS_KEY=<your-secret>
export AWS_DEFAULT_REGION=ap-south-1

# 4. Set your AWS account
export CDK_DEFAULT_ACCOUNT=$(aws sts get-caller-identity --query Account --output text)
export CDK_DEFAULT_REGION=ap-south-1
```

## First-Time Setup

```bash
# From the gss-infra directory:

# 1. Install dependencies
npm install

# 2. Bootstrap CDK in your AWS account (once per account/region)
cdk bootstrap aws://$CDK_DEFAULT_ACCOUNT/$CDK_DEFAULT_REGION

# 3. Synthesize CloudFormation (verify output before deploying)
cdk synth

# 4. Deploy all stacks
cdk deploy --all --require-approval never
```

## Or Deploy One Stack at a Time (Recommended)

```bash
# Deploy in order (dependencies are respected automatically)
cdk deploy GssNetworkStack
cdk deploy GssDatabaseStack
cdk deploy GssEcsStack
cdk deploy GssPipelineStack
```

## After Deployment — Verify

```bash
# Get the ALB URL from CDK output:
# GssEcsStack.AlbDnsName = gss-admin-alb-xxxxxxxxx.ap-south-1.elb.amazonaws.com

# 1. Health check
curl http://<ALB_DNS_NAME>/health

# Expected response:
# {"status":"healthy","timestamp":"2026-02-26T07:50:00Z","service":"admin-service","version":"1.0.0"}

# 2. Check ECS service status
aws ecs describe-services \
  --cluster gss-cluster \
  --services gss-admin-service \
  --query 'services[0].{Status:status,Desired:desiredCount,Running:runningCount}'

# 3. View container logs (real-time)
aws logs tail /ecs/gss/admin-service --follow
```

## Push Code to CodeCommit to Trigger Pipeline

```bash
# Get the CodeCommit HTTPS URL from CDK output:
# GssPipelineStack.CodeCommitCloneUrl = https://git-codecommit.ap-south-1.amazonaws.com/...

# Add CodeCommit as a remote (or re-init from the admin-service folder)
cd Services/admin-service
git init
git remote add codecommit <CodeCommitCloneUrl>

# Push to trigger pipeline
git add .
git commit -m "Initial deployment"
git push codecommit main
```

## Watch Pipeline (AWS Console)

```
AWS Console → CodePipeline → gss-admin-service-pipeline
```

Or via CLI:
```bash
aws codepipeline list-pipeline-executions \
  --pipeline-name gss-admin-service-pipeline \
  --max-results 1 \
  --query 'pipelineExecutionSummaries[0]'
```

## Stacks Summary

| Stack | Resources Created |
|-------|-------------------|
| `GssNetworkStack` | VPC, 2 public + 2 private subnets, NAT Gateway, 3 Security Groups |
| `GssDatabaseStack` | RDS PostgreSQL 16 (t3.micro), DB Subnet Group, Secrets Manager secret |
| `GssEcsStack` | ECR repo, ECS Cluster, Fargate Service (1 task), ALB, Target Group, CloudWatch Log Group |
| `GssPipelineStack` | CodeCommit repo, CodeBuild project, CodePipeline (3 stages), IAM roles |

## Environment Variables Injected into Container

| Variable | Source | Value |
|----------|--------|-------|
| `ASPNETCORE_ENVIRONMENT` | Task Def | `Production` |
| `ASPNETCORE_URLS` | Task Def | `http://+:80` |
| `JWT_SECRET` | Task Def | `dev-secret-key-replace-before-production` |
| `ConnectionStrings__DefaultConnection` | CFN Dynamic Ref → Secrets Manager | Built from RDS endpoint + secret |

## Tear Down

```bash
# Destroy all resources (WARNING: removes RDS data!)
cdk destroy --all --force
```

## Cost Estimate (ap-south-1, per month)

| Resource | Est. Cost |
|----------|-----------|
| RDS t3.micro (on-demand) | ~$15 |
| Fargate (0.5 vCPU / 1 GB, 24/7) | ~$18 |
| ALB | ~$18 |
| NAT Gateway | ~$35 |
| CodeBuild (free tier: 100 min/month) | ~$0-$3 |
| ECR (first 500MB free) | ~$0-$1 |
| **Total** | **~$85-$90/month** |

> 💡 Tip: Stop Fargate tasks and RDS when not in use to reduce costs.
