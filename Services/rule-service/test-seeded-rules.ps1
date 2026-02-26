# PowerShell script to test seeded rules
# Run this from PowerShell

$BASE_URL = "http://localhost:5001"

# Seeded IDs from migration
$PRODUCT_GROUP_ID = "11111111-1111-1111-1111-111111111111"
$COUNTRY_ID = "22222222-2222-2222-2222-222222222222"
$RULESET_ID = "33333333-3333-3333-3333-333333333333"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Rule Service - Seeded Data Test" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. Testing Health Endpoint..." -ForegroundColor Yellow
Invoke-RestMethod -Uri "$BASE_URL/health" | ConvertTo-Json
Write-Host ""

Write-Host "2. Getting RuleSet by ID..." -ForegroundColor Yellow
Write-Host "GET $BASE_URL/api/v1/ruleset/$RULESET_ID" -ForegroundColor Gray
try {
    Invoke-RestMethod -Uri "$BASE_URL/api/v1/ruleset/$RULESET_ID" | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "3. Getting Active Rules for Product Group..." -ForegroundColor Yellow
Write-Host "GET $BASE_URL/api/v1/rule-evaluation/active-rules/$PRODUCT_GROUP_ID/$COUNTRY_ID" -ForegroundColor Gray
try {
    Invoke-RestMethod -Uri "$BASE_URL/api/v1/rule-evaluation/active-rules/$PRODUCT_GROUP_ID/$COUNTRY_ID" | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "4. Testing Rule Evaluation - Should PASS (width=150, height=150, quantity=200)..." -ForegroundColor Yellow
$body1 = @{
    ruleSetId = $RULESET_ID
    productGroupId = $PRODUCT_GROUP_ID
    countryId = $COUNTRY_ID
    configurationData = '{"width": 150, "height": 150, "quantity": 200}'
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$BASE_URL/api/v1/rule-evaluation/evaluate" -Method Post -Body $body1 -ContentType "application/json" | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "5. Testing Rule Evaluation - Should FAIL spatial rule (width=50, height=50, quantity=200)..." -ForegroundColor Yellow
$body2 = @{
    ruleSetId = $RULESET_ID
    productGroupId = $PRODUCT_GROUP_ID
    countryId = $COUNTRY_ID
    configurationData = '{"width": 50, "height": 50, "quantity": 200}'
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$BASE_URL/api/v1/rule-evaluation/evaluate" -Method Post -Body $body2 -ContentType "application/json" | ConvertTo-Json -Depth 10
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
Write-Host ""

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Test Complete!" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
