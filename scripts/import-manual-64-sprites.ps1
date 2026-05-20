param(
    [string]$InputRoot = "ManualCutoutPack64",
    [string]$OutputRoot = "Game/Assets/Art/Generated"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$inputRootPath = (Resolve-Path -LiteralPath $InputRoot).Path
$outputRootPath = (Resolve-Path -LiteralPath $OutputRoot).Path

$names = @("player", "walker", "runner", "spitter", "tanker", "boss", "final_boss")

function New-ClearBitmap {
    param([int]$Width, [int]$Height)
    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.Dispose()
    return $bitmap
}

function Save-DamageVariant {
    param(
        [System.Drawing.Bitmap]$Base,
        [string]$Name,
        [double]$BloodAmount,
        [double]$Darken
    )

    $variant = New-ClearBitmap $Base.Width $Base.Height
    $blood = [System.Drawing.Color]::FromArgb(255, 152, 20, 24)

    for ($y = 0; $y -lt $Base.Height; $y++) {
        for ($x = 0; $x -lt $Base.Width; $x++) {
            $color = $Base.GetPixel($x, $y)
            if ($color.A -eq 0) {
                continue
            }

            $r = [int][Math]::Round($color.R * $Darken)
            $g = [int][Math]::Round($color.G * $Darken)
            $b = [int][Math]::Round($color.B * $Darken)
            $noise = (($x * 37 + $y * 59 + ($x -band 7) * 11) % 100) / 100.0
            if ($noise -lt $BloodAmount -and $y -gt 14 -and $x -gt 8 -and $x -lt 56) {
                $r = [int][Math]::Round($r * 0.45 + $blood.R * 0.55)
                $g = [int][Math]::Round($g * 0.45 + $blood.G * 0.55)
                $b = [int][Math]::Round($b * 0.45 + $blood.B * 0.55)
            }

            $variant.SetPixel($x, $y, [System.Drawing.Color]::FromArgb($color.A, [Math]::Min(255, $r), [Math]::Min(255, $g), [Math]::Min(255, $b)))
        }
    }

    $variant.Save((Join-Path $outputRootPath ($Name + ".png")), [System.Drawing.Imaging.ImageFormat]::Png)
    $variant.Dispose()
}

function Flatten-Alpha {
    param([System.Drawing.Bitmap]$Bitmap)

    for ($y = 0; $y -lt $Bitmap.Height; $y++) {
        for ($x = 0; $x -lt $Bitmap.Width; $x++) {
            $color = $Bitmap.GetPixel($x, $y)
            if ($color.A -lt 128) {
                $Bitmap.SetPixel($x, $y, [System.Drawing.Color]::Transparent)
                continue
            }

            $Bitmap.SetPixel($x, $y, [System.Drawing.Color]::FromArgb(255, $color.R, $color.G, $color.B))
        }
    }
}

foreach ($name in $names) {
    $sourcePath = Join-Path $inputRootPath ($name + ".png")
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Missing input sprite: $sourcePath"
    }

    $targetPath = Join-Path $outputRootPath ($name + ".png")
    $baseBitmap = [System.Drawing.Bitmap]::FromFile($sourcePath)
    Flatten-Alpha $baseBitmap
    $baseBitmap.Save($targetPath, [System.Drawing.Imaging.ImageFormat]::Png)

    if ($name -ne "player") {
        Save-DamageVariant $baseBitmap ($name + "_wounded") 0.12 0.92
        Save-DamageVariant $baseBitmap ($name + "_critical") 0.24 0.78
    }

    $baseBitmap.Dispose()

    Write-Host "Imported $name"
}
