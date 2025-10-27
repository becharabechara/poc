# Clean Coverage Report Script
# This script runs tests and generates coverage excluding auto-generated files

param(
    [string]$OutputPath = ".\coveragereport",
    [switch]$OpenReport = $false
)

Write-Host "🧪 SmartNotes Clean Coverage Report" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Clean previous results
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "🧹 Cleaned previous coverage results" -ForegroundColor Yellow
}

# Clean previous test results
Get-ChildItem -Path . -Recurse -Directory -Name "TestResults" | ForEach-Object {
    Remove-Item $_ -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "🔍 Running tests with coverage exclusions..." -ForegroundColor Green

# Run tests with the custom coverage settings
$testResult = dotnet test --settings coverlet.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Tests failed! Check the output above." -ForegroundColor Red
    exit 1
}

Write-Host "✅ Tests completed successfully" -ForegroundColor Green

# Find coverage files
$coverageFiles = Get-ChildItem -Path . -Recurse -Filter "coverage.cobertura.xml" | Select-Object -ExpandProperty FullName

if ($coverageFiles.Count -eq 0) {
    Write-Host "❌ No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host "📊 Found $($coverageFiles.Count) coverage files" -ForegroundColor Green

# Generate reports
Write-Host "📈 Generating coverage reports..." -ForegroundColor Green

# Generate text summary
$reportsParam = ($coverageFiles -join ";")
& reportgenerator -reports:$reportsParam -targetdir:$OutputPath -reporttypes:"Html;TextSummary"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Report generation failed!" -ForegroundColor Red
    exit 1
}

# Display summary
if (Test-Path "$OutputPath\Summary.txt") {
    Write-Host "`n📋 Coverage Summary:" -ForegroundColor Cyan
    Write-Host "====================" -ForegroundColor Cyan
    Get-Content "$OutputPath\Summary.txt" | Select-Object -First 20
    
    # Extract key metrics
    $content = Get-Content "$OutputPath\Summary.txt"
    $lineCoverage = ($content | Where-Object { $_ -match "Line coverage:" }) -replace ".*Line coverage: ", ""
    $branchCoverage = ($content | Where-Object { $_ -match "Branch coverage:" }) -replace ".*Branch coverage: ", ""
    
    Write-Host "`n🎯 Key Metrics (Excluding Auto-Generated Files):" -ForegroundColor Green
    Write-Host "  📏 Line Coverage: $lineCoverage" -ForegroundColor White
    Write-Host "  🌿 Branch Coverage: $branchCoverage" -ForegroundColor White
}

Write-Host "`n✨ Coverage report generated at: $OutputPath" -ForegroundColor Green

if ($OpenReport -and (Test-Path "$OutputPath\index.html")) {
    Write-Host "🌐 Opening coverage report in browser..." -ForegroundColor Yellow
    Start-Process "$OutputPath\index.html"
}

Write-Host "`n🎉 Coverage analysis complete!" -ForegroundColor Cyan