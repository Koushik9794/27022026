# Convert part master CSV to JSON
# The CSV file contains a JSON string that needs to be extracted

$csvPath = "data-1770203456209__.csv"
$jsonPath = "part_master_full.json"

Write-Host "Reading CSV file: $csvPath"
$content = Get-Content $csvPath -Raw

# Remove the CSV header line "jsonb_pretty"
$content = $content -replace '^"jsonb_pretty"\r?\n', ''

# Remove the opening quote and bracket at the start
$content = $content -replace '^"\[', '['

# Remove the closing bracket and quote at the end, handling potential whitespace
$content = $content -replace '\]"\s*$', ']'

# Replace escaped double quotes with regular quotes
$content = $content -replace '""', '"'

Write-Host "Saving JSON to: $jsonPath"
$content | Set-Content $jsonPath -NoNewline

Write-Host "Conversion complete!"
Write-Host "Validating JSON..."

try {
    $json = Get-Content $jsonPath -Raw | ConvertFrom-Json
    Write-Host "✓ JSON is valid!"
    Write-Host "✓ Total parts: $($json.Count)"
} catch {
    Write-Host "✗ JSON validation failed: $_"
    exit 1
}
