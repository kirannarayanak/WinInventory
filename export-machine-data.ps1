# WinInventory Machine Data Export Script
# This script exports machine information to a CSV file for Mac recommendation

$outputFile = "machine-data.csv"
$data = @()

# Get Computer System Info
$cs = Get-WmiObject -Class Win32_ComputerSystem
$computerName = $cs.Name
$manufacturer = $cs.Manufacturer
$model = $cs.Model
$totalMemoryGB = [math]::Round($cs.TotalPhysicalMemory / 1GB, 2)

# Get OS Info
$os = Get-WmiObject -Class Win32_OperatingSystem
$osName = $os.Caption
$osVersion = $os.Version
$buildNumber = $os.BuildNumber

# Get Processor Info
$cpu = Get-WmiObject -Class Win32_Processor | Select-Object -First 1
$processor = $cpu.Name
$physicalCores = $cpu.NumberOfCores
$logicalCores = $cpu.NumberOfLogicalProcessors

# Get Disk Info
$disks = Get-WmiObject -Class Win32_LogicalDisk -Filter "DriveType=3"
$diskInfo = ""
foreach ($disk in $disks) {
    $sizeGB = [math]::Round($disk.Size / 1GB, 2)
    $freeGB = [math]::Round($disk.FreeSpace / 1GB, 2)
    $diskInfo += "$($disk.DeviceID) $($disk.FileSystem) ${sizeGB}GB ${freeGB}GB;"
}

# Get Installed Applications
$apps = @()
$uninstallKeys = @(
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

foreach ($key in $uninstallKeys) {
    $items = Get-ItemProperty $key -ErrorAction SilentlyContinue
    foreach ($item in $items) {
        if ($item.DisplayName -and !$item.SystemComponent -and !$item.ReleaseType) {
            $apps += $item.DisplayName
        }
    }
}

$appsList = $apps -join ";"

# Create CSV row
$row = [PSCustomObject]@{
    ComputerName = $computerName
    Manufacturer = $manufacturer
    Model = $model
    OSName = $osName
    OSVersion = $osVersion
    BuildNumber = $buildNumber
    Processor = $processor
    PhysicalCores = $physicalCores
    LogicalCores = $logicalCores
    TotalMemoryGB = $totalMemoryGB
    Disks = $diskInfo.TrimEnd(';')
    Applications = $appsList
}

# Export to CSV
$row | Export-Csv -Path $outputFile -NoTypeInformation -Encoding UTF8

Write-Host "Machine data exported to: $outputFile" -ForegroundColor Green
Write-Host "File location: $(Resolve-Path $outputFile)" -ForegroundColor Cyan

