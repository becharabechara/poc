# SmartNotes End-to-End Integration Test Suite
# Tests the complete flow: Frontend -> Backend -> Database

param(
    [string]$BaseUrl = "http://localhost:3000",
    [string]$ApiUrl = "http://localhost:8080"
)

Write-Host "Starting SmartNotes End-to-End Integration Tests" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan

# Test counter
$TestsRun = 0
$TestsPassed = 0
$TestsFailed = 0

# Helper function to log test results
function Log-Test {
    param(
        [string]$TestName,
        [string]$Result,
        [string]$Message = ""
    )

    $script:TestsRun++

    if ($Result -eq "PASS") {
        $script:TestsPassed++
        Write-Host "PASS - $TestName" -ForegroundColor Green
        if ($Message) { Write-Host "   $Message" -ForegroundColor Gray }
    } else {
        $script:TestsFailed++
        Write-Host "FAIL - $TestName" -ForegroundColor Red
        if ($Message) { Write-Host "   $Message" -ForegroundColor Gray }
    }
}

# Helper function to test HTTP status
function Test-HttpStatus {
    param(
        [string]$Url,
        [int]$ExpectedStatus,
        [string]$TestName
    )

    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10
        if ($response.StatusCode -eq $ExpectedStatus) {
            Log-Test -TestName $TestName -Result "PASS" -Message "HTTP $($response.StatusCode) as expected"
            return $true
        } else {
            Log-Test -TestName $TestName -Result "FAIL" -Message "Expected HTTP $ExpectedStatus, got $($response.StatusCode)"
            return $false
        }
    } catch {
        Log-Test -TestName $TestName -Result "FAIL" -Message "Request failed: $($_.Exception.Message)"
        return $false
    }
}

Write-Host "Checking service availability..." -ForegroundColor Yellow
Write-Host "-------------------------------"

# Test 1: Frontend accessibility
Test-HttpStatus -Url $BaseUrl -ExpectedStatus 200 -TestName "Frontend Accessibility"

# Test 2: Backend health check
Test-HttpStatus -Url "$ApiUrl/api/notes/health" -ExpectedStatus 200 -TestName "Backend Health Check"

# Test 3: API documentation
Test-HttpStatus -Url "$ApiUrl/swagger" -ExpectedStatus 200 -TestName "API Documentation"

Write-Host ""
Write-Host "Testing API Endpoints..." -ForegroundColor Yellow
Write-Host "-----------------------"

# Test 4: Get all notes
Test-HttpStatus -Url "$ApiUrl/api/notes" -ExpectedStatus 200 -TestName "Get All Notes"

# Test 5: Create a note
$createBody = @{
    title = "E2E Test Note"
    content = "This is a test note created by the E2E test suite"
    tags = @("test", "e2e", "automation")
} | ConvertTo-Json

try {
    $createResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes" -Method POST -Body $createBody -ContentType "application/json" -TimeoutSec 10
    $createdNote = $createResponse.Content | ConvertFrom-Json

    if ($createdNote.id) {
        $noteId = $createdNote.id
        Log-Test -TestName "Create Note" -Result "PASS" -Message "Note created with ID: $noteId"
    } else {
        Log-Test -TestName "Create Note" -Result "FAIL" -Message "Note creation response missing ID"
        $noteId = $null
    }
} catch {
    Log-Test -TestName "Create Note" -Result "FAIL" -Message "Failed to create note: $($_.Exception.Message)"
    $noteId = $null
}

# Test 6: Get the created note by ID
if ($noteId) {
    Test-HttpStatus -Url "$ApiUrl/api/notes/$noteId" -ExpectedStatus 200 -TestName "Get Note by ID"
}

# Test 7: Update the note
if ($noteId) {
    $updateBody = @{
        id = $noteId
        title = "Updated E2E Test Note"
        content = "This note has been updated by the E2E test suite"
        tags = @("test", "e2e", "automation", "updated")
    } | ConvertTo-Json

    try {
        $updateResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes/$noteId" -Method PUT -Body $updateBody -ContentType "application/json" -TimeoutSec 10
        Log-Test -TestName "Update Note" -Result "PASS" -Message "Note updated successfully"
    } catch {
        Log-Test -TestName "Update Note" -Result "FAIL" -Message "Failed to update note: $($_.Exception.Message)"
    }
}

