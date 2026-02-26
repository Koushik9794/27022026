#!/usr/bin/env bash
# ─────────────────────────────────────────────────────────────────────────────
# deploy.sh — One-shot GSS admin-service deployment script
#
# Usage:
#   chmod +x deploy.sh
#   ./deploy.sh
#
# Prerequisites:
#   • AWS CLI configured (aws configure OR env vars)
#   • Node.js ≥ 18 installed
#   • Docker running (for CodeBuild local testing, optional)
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

# ── Colors ────────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; CYAN='\033[0;36m'; NC='\033[0m'

info()    { echo -e "${CYAN}[INFO]${NC}  $*"; }
success() { echo -e "${GREEN}[OK]${NC}    $*"; }
warn()    { echo -e "${YELLOW}[WARN]${NC}  $*"; }
error()   { echo -e "${RED}[ERROR]${NC} $*"; exit 1; }

# ── Validate prerequisites ─────────────────────────────────────────────────────
info "Checking prerequisites..."
command -v aws    >/dev/null 2>&1 || error "AWS CLI not found. Install from: https://aws.amazon.com/cli/"
command -v node   >/dev/null 2>&1 || error "Node.js not found. Install v18+ from: https://nodejs.org"
command -v npm    >/dev/null 2>&1 || error "npm not found."
command -v git    >/dev/null 2>&1 || error "git not found."
success "All prerequisites found."

# ── AWS Identity Check ─────────────────────────────────────────────────────────
info "Verifying AWS credentials..."
CALLER_IDENTITY=$(aws sts get-caller-identity 2>/dev/null) || error "AWS credentials not configured. Run: aws configure"
AWS_ACCOUNT=$(echo "$CALLER_IDENTITY" | grep -o '"Account": "[^"]*"' | cut -d'"' -f4)
AWS_REGION=${AWS_DEFAULT_REGION:-${CDK_DEFAULT_REGION:-ap-south-1}}
success "AWS Account: $AWS_ACCOUNT | Region: $AWS_REGION"

# Export for CDK
export CDK_DEFAULT_ACCOUNT="$AWS_ACCOUNT"
export CDK_DEFAULT_REGION="$AWS_REGION"

# ── Install CDK dependencies ───────────────────────────────────────────────────
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

info "Installing npm dependencies..."
npm install --silent
success "Dependencies installed."

# ── CDK Bootstrap ─────────────────────────────────────────────────────────────
info "Bootstrapping CDK (safe to re-run)..."
npx cdk bootstrap "aws://$CDK_DEFAULT_ACCOUNT/$CDK_DEFAULT_REGION" \
  --require-approval never \
  --quiet
success "CDK bootstrapped."

# ── Synthesize ─────────────────────────────────────────────────────────────────
info "Synthesizing CloudFormation templates..."
npx cdk synth --quiet
success "Synthesis complete."

# ── Deploy stacks in order ─────────────────────────────────────────────────────
STACKS=(
  "GssNetworkStack"
  "GssDatabaseStack"
  "GssEcsStack"
  "GssPipelineStack"
)

for STACK in "${STACKS[@]}"; do
  info "Deploying $STACK..."
  npx cdk deploy "$STACK" \
    --require-approval never \
    --outputs-file "outputs-${STACK}.json" \
    2>&1 | tail -20
  success "$STACK deployed."
done

# ── Read outputs ───────────────────────────────────────────────────────────────
info "Deployment complete. Reading outputs..."

ALB_DNS=$(cat outputs-GssEcsStack.json 2>/dev/null | \
  grep -o '"AlbDnsName": "[^"]*"' | cut -d'"' -f4 || echo "<not found>")

CODECOMMIT_URL=$(cat outputs-GssPipelineStack.json 2>/dev/null | \
  grep -o '"CodeCommitCloneUrl": "[^"]*"' | cut -d'"' -f4 || echo "<not found>")

DB_ENDPOINT=$(cat outputs-GssDatabaseStack.json 2>/dev/null | \
  grep -o '"DbEndpoint": "[^"]*"' | cut -d'"' -f4 || echo "<not found>")

# ── Print summary ──────────────────────────────────────────────────────────────
echo ""
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo -e "${GREEN}  GSS admin-service — AWS Deployment Complete${NC}"
echo -e "${GREEN}════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "  ${CYAN}ALB Health Check URL:${NC}"
echo -e "    http://${ALB_DNS}/health"
echo ""
echo -e "  ${CYAN}CodeCommit Clone URL:${NC}"
echo -e "    ${CODECOMMIT_URL}"
echo ""
echo -e "  ${CYAN}RDS Endpoint:${NC}"
echo -e "    ${DB_ENDPOINT}"
echo ""
echo -e "  ${CYAN}Next step — push code to trigger pipeline:${NC}"
echo -e "    cd ../Services/admin-service"
echo -e "    git remote add codecommit ${CODECOMMIT_URL}"
echo -e "    git push codecommit main"
echo ""
echo -e "  ${CYAN}Monitor pipeline:${NC}"
echo -e "    aws codepipeline list-pipeline-executions \\"
echo -e "      --pipeline-name gss-admin-service-pipeline \\"
echo -e "      --max-results 1"
echo ""
echo -e "  ${CYAN}View container logs:${NC}"
echo -e "    aws logs tail /ecs/gss/admin-service --follow"
echo ""

# ── Health check ──────────────────────────────────────────────────────────────
if [[ "$ALB_DNS" != "<not found>" ]]; then
  info "Running health check (may take 2-3 min for container to start)..."
  for i in {1..10}; do
    HTTP_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "http://${ALB_DNS}/health" || echo "000")
    if [[ "$HTTP_STATUS" == "200" ]]; then
      success "Health check passed! Service is running."
      curl -s "http://${ALB_DNS}/health" | python3 -m json.tool 2>/dev/null || true
      break
    else
      warn "Attempt $i/10 — HTTP $HTTP_STATUS — waiting 30s..."
      sleep 30
    fi
  done
fi
