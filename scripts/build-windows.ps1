param(
    [string]$UnityExe = "D:\Tuanjie\Hub\Editor\2022.3.62f3c1\Editor\Unity.exe",
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "Game"
$logsPath = Join-Path $repoRoot "logs"

if (-not $OutputPath) {
    $OutputPath = Join-Path $repoRoot "Builds\Windows\ZombieOverdrive.exe"
}

if (-not (Test-Path $UnityExe)) {
    throw "Tuanjie/Unity editor not found: $UnityExe"
}

if (-not (Test-Path $projectPath)) {
    throw "Unity project not found: $projectPath"
}

New-Item -ItemType Directory -Force -Path $logsPath | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $OutputPath) | Out-Null
New-Item -ItemType Directory -Force -Path "D:\Tuanjie\AppData\Roaming" | Out-Null
New-Item -ItemType Directory -Force -Path "D:\Tuanjie\AppData\Local" | Out-Null
New-Item -ItemType Directory -Force -Path "D:\Tuanjie\Temp" | Out-Null

$env:APPDATA = "D:\Tuanjie\AppData\Roaming"
$env:LOCALAPPDATA = "D:\Tuanjie\AppData\Local"
$env:TEMP = "D:\Tuanjie\Temp"
$env:TMP = "D:\Tuanjie\Temp"

$logFile = Join-Path $logsPath "build-windows.log"
$arguments = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectPath,
    "-executeMethod", "ZombieOverdriveBuildTools.BuildWindows64",
    "-buildOutput", $OutputPath,
    "-logFile", $logFile
)

Write-Host "Building Windows player..."
Write-Host "Editor: $UnityExe"
Write-Host "Output: $OutputPath"
Write-Host "Log: $logFile"

$process = Start-Process `
    -FilePath $UnityExe `
    -ArgumentList $arguments `
    -WindowStyle Hidden `
    -Wait `
    -PassThru

if ($process.ExitCode -ne 0) {
    Write-Host ""
    Write-Host "Build failed. Last log lines:" -ForegroundColor Red
    if (Test-Path $logFile) {
        Get-Content $logFile -Tail 80
    }
    exit $process.ExitCode
}

if (-not (Test-Path $OutputPath)) {
    throw "Build command finished, but executable was not created: $OutputPath"
}

Write-Host ""
Write-Host "Windows build created:" -ForegroundColor Green
Write-Host $OutputPath
