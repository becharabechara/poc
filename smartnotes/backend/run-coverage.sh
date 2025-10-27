#!/bin/bash
# Clean Coverage Report Script for SmartNotes
# This script runs tests and generates coverage excluding auto-generated files

OUTPUT_PATH="./coveragereport-clean"
OPEN_REPORT=false

echo "🧪 SmartNotes Clean Coverage Report"
echo "================================="

# Parse arguments
while [[ $# -gt 0 ]]; do
  case $1 in
    --output)
      OUTPUT_PATH="$2"
      shift
      shift
      ;;
    --open)
      OPEN_REPORT=true
      shift
      ;;
    *)
      echo "Unknown option: $1"
      exit 1
      ;;
  esac
done

# Clean previous results
if [ -d "$OUTPUT_PATH" ]; then
    rm -rf "$OUTPUT_PATH"
    echo "🧹 Cleaned previous coverage results"
fi

# Clean previous test results
find . -name "TestResults" -type d -exec rm -rf {} + 2>/dev/null || true

echo "🔍 Running tests with coverage exclusions..."

# Run tests with exclusions
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory TestResults \
  /p:CollectCoverage=true \
  /p:ExcludeByFile="**/Migrations/**" \
  /p:Exclude="[*]*Migrations*"

if [ $? -ne 0 ]; then
    echo "❌ Tests failed! Check the output above."
    exit 1
fi

echo "✅ Tests completed successfully"

# Find coverage files
COVERAGE_FILES=$(find . -name "coverage.cobertura.xml" -type f)

if [ -z "$COVERAGE_FILES" ]; then
    echo "❌ No coverage files found!"
    exit 1
fi

echo "📊 Found coverage files"

# Generate reports
echo "📈 Generating coverage reports..."

reportgenerator \
  -reports:"**/*coverage.cobertura.xml" \
  -targetdir:"$OUTPUT_PATH" \
  -reporttypes:"Html;TextSummary"

if [ $? -ne 0 ]; then
    echo "❌ Report generation failed!"
    exit 1
fi

# Display summary
if [ -f "$OUTPUT_PATH/Summary.txt" ]; then
    echo ""
    echo "📋 Coverage Summary:"
    echo "===================="
    head -20 "$OUTPUT_PATH/Summary.txt"
    
    # Extract key metrics
    LINE_COVERAGE=$(grep "Line coverage:" "$OUTPUT_PATH/Summary.txt" | sed 's/.*Line coverage: //')
    BRANCH_COVERAGE=$(grep "Branch coverage:" "$OUTPUT_PATH/Summary.txt" | sed 's/.*Branch coverage: //')
    
    echo ""
    echo "🎯 Key Metrics (Excluding Auto-Generated Files):"
    echo "  📏 Line Coverage: $LINE_COVERAGE"
    echo "  🌿 Branch Coverage: $BRANCH_COVERAGE"
fi

echo ""
echo "✨ Coverage report generated at: $OUTPUT_PATH"

if [ "$OPEN_REPORT" = true ] && [ -f "$OUTPUT_PATH/index.html" ]; then
    echo "🌐 Opening coverage report in browser..."
    if command -v xdg-open > /dev/null; then
        xdg-open "$OUTPUT_PATH/index.html"
    elif command -v open > /dev/null; then
        open "$OUTPUT_PATH/index.html"
    else
        echo "Please open $OUTPUT_PATH/index.html manually"
    fi
fi

echo ""
echo "🎉 Coverage analysis complete!"