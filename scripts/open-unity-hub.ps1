$ErrorActionPreference = "Stop"

$hubCandidates = @(
    "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
    "${env:ProgramFiles(x86)}\Unity Hub\Unity Hub.exe",
    "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe"
) | Where-Object { $_ }

$hub = $hubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $hub) {
    throw "Unity Hub is not installed. Install it from https://unity.com/unity-hub"
}

Start-Process -FilePath $hub
