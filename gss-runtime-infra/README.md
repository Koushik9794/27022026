# GSS Runtime Infrastructure — Cross-Account CDK

Deploys the **compute layer only** into the runtime AWS account (`771355239306`).

Runs from: **CI/CD account** `793912575116`  
Deploys to: **Runtime account** `771355239306`  
Region: `ap-south-1`

---

## What This Creates

| Stack | Resources |
|---|---|
| `GssCrossAccountRoleStack` | Exports `arn:aws:iam::771355239036:role/Infra_setup` ARN (pre-existing, not created) |
| `GssNetworkImportStack` | Imports VPC `vpc-0146f76f9e738f1e3f` (no creation) |
| `GssEcsClusterStack` | ECS Cluster, ALB SG, ECS SG |
| `GssAlbStack` | ALB, HTTP :80 Listener, Target Group :5000 |
| `GssServiceStack` | Fargate Task Def, Fargate Service, CloudWatch Logs |

## What This Does NOT Touch

- VPC / Subnets
- RDS instance
- ECR repository
- CodePipeline / CodeBuild / CodeCommit
- Any existing security groups outside this project

---

## One-Time Bootstrap Setup

Run these once per account/region pair before the first CDK deploy.

```bash
# 1. Bootstrap runtime account — trust the CI/CD account
cdk bootstrap aws://771355239306/ap-south-1 \
  --trust 793912575116 \
  --cloudformation-execution-policies arn:aws:iam::aws:policy/AdministratorAccess \
  --profile runtime-account-admin

# 2. Bootstrap CI/CD account (standard)
cdk bootstrap aws://793912575116/ap-south-1 \
  --profile cicd-account-admin
```

---

## Deploy Role

The deployment role **already exists** as an enterprise-managed IAM role:

```
arn:aws:iam::771355239036:role/Infra_setup
```

This role is **not created by CDK**. It is pre-provisioned by the infra team and
trusted to be assumed from CI/CD account `793912575116`.

---

## Regular Deploy (CI/CD Account → Runtime Account)

After the deploy role exists, all subsequent deploys assume it:

```bash
# From CI/CD account
export CDK_DEPLOY_ROLE_ARN=arn:aws:iam::771355239036:role/Infra_setup

cdk deploy --all \
  --role-arn $CDK_DEPLOY_ROLE_ARN \
  --require-approval never
```

Or for individual stacks:

```bash
cdk deploy GssEcsClusterStack GssAlbStack GssServiceStack \
  --role-arn $CDK_DEPLOY_ROLE_ARN \
  --require-approval never
```

---

## Trigger ECS Redeployment After CodeBuild Push

After CodeBuild pushes a new image to ECR:

```bash
CLUSTER=gss-cluster
SERVICE=gss-admin-service
REGION=ap-south-1

# Force a new deployment (picks up latest ECR image tag)
aws ecs update-service \
  --cluster $CLUSTER \
  --service $SERVICE \
  --force-new-deployment \
  --region $REGION
```

Or via CDK (registers a new task definition revision):

```bash
cdk deploy GssServiceStack \
  --role-arn arn:aws:iam::771355239036:role/Infra_setup \
  --require-approval never
```

---

## Environment Variables Injected into ECS

| Variable | Value |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://+:5000` |
| `DB_HOST` | `gss-configurator.c9u4e20w07bp.ap-south-1.rds.amazonaws.com` |
| `DB_PORT` | `5432` |
| `DB_NAME` | `postgres` |
| `DB_USER` | `postgres` |
| `DB_PASSWORD` | `SecureKey_7788` ⚠️ move to Secrets Manager |
| `JWT_SECRET` | `temp-dev-secret` ⚠️ rotate before go-live |

---

## Network Layout (Imported — Not Created)

| Resource | Value |
|---|---|
| VPC | `vpc-0146f76f9e738f1e3f` |
| Public subnets (ALB) | `172.19.142.128/28`, `172.19.143.128/28` |
| Private app subnets (ECS) | `172.19.142.0/25`, `172.19.143.0/25` |
| Private DB subnets | `172.19.144.0/28`, `172.19.144.16/28` |

---

## ECR Repository (Imported — Not Created)

```
771355239036.dkr.ecr.ap-south-1.amazonaws.com/gss-backend
```

> **Note:** The ECR account (`771355239036`) differs from the runtime account
> (`771355239306`). You must add a resource-based policy on the ECR repo allowing
> the ECS task execution role to pull images:
>
> ```bash
> aws ecr set-repository-policy \
>   --repository-name gss-backend \
>   --region ap-south-1 \
>   --profile ecr-account \
>   --policy-text '{
>     "Version": "2012-10-17",
>     "Statement": [{
>       "Sid": "AllowECSPull",
>       "Effect": "Allow",
>       "Principal": {
>         "AWS": "arn:aws:iam::771355239306:role/gss-admin-task-execution-role"
>       },
>       "Action": [
>         "ecr:GetDownloadUrlForLayer",
>         "ecr:BatchGetImage",
>         "ecr:BatchCheckLayerAvailability"
>       ]
>     }]
>   }'
> ```

---

## Verify Service Health

After deploy, get the ALB DNS from CloudFormation outputs:

```bash
aws cloudformation describe-stacks \
  --stack-name GssAlbStack \
  --region ap-south-1 \
  --query "Stacks[0].Outputs[?OutputKey=='AlbDnsName'].OutputValue" \
  --output text

# Test health
curl http://<ALB_DNS>/health
```

Expected response: `200 OK`
