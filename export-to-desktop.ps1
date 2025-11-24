# WinInventory Machine Data Export Script
# Exports all machine data needed for Mac recommendation to Desktop

$desktopPath = [Environment]::GetFolderPath("Desktop")
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$outputFile = Join-Path $desktopPath "machine-data_$timestamp.csv"

Write-Host "Collecting machine data..." -ForegroundColor Cyan

# --- Computer System Info ---
$cs = Get-WmiObject -Class Win32_ComputerSystem
$computerName = $cs.Name
$manufacturer = $cs.Manufacturer
$model = $cs.Model
$totalMemoryBytes = $cs.TotalPhysicalMemory
$totalMemoryGB = [math]::Round($totalMemoryBytes / 1GB, 2)

# --- OS Info ---
$os = Get-WmiObject -Class Win32_OperatingSystem
$osName = $os.Caption
$osVersion = $os.Version
$buildNumber = $os.BuildNumber

# --- Processor Info ---
$cpu = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
$processor = $cpu.Name
$physicalCores = $cpu.NumberOfCores
$logicalCores = $cpu.NumberOfLogicalProcessors

# --- Disk Info ---
$disks = Get-WmiObject -Class Win32_LogicalDisk -Filter "DriveType=3"
$diskInfo = @()
foreach ($disk in $disks) {
    $sizeGB = [math]::Round($disk.Size / 1GB, 2)
    $freeGB = [math]::Round($disk.FreeSpace / 1GB, 2)
    $diskInfo += "$($disk.DeviceID) $($disk.FileSystem) ${sizeGB}GB ${freeGB}GB"
}
$disksString = $diskInfo -join ";"

# --- Installed Applications ---
Write-Host "Collecting installed applications..." -ForegroundColor Yellow
$apps = @()
$uninstallKeys = @(
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

foreach ($key in $uninstallKeys) {
    try {
        $items = Get-ItemProperty $key -ErrorAction SilentlyContinue
        foreach ($item in $items) {
            if ($item.DisplayName -and !$item.SystemComponent -and !$item.ReleaseType) {
                $apps += $item.DisplayName
            }
        }
    } catch {}
}

$apps = $apps | Sort-Object -Unique
$appsList = $apps -join ";"

# --- Build CSV ---
$csvContent = @"
ComputerName,Manufacturer,Model,OSName,OSVersion,BuildNumber,Processor,PhysicalCores,LogicalCores,TotalMemoryGB,Disks,Applications
"$computerName","$manufacturer","$model","$osName","$osVersion","$buildNumber","$processor",$physicalCores,$logicalCores,$totalMemoryGB,"$disksString","$appsList"
"@

# --- Save File ---
$csvContent | Out-File -FilePath $outputFile -Encoding UTF8 -NoNewline

Write-Host "`nMachine data exported successfully!" -ForegroundColor Green
Write-Host "Saved to: $outputFile" -ForegroundColor Cyan
Write-Host "Applications found: $($apps.Count)" -ForegroundColor Yellow

