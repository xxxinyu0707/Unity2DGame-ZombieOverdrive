param(
    [string]$InputRoot = "ManualCutoutPack",
    [string]$OutputRoot = "Game/Assets/Art/Generated"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$inputRootPath = (Resolve-Path -LiteralPath $InputRoot).Path
$outputRootPath = (Resolve-Path -LiteralPath $OutputRoot).Path

$jobs = @(
    @{ Source = "01_player_green.png"; Name = "player"; Scale = 1.00 },
    @{ Source = "02_walker_green.png"; Name = "walker"; Scale = 1.00 },
    @{ Source = "03_runner_green.png"; Name = "runner"; Scale = 1.00 },
    @{ Source = "04_spitter_green.png"; Name = "spitter"; Scale = 1.00 },
    @{ Source = "05_tanker_green.png"; Name = "tanker"; Scale = 1.00 },
    @{ Source = "06_mid_boss_green.png"; Name = "boss"; Scale = 1.00 },
    @{ Source = "07_final_boss_green.png"; Name = "final_boss"; Scale = 1.00 }
)

function New-ClearBitmap {
    param([int]$Width, [int]$Height)

    $bitmap = New-Object System.Drawing.Bitmap $Width, $Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.Dispose()
    return $bitmap
}

function Get-TrimBounds {
    param([System.Drawing.Bitmap]$Bitmap)

    $minX = $Bitmap.Width
    $minY = $Bitmap.Height
    $maxX = -1
    $maxY = -1
    for ($y = 0; $y -lt $Bitmap.Height; $y++) {
        for ($x = 0; $x -lt $Bitmap.Width; $x++) {
            if ($Bitmap.GetPixel($x, $y).A -gt 8) {
                if ($x -lt $minX) { $minX = $x }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }

    if ($maxX -lt $minX -or $maxY -lt $minY) {
        return [System.Drawing.Rectangle]::new(0, 0, 1, 1)
    }

    return [System.Drawing.Rectangle]::new($minX, $minY, $maxX - $minX + 1, $maxY - $minY + 1)
}

function Copy-Trimmed {
    param([System.Drawing.Bitmap]$Bitmap, [System.Drawing.Rectangle]$Bounds)

    $trimmed = New-Object System.Drawing.Bitmap $Bounds.Width, $Bounds.Height, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    for ($y = 0; $y -lt $Bounds.Height; $y++) {
        for ($x = 0; $x -lt $Bounds.Width; $x++) {
            $trimmed.SetPixel($x, $y, $Bitmap.GetPixel($Bounds.X + $x, $Bounds.Y + $y))
        }
    }
    return $trimmed
}

function Resize-ToCanvas {
    param(
        [System.Drawing.Bitmap]$SourceBitmap,
        [double]$ScaleMultiplier = 1.0,
        [int]$CanvasSize = 64
    )

    $target = New-ClearBitmap $CanvasSize $CanvasSize
    $scale = [Math]::Min(($CanvasSize - 4.0) / [Math]::Max(1, $SourceBitmap.Width), ($CanvasSize - 4.0) / [Math]::Max(1, $SourceBitmap.Height)) * $ScaleMultiplier
    $drawWidth = [Math]::Max(1, [int][Math]::Round($SourceBitmap.Width * $scale))
    $drawHeight = [Math]::Max(1, [int][Math]::Round($SourceBitmap.Height * $scale))
    if ($drawWidth -gt $CanvasSize) {
        $adjust = $CanvasSize / $drawWidth
        $drawWidth = $CanvasSize
        $drawHeight = [Math]::Max(1, [int][Math]::Round($drawHeight * $adjust))
    }
    if ($drawHeight -gt $CanvasSize) {
        $adjust = $CanvasSize / $drawHeight
        $drawHeight = $CanvasSize
        $drawWidth = [Math]::Max(1, [int][Math]::Round($drawWidth * $adjust))
    }

    $dstX = [int][Math]::Round(($CanvasSize - $drawWidth) / 2.0)
    $dstY = [int][Math]::Round($CanvasSize - $drawHeight - 1)

    for ($y = 0; $y -lt $drawHeight; $y++) {
        $sourceY = [Math]::Min($SourceBitmap.Height - 1, [int][Math]::Floor($y / $scale))
        for ($x = 0; $x -lt $drawWidth; $x++) {
            $sourceX = [Math]::Min($SourceBitmap.Width - 1, [int][Math]::Floor($x / $scale))
            $target.SetPixel($dstX + $x, $dstY + $y, $SourceBitmap.GetPixel($sourceX, $sourceY))
        }
    }

    return $target
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

foreach ($job in $jobs) {
    $sourcePath = Join-Path $inputRootPath $job.Source
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        throw "Missing input sprite: $sourcePath"
    }

    $source = [System.Drawing.Bitmap]::FromFile($sourcePath)
    $bounds = Get-TrimBounds $source
    $trimmed = Copy-Trimmed $source $bounds
    $sprite = Resize-ToCanvas $trimmed ([double]$job.Scale) 64
    $sprite.Save((Join-Path $outputRootPath ($job.Name + ".png")), [System.Drawing.Imaging.ImageFormat]::Png)

    if ($job.Name -ne "player") {
        Save-DamageVariant $sprite ($job.Name + "_wounded") 0.12 0.92
        Save-DamageVariant $sprite ($job.Name + "_critical") 0.24 0.78
    }

    Write-Host ("Imported {0} from {1}" -f $job.Name, $job.Source)
    $sprite.Dispose()
    $trimmed.Dispose()
    $source.Dispose()
}
