$ErrorActionPreference = "Stop"

$appDataRoot = "D:\Tuanjie\AppData"
$roamingRoot = Join-Path $appDataRoot "Roaming"
$localRoot = Join-Path $appDataRoot "Local"
$tempRoot = "D:\Temp"
$hubDataRoot = Join-Path $roamingRoot "TuanjieHub"
$installRoot = "D:\Tuanjie\Hub\Editor"
$downloadRoot = "D:\Tuanjie\Hub\Downloads"

New-Item -ItemType Directory -Force -Path $roamingRoot, $localRoot, $tempRoot, $hubDataRoot, $installRoot, $downloadRoot | Out-Null

# Keep Tuanjie Hub's bulky user data, downloads, and temp files on D drive.
$env:APPDATA = $roamingRoot
$env:LOCALAPPDATA = $localRoot
$env:TEMP = $tempRoot
$env:TMP = $tempRoot

Set-Content -LiteralPath (Join-Path $hubDataRoot "secondaryDownloadLocation.json") -Value (@{ path = $downloadRoot } | ConvertTo-Json -Compress)
Set-Content -LiteralPath (Join-Path $hubDataRoot "secondaryInstallPath.json") -Value ($installRoot | ConvertTo-Json -Compress)

$hubCandidates = @(
    "D:\Tuanjie\Hub\App\Tuanjie Hub.exe",
    "$env:ProgramFiles\Tuanjie Hub\Tuanjie Hub.exe",
    "${env:ProgramFiles(x86)}\Tuanjie Hub\Tuanjie Hub.exe",
    "$env:LOCALAPPDATA\Programs\Tuanjie Hub\Tuanjie Hub.exe"
) | Where-Object { $_ }

$hub = $hubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $hub) {
    throw "Tuanjie Hub is not installed. Install it from https://unity.cn/tuanjie/releases"
}

Start-Process -FilePath $hub
