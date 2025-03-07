# PowerShell script to run the SQL test users script against the database
# This script assumes that SQL Server LocalDB is installed and the database exists

# Get the directory of this script
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# SQL script path
$sqlScriptPath = Join-Path $scriptDir "CreateTestUsers.sql"

# Database connection string
$connectionString = "Server=(localdb)\MSSQLLocalDB;Database=shacab;Trusted_Connection=True;"

# Check if the SQL script exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Error "SQL script not found at: $sqlScriptPath"
    exit 1
}

Write-Host "Running SQL script to create/update test users..."

# Run the SQL script using sqlcmd
try {
    # Using sqlcmd to execute the script
    sqlcmd -S "(localdb)\MSSQLLocalDB" -d "shacab" -i $sqlScriptPath -E
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Test users creation/update completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Error executing SQL script. Exit code: $LASTEXITCODE" -ForegroundColor Red
    }
} catch {
    Write-Host "Error executing SQL script: $_" -ForegroundColor Red
}

Write-Host "Done." 