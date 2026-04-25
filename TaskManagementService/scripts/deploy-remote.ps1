param(
    [Parameter(Mandatory = $true)]
    [string]$ServerIp,

    [string]$ServerUser = "root",
    [string]$SourcePath = "F:\Projects\TestTask\TaskManagementService",
    [string]$RemotePath = "/opt/TaskManagementService"
)

$ErrorActionPreference = "Stop"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
    & $Action
}

if (-not (Test-Path $SourcePath)) {
    throw "Source path not found: $SourcePath"
}

$sourcePathFull = (Resolve-Path $SourcePath).Path
$workspaceRoot = Split-Path -Path $sourcePathFull -Parent
$deployCopyPath = Join-Path $workspaceRoot "TaskManagementService_deploy"
$remoteTarget = "$ServerUser@$ServerIp"
$remoteTemp = "/opt/TaskManagementService_deploy"

Invoke-Step -Message "Preparing clean deploy copy" -Action {
    if (Test-Path $deployCopyPath) {
        Remove-Item $deployCopyPath -Recurse -Force
    }

    $null = New-Item -ItemType Directory -Path $deployCopyPath

    robocopy $sourcePathFull $deployCopyPath /E /R:1 /W:1 /NFL /NDL /NP `
        /XD .git .vs bin obj logs `
        /XF *.user *.suo *.log

    if ($LASTEXITCODE -gt 7) {
        throw "robocopy failed with exit code $LASTEXITCODE"
    }
}

Invoke-Step -Message "Removing old temporary folder on remote host" -Action {
    ssh $remoteTarget "rm -rf $remoteTemp"
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to remove remote temp folder."
    }
}

Invoke-Step -Message "Uploading project via scp" -Action {
    scp -r $deployCopyPath "$remoteTarget`:/opt/"
    if ($LASTEXITCODE -ne 0) {
        throw "scp upload failed."
    }
}

Invoke-Step -Message "Deploying and starting containers on remote host" -Action {
    $remoteDeployScript = @"
set -e
rm -rf "$RemotePath"
mv "$remoteTemp" "$RemotePath"
cd "$RemotePath"
docker compose down --remove-orphans || true
docker compose up -d --build
docker compose ps
"@

    ssh $remoteTarget $remoteDeployScript
    if ($LASTEXITCODE -ne 0) {
        throw "Remote deploy command failed."
    }
}

Invoke-Step -Message "Cleaning local deploy copy" -Action {
    if (Test-Path $deployCopyPath) {
        Remove-Item $deployCopyPath -Recurse -Force
    }
}

Write-Host "`nDeploy completed successfully." -ForegroundColor Green
Write-Host "Swagger: http://$ServerIp`:5000/swagger"
Write-Host "RabbitMQ UI: http://$ServerIp`:15672"
Write-Host "Jaeger UI: http://$ServerIp`:16686"
