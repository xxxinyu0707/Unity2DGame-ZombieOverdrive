$ErrorActionPreference = "SilentlyContinue"

function Test-Command {
    param([string]$Name)

    $cmd = Get-Command $Name -ErrorAction SilentlyContinue
    if ($cmd) {
        [PSCustomObject]@{
            Tool = $Name
            Status = "OK"
            Detail = $cmd.Source
        }
    }
    else {
        [PSCustomObject]@{
            Tool = $Name
            Status = "Missing"
            Detail = ""
        }
    }
}

function Test-PathStatus {
    param(
        [string]$Name,
        [string]$Path
    )

    [PSCustomObject]@{
        Tool = $Name
        Status = if (Test-Path $Path) { "OK" } else { "Missing" }
        Detail = $Path
    }
}

$repoRoot = Split-Path -Parent $PSScriptRoot
$gameRoot = Join-Path $repoRoot "Game"
$tuanjieRoot = "D:\Tuanjie"
$tuanjieEditorRoot = "D:\Tuanjie\Hub\Editor"
$tuanjieDownloadRoot = "D:\Tuanjie\Hub\Downloads"
$tuanjieAppDataRoot = "D:\Tuanjie\AppData"
$tempRoot = "D:\Temp"

$checks = @()
$checks += Test-Command "git"
$checks += Test-Command "git-lfs"
$checks += Test-Command "dotnet"
$checks += Test-Command "code"

$hubCandidates = @(
    "$env:ProgramFiles\Unity Hub\Unity Hub.exe",
    "${env:ProgramFiles(x86)}\Unity Hub\Unity Hub.exe",
    "$env:LOCALAPPDATA\Programs\Unity Hub\Unity Hub.exe"
) | Where-Object { $_ }

$tuanjieHubCandidates = @(
    "D:\Tuanjie\Hub\App\Tuanjie Hub.exe",
    "$env:ProgramFiles\Tuanjie Hub\Tuanjie Hub.exe",
    "${env:ProgramFiles(x86)}\Tuanjie Hub\Tuanjie Hub.exe",
    "$env:LOCALAPPDATA\Programs\Tuanjie Hub\Tuanjie Hub.exe"
) | Where-Object { $_ }

$editorCandidates = @(
    "D:\Tuanjie\Hub\Editor",
    "D:\Unity\Hub\Editor",
    "$env:ProgramFiles\Tuanjie\Hub\Editor",
    "$env:ProgramFiles\Unity\Hub\Editor"
) | Where-Object { $_ }

$hubFound = $hubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$tuanjieHubFound = $tuanjieHubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$editorFound = $editorCandidates |
    Where-Object { Test-Path $_ } |
    ForEach-Object {
        Get-ChildItem $_ -Directory -ErrorAction SilentlyContinue |
            Where-Object { Test-Path (Join-Path $_.FullName "Editor\Unity.exe") }
    } |
    Select-Object -First 1

$checks += [PSCustomObject]@{
    Tool = "Tuanjie Hub app"
    Status = if ($tuanjieHubFound) { "OK" } else { "Missing" }
    Detail = if ($tuanjieHubFound) { $tuanjieHubFound } else { "Install Tuanjie Hub from https://unity.cn/tuanjie/releases" }
}

$checks += [PSCustomObject]@{
    Tool = "Unity Hub app"
    Status = if ($hubFound) { "OK" } else { "Optional" }
    Detail = if ($hubFound) { $hubFound } else { "Optional fallback; uninstalled to save C drive space" }
}

$checks += [PSCustomObject]@{
    Tool = "Tuanjie/Unity Editor"
    Status = if ($editorFound) { "OK" } else { "Missing" }
    Detail = if ($editorFound) { $editorFound.FullName } else { "Install Tuanjie Engine in Tuanjie Hub" }
}

$checks += Test-PathStatus "Unity project root" $gameRoot
$checks += Test-PathStatus "Assets folder" (Join-Path $gameRoot "Assets")
$checks += Test-PathStatus "Packages folder" (Join-Path $gameRoot "Packages")
$checks += Test-PathStatus "ProjectSettings folder" (Join-Path $gameRoot "ProjectSettings")
$checks += Test-PathStatus "D drive Tuanjie root" $tuanjieRoot
$checks += Test-PathStatus "D drive editor path" $tuanjieEditorRoot
$checks += Test-PathStatus "D drive downloads path" $tuanjieDownloadRoot
$checks += Test-PathStatus "D drive Hub appdata" $tuanjieAppDataRoot
$checks += Test-PathStatus "D drive temp path" $tempRoot

$checks | Format-Table -AutoSize

$missing = $checks | Where-Object { $_.Status -eq "Missing" }
if ($missing) {
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    $missing | ForEach-Object {
        Write-Host "- $($_.Tool): $($_.Detail)"
    }
    exit 1
}

Write-Host ""
Write-Host "Environment looks ready." -ForegroundColor Green
