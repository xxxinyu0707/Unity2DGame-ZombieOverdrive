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

$editorCandidates = @(
    "$env:ProgramFiles\Unity\Hub\Editor",
    "D:\Unity\Hub\Editor"
) | Where-Object { $_ }

$hubFound = $hubCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
$editorFound = $editorCandidates |
    Where-Object { Test-Path $_ } |
    ForEach-Object { Get-ChildItem $_ -Directory -ErrorAction SilentlyContinue } |
    Select-Object -First 1

$checks += [PSCustomObject]@{
    Tool = "Unity Hub app"
    Status = if ($hubFound) { "OK" } else { "Missing" }
    Detail = if ($hubFound) { $hubFound } else { "Install Unity Hub from https://unity.com/unity-hub" }
}

$checks += [PSCustomObject]@{
    Tool = "Unity Editor"
    Status = if ($editorFound) { "OK" } else { "Missing" }
    Detail = if ($editorFound) { $editorFound.FullName } else { "Install Unity 6.3 LTS in Unity Hub" }
}

$checks += Test-PathStatus "Unity project root" $gameRoot
$checks += Test-PathStatus "Assets folder" (Join-Path $gameRoot "Assets")
$checks += Test-PathStatus "Packages folder" (Join-Path $gameRoot "Packages")
$checks += Test-PathStatus "ProjectSettings folder" (Join-Path $gameRoot "ProjectSettings")

$checks | Format-Table -AutoSize

$missing = $checks | Where-Object { $_.Status -ne "OK" }
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
