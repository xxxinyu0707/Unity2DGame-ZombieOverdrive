param(
    [string]$ExePath,
    [switch]$Headless
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$logsPath = Join-Path $repoRoot "logs"

if (-not $ExePath) {
    $ExePath = Join-Path $repoRoot "Builds\Windows\ZombieOverdrive.exe"
}

if (-not (Test-Path $ExePath)) {
    throw "Build executable not found: $ExePath"
}

$dataPath = Join-Path (Split-Path -Parent $ExePath) "ZombieOverdrive_Data"
if (-not (Test-Path $dataPath)) {
    throw "Build data folder not found: $dataPath"
}

New-Item -ItemType Directory -Force -Path $logsPath | Out-Null
$playerLog = Join-Path $logsPath "player-smoke.log"

Write-Host "Launching build for a short smoke test..."
Write-Host "Exe: $ExePath"
Write-Host "Log: $playerLog"

$playerArguments = @("-logFile", $playerLog)
if ($Headless) {
    $playerArguments += @("-batchmode", "-nographics")
}
else {
    $playerArguments += @("-screen-width", "1280", "-screen-height", "720", "-screen-fullscreen", "0")
}

$process = Start-Process `
    -FilePath $ExePath `
    -ArgumentList $playerArguments `
    -WindowStyle Hidden `
    -PassThru

Start-Sleep -Seconds 10

if ($process.HasExited) {
    if ($process.ExitCode -ne 0) {
        Write-Host "Player exited early with code $($process.ExitCode)." -ForegroundColor Red
        if (Test-Path $playerLog) {
            Get-Content $playerLog -Tail 80
        }
        exit $process.ExitCode
    }

    Write-Host "Player exited cleanly during smoke test." -ForegroundColor Green
}
else {
    Stop-Process -Id $process.Id -Force
    Write-Host "Player stayed alive for 10 seconds; smoke test passed." -ForegroundColor Green
}