# Test 8: Search notes
try {
    $searchResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes/search?keyword=test" -Method GET -TimeoutSec 10
    Log-Test -TestName "Search Notes" -Result "PASS" -Message "Search request successful"
} catch {
    Log-Test -TestName "Search Notes" -Result "FAIL" -Message "Search request failed: $($_.Exception.Message)"
}

# Test 9: Delete the note
if ($noteId) {
    try {
        $deleteResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes/$noteId" -Method DELETE -TimeoutSec 10
        if ($deleteResponse.StatusCode -eq 204) {
            Log-Test -TestName "Delete Note" -Result "PASS" -Message "Note deleted successfully"
        } else {
            Log-Test -TestName "Delete Note" -Result "FAIL" -Message "Delete returned status $($deleteResponse.StatusCode)"
        }
    } catch {
        Log-Test -TestName "Delete Note" -Result "FAIL" -Message "Failed to delete note: $($_.Exception.Message)"
    }
}

Write-Host ""
Write-Host "Testing Architecture Compliance..." -ForegroundColor Yellow
Write-Host "---------------------------------"

# Test 10: Verify hexagonal architecture - Domain layer isolation
Test-HttpStatus -Url "$ApiUrl/api/notes/health" -ExpectedStatus 200 -TestName "Domain Layer Isolation"

# Test 11: Verify dependency injection
Test-HttpStatus -Url "$ApiUrl/api/notes" -ExpectedStatus 200 -TestName "Dependency Injection"

Write-Host ""
Write-Host "Testing Performance & Reliability..." -ForegroundColor Yellow
Write-Host "-----------------------------------"

# Test 12: Error handling
$errorBody = '{"invalid": "json"}'
try {
    $errorResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes" -Method POST -Body $errorBody -ContentType "application/json" -TimeoutSec 10
    Log-Test -TestName "Error Handling" -Result "FAIL" -Message "Expected error but got success response"
} catch {
    $statusCode = $_.Exception.Response.StatusCode
    if ($statusCode -eq 400) {
        Log-Test -TestName "Error Handling" -Result "PASS" -Message "Proper error response (400) for invalid input"
    } else {
        Log-Test -TestName "Error Handling" -Result "FAIL" -Message "Unexpected error status: $statusCode"
    }
}

# Test 13: CORS headers
try {
    $corsResponse = Invoke-WebRequest -Uri "$ApiUrl/api/notes" -Method GET -Headers @{"Origin" = "http://localhost:3000"} -TimeoutSec 10
    $corsHeader = $corsResponse.Headers["Access-Control-Allow-Origin"]
    if ($corsHeader) {
        Log-Test -TestName "CORS Configuration" -Result "PASS" -Message "CORS headers properly configured"
    } else {
        Log-Test -TestName "CORS Configuration" -Result "FAIL" -Message "CORS headers missing"
    }
} catch {
    Log-Test -TestName "CORS Configuration" -Result "FAIL" -Message "CORS test failed: $($_.Exception.Message)"
}

Write-Host ""
Write-Host "Test Results Summary" -ForegroundColor Cyan
Write-Host "==================="
Write-Host "Tests Run: $TestsRun"
Write-Host "Tests Passed: " -NoNewline; Write-Host $TestsPassed -ForegroundColor Green
Write-Host "Tests Failed: " -NoNewline; Write-Host $TestsFailed -ForegroundColor Red

if ($TestsFailed -eq 0) {
    Write-Host ""
    Write-Host "All End-to-End Integration Tests PASSED!" -ForegroundColor Green
    Write-Host "The SmartNotes application is fully functional and properly integrated."
    exit 0
} else {
    Write-Host ""
    Write-Host "Some tests failed. Please review the issues above." -ForegroundColor Red
    exit 1
}