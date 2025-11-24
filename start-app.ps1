# Refresh PATH to include .NET SDK
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

# Set environment
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Navigate to project directory
Set-Location "C:\Users\HP\Downloads\WinInventory-20251119T105049Z-1-001\WinInventory"

# Verify .NET is available
Write-Host "Checking .NET SDK..." -ForegroundColor Cyan
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host ".NET SDK Version: $dotnetVersion" -ForegroundColor Green
} else {
    Write-Host "ERROR: .NET SDK not found! Please install .NET 8.0 SDK." -ForegroundColor Red
    pause
    exit 1
}

# Run the application
Write-Host "`nStarting WinInventory application..." -ForegroundColor Cyan
Write-Host "The app will be available at: http://localhost:5127" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the application`n" -ForegroundColor Gray

dotnet run

