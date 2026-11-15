param(
    [Parameter(Mandatory)] [string] $ZipPath,
    [Parameter(Mandatory)] [string] $Environment,          # UAT or Production
    [string] $ServiceName = "ProvisioningWorker",
    [string] $InstallRoot = "C:\source\repos\iam-provisioning-demo\Services\ProvisioningWorker"
)
$ErrorActionPreference = "Stop"
$current = Join-Path $InstallRoot "current"
$backup  = Join-Path $InstallRoot ("backup-" + (Get-Date -Format "yyyyMMddHHmmss"))
New-Item -ItemType Directory -Force -Path $InstallRoot | Out-Null

# 1. Stop the service if it exists
$svc = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($svc -and $svc.Status -ne 'Stopped') {
    Stop-Service $ServiceName -Force
    $svc.WaitForStatus('Stopped', '00:00:30')
}

# 2. Back up the current version
if (Test-Path $current) { Move-Item $current $backup }

# 3. Unzip the new version
Expand-Archive -Path $ZipPath -DestinationPath $current -Force
$exe = Join-Path $current "Provisioning.Worker.exe"

# 4. Create the service the first time (environment passed as an argument)
if (-not $svc) {
    New-Service -Name $ServiceName -BinaryPathName "`"$exe`" --environment $Environment" `
                -StartupType Automatic | Out-Null
}

# 5. Start and verify
Start-Service $ServiceName
Start-Sleep -Seconds 10
$status = (Get-Service $ServiceName).Status
Write-Host "Service status: $status"

# 6. Roll back if it didn't stay running
if ($status -ne 'Running') {
    Write-Warning "Deployment failed. Rolling back."
    Stop-Service $ServiceName -Force -ErrorAction SilentlyContinue
    Remove-Item $current -Recurse -Force
    if (Test-Path $backup) { Move-Item $backup $current; Start-Service $ServiceName }
    throw "Deployment of $ZipPath to $Environment failed and was rolled back."
}
Write-Host "Deployed $ZipPath to $Environment successfully."
