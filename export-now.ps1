# Quick Export Script
$desktop = [Environment]::GetFolderPath("Desktop")
$timestamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$outputFile = Join-Path $desktop "machine-data_$timestamp.csv"

Write-Host "Collecting machine data..." -ForegroundColor Cyan

$cs = Get-WmiObject Win32_ComputerSystem
$os = Get-WmiObject Win32_OperatingSystem
$cpu = Get-WmiObject Win32_Processor | Select-Object -First 1
$memoryGB = [math]::Round($cs.TotalPhysicalMemory / 1GB, 2)

$diskList = @()
Get-WmiObject Win32_LogicalDisk -Filter "DriveType=3" | ForEach-Object {
    $sizeGB = [math]::Round($_.Size / 1GB, 2)
    $freeGB = [math]::Round($_.FreeSpace / 1GB, 2)
    $diskList += "$($_.DeviceID) $($_.FileSystem) ${sizeGB}GB ${freeGB}GB"
}
$disks = $diskList -join ";"

Write-Host "Collecting installed applications..." -ForegroundColor Yellow
$apps = @()
$keys = @(
    "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*",
    "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*"
)

foreach ($key in $keys) {
    try {
        Get-ItemProperty $key -ErrorAction SilentlyContinue | Where-Object {
            $_.DisplayName -and !$_.SystemComponent -and !$_.ReleaseType
        } | ForEach-Object {
            $apps += $_.DisplayName
        }
    } catch {}
}

$apps = ($apps | Sort-Object -Unique) -join ";"

$csv = "ComputerName,Manufacturer,Model,OSName,OSVersion,BuildNumber,Processor,PhysicalCores,LogicalCores,TotalMemoryGB,Disks,Applications`r`n"
$csv += '"' + $cs.Name + '","' + $cs.Manufacturer + '","' + $cs.Model + '","' + $os.Caption + '","' + $os.Version + '","' + $os.BuildNumber + '","' + $cpu.Name + '",' + $cpu.NumberOfCores + ',' + $cpu.NumberOfLogicalProcessors + ',' + $memoryGB + ',"' + $disks + '","' + $apps + '"'

$csv | Out-File -FilePath $outputFile -Encoding UTF8 -NoNewline

Write-Host ""
Write-Host "SUCCESS! File saved to:" -ForegroundColor Green
Write-Host $outputFile -ForegroundColor Cyan
Write-Host ""
Write-Host "Summary:" -ForegroundColor Yellow
Write-Host "  Computer: $($cs.Name)"
Write-Host "  Model: $($cs.Model)"
Write-Host "  Processor: $($cpu.Name)"
Write-Host "  Memory: $memoryGB GB"
$appCount = if ($apps) { ($apps.Split(';')).Count } else { 0 }
Write-Host "  Applications: $appCount found"
Write-Host ""
Write-Host "Next: Open http://localhost:7000 and click Import Data" -ForegroundColor Green
