$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDirectory "..")

Write-Host "Запуск Observer (http://localhost:5080)..." -ForegroundColor Cyan
$observerProcess = Start-Process dotnet `
    -ArgumentList "run --project src/TaskManagement.Observer/TaskManagement.Observer.csproj --urls http://localhost:5080" `
    -WorkingDirectory $projectRoot `
    -PassThru

Write-Host "Запуск API (http://localhost:5000)..." -ForegroundColor Cyan
Write-Host "Для остановки нажмите Ctrl+C в этом окне." -ForegroundColor Yellow

try {
    dotnet run --project "src/TaskManagement.API/TaskManagement.API.csproj" --urls "http://localhost:5000"
}
finally {
    if ($null -ne $observerProcess -and -not $observerProcess.HasExited) {
        Write-Host "Остановка Observer..." -ForegroundColor Yellow
        Stop-Process -Id $observerProcess.Id
    }
}
