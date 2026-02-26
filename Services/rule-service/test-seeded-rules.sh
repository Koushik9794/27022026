#!/bin/bash
# Test script to verify seeded rules are working

BASE_URL="http://localhost:5001"

echo "========================================="
echo "Rule Service - Seeded Data Test"
echo "========================================="
echo ""

# Seeded IDs from migration
PRODUCT_GROUP_ID="11111111-1111-1111-1111-111111111111"
COUNTRY_ID="22222222-2222-2222-2222-222222222222"
RULESET_ID="33333333-3333-3333-3333-333333333333"

echo "1. Testing Health Endpoint..."
curl -s "$BASE_URL/health" | jq '.'
echo ""

echo "2. Getting RuleSet by ID..."
echo "GET $BASE_URL/api/v1/ruleset/$RULESET_ID"
curl -s "$BASE_URL/api/v1/ruleset/$RULESET_ID" | jq '.'
echo ""

echo "3. Getting Active Rules for Product Group..."
echo "GET $BASE_URL/api/v1/rule-evaluation/active-rules/$PRODUCT_GROUP_ID/$COUNTRY_ID"
curl -s "$BASE_URL/api/v1/rule-evaluation/active-rules/$PRODUCT_GROUP_ID/$COUNTRY_ID" | jq '.'
echo ""

echo "4. Testing Rule Evaluation - Should PASS (width=150, height=150, quantity=200)..."
curl -s -X POST "$BASE_URL/api/v1/rule-evaluation/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "ruleSetId": "'"$RULESET_ID"'",
    "productGroupId": "'"$PRODUCT_GROUP_ID"'",
    "countryId": "'"$COUNTRY_ID"'",
    "configurationData": "{\"width\": 150, \"height\": 150, \"quantity\": 200}"
  }' | jq '.'
echo ""

echo "5. Testing Rule Evaluation - Should FAIL spatial rule (width=50, height=50, quantity=200)..."
curl -s -X POST "$BASE_URL/api/v1/rule-evaluation/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "ruleSetId": "'"$RULESET_ID"'",
    "productGroupId": "'"$PRODUCT_GROUP_ID"'",
    "countryId": "'"$COUNTRY_ID"'",
    "configurationData": "{\"width\": 50, \"height\": 50, \"quantity\": 200}"
  }' | jq '.'
echo ""

echo "========================================="
echo "Test Complete!"
echo "========================================="
