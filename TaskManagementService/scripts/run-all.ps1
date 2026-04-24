$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDirectory "..")
$observerOutLog = Join-Path $projectRoot "logs\observer-stdout.log"
$observerErrLog = Join-Path $projectRoot "logs\observer-stderr.log"
New-Item -ItemType Directory -Path (Join-Path $projectRoot "logs") -Force | Out-Null

Write-Host "Шаг 1/3: запуск тестов..." -ForegroundColor Cyan
dotnet test "$projectRoot\TaskManagementService.slnx"

Write-Host "Шаг 2/3: запуск Observer (http://localhost:5080)..." -ForegroundColor Cyan
$observerProcess = Start-Process dotnet `
    -ArgumentList "run --no-launch-profile --project src/TaskManagement.Observer/TaskManagement.Observer.csproj --urls http://localhost:5080" `
    -WorkingDirectory $projectRoot `
    -RedirectStandardOutput $observerOutLog `
    -RedirectStandardError $observerErrLog `
    -PassThru
Write-Host "Observer PID: $($observerProcess.Id)" -ForegroundColor DarkGray

$observerStarted = $false
for ($i = 0; $i -lt 20; $i++) {
    if ($observerProcess.HasExited) { break }
    $listener = Get-NetTCPConnection -LocalPort 5080 -State Listen -ErrorAction SilentlyContinue
    if ($null -ne $listener) {
        $observerStarted = $true
        break
    }
    Start-Sleep -Milliseconds 500
}

if (-not $observerStarted) {
    Write-Host "Observer не стартовал. Последние логи:" -ForegroundColor Red
    if (Test-Path $observerErrLog) { Get-Content $observerErrLog -Tail 50 }
    if (Test-Path $observerOutLog) { Get-Content $observerOutLog -Tail 50 }
    throw "Observer не удалось запустить на порту 5080."
}

Write-Host "Шаг 3/3: запуск API (http://localhost:5000)..." -ForegroundColor Cyan
Write-Host "Для остановки нажмите Ctrl+C в этом окне." -ForegroundColor Yellow

try {
    dotnet run --no-launch-profile --project "src/TaskManagement.API/TaskManagement.API.csproj" --urls "http://localhost:5000"
}
finally {
    if ($null -ne $observerProcess -and -not $observerProcess.HasExited) {
        Write-Host "Остановка Observer..." -ForegroundColor Yellow
        Stop-Process -Id $observerProcess.Id
    }
}
