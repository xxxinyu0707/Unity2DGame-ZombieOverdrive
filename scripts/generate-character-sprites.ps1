param(
    [string]$OutputRoot = "Game/Assets/Art/Generated"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Drawing

$root = Resolve-Path -LiteralPath (Join-Path (Get-Location) $OutputRoot)
$logicalSize = 32
$scale = 2
$size = $logicalSize * $scale

function Color-Hex {
    param(
        [string]$Hex,
        [int]$Alpha = 255
    )

    $value = $Hex.TrimStart("#")
    $r = [Convert]::ToInt32($value.Substring(0, 2), 16)
    $g = [Convert]::ToInt32($value.Substring(2, 2), 16)
    $b = [Convert]::ToInt32($value.Substring(4, 2), 16)
    return [System.Drawing.Color]::FromArgb($Alpha, $r, $g, $b)
}

function Set-LogicalPixel {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [int]$X,
        [int]$Y,
        [System.Drawing.Color]$Color
    )

    if ($X -lt 0 -or $X -ge $logicalSize -or $Y -lt 0 -or $Y -ge $logicalSize) {
        return
    }

    $pixelX = $X * $scale
    $pixelY = ($logicalSize - 1 - $Y) * $scale
    for ($dx = 0; $dx -lt $scale; $dx++) {
        for ($dy = 0; $dy -lt $scale; $dy++) {
            $Bitmap.SetPixel($pixelX + $dx, $pixelY + $dy, $Color)
        }
    }
}

function Draw-Rect {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [int]$X,
        [int]$Y,
        [int]$W,
        [int]$H,
        [System.Drawing.Color]$Color
    )

    for ($ix = $X; $ix -lt $X + $W; $ix++) {
        for ($iy = $Y; $iy -lt $Y + $H; $iy++) {
            Set-LogicalPixel $Bitmap $ix $iy $Color
        }
    }
}

function Draw-Line {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [int]$X0,
        [int]$Y0,
        [int]$X1,
        [int]$Y1,
        [System.Drawing.Color]$Color,
        [int]$Thickness = 1
    )

    $dx = [Math]::Abs($X1 - $X0)
    $sx = if ($X0 -lt $X1) { 1 } else { -1 }
    $dy = -[Math]::Abs($Y1 - $Y0)
    $sy = if ($Y0 -lt $Y1) { 1 } else { -1 }
    $err = $dx + $dy
    while ($true) {
        Draw-Rect $Bitmap ($X0 - [Math]::Floor($Thickness / 2)) ($Y0 - [Math]::Floor($Thickness / 2)) $Thickness $Thickness $Color
        if ($X0 -eq $X1 -and $Y0 -eq $Y1) {
            break
        }

        $e2 = 2 * $err
        if ($e2 -ge $dy) {
            $err += $dy
            $X0 += $sx
        }

        if ($e2 -le $dx) {
            $err += $dx
            $Y0 += $sy
        }
    }
}

function Draw-Shadow {
    param([System.Drawing.Bitmap]$Bitmap)

    $shadow = Color-Hex "#050608" 110
    Draw-Rect $Bitmap 9 2 14 1 $shadow
    Draw-Rect $Bitmap 7 3 18 2 $shadow
    Draw-Rect $Bitmap 10 5 12 1 $shadow
}

function Draw-BloodOverlay {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [int]$Level
    )

    if ($Level -le 0) {
        return
    }

    $blood = Color-Hex "#b8172d"
    $darkBlood = Color-Hex "#661122"
    Draw-Rect $Bitmap 20 14 3 2 $blood
    Draw-Rect $Bitmap 21 12 2 2 $darkBlood
    Draw-Rect $Bitmap 12 20 2 2 $blood
    Draw-Rect $Bitmap 18 23 3 1 $darkBlood

    if ($Level -ge 2) {
        Draw-Rect $Bitmap 8 12 4 2 $blood
        Draw-Rect $Bitmap 23 21 3 3 $darkBlood
        Draw-Rect $Bitmap 15 8 2 3 $blood
        Draw-Rect $Bitmap 11 25 2 2 $darkBlood
        Draw-Line $Bitmap 18 11 23 8 $blood 1
    }
}

function New-Canvas {
    $bitmap = New-Object System.Drawing.Bitmap $size, $size, ([System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $clear = [System.Drawing.Color]::FromArgb(0, 0, 0, 0)
    for ($x = 0; $x -lt $size; $x++) {
        for ($y = 0; $y -lt $size; $y++) {
            $bitmap.SetPixel($x, $y, $clear)
        }
    }

    return $bitmap
}

function Save-Sprite {
    param(
        [string]$Name,
        [scriptblock]$Draw,
        [int]$Damage = 0
    )

    $bitmap = New-Canvas
    & $Draw $bitmap
    Draw-BloodOverlay $bitmap $Damage
    $path = Join-Path $root "$Name.png"
    $bitmap.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()
}

function Draw-Player {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#101722"
    $boots = Color-Hex "#171b24"
    $pants = Color-Hex "#264660"
    $jacket = Color-Hex "#2ba9d6"
    $jacketDark = Color-Hex "#16658b"
    $skin = Color-Hex "#e8ad76"
    $hair = Color-Hex "#2a1b17"
    $visor = Color-Hex "#d5fbff"
    $metal = Color-Hex "#d8dde3"
    $yellow = Color-Hex "#ffe35a"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 11 4 4 5 $outline
    Draw-Rect $Bitmap 12 5 2 4 $boots
    Draw-Rect $Bitmap 17 4 4 5 $outline
    Draw-Rect $Bitmap 18 5 2 4 $boots
    Draw-Rect $Bitmap 10 8 12 10 $outline
    Draw-Rect $Bitmap 12 9 8 8 $jacket
    Draw-Rect $Bitmap 12 9 3 8 $jacketDark
    Draw-Rect $Bitmap 15 10 1 7 $metal
    Draw-Line $Bitmap 8 15 5 11 $outline 2
    Draw-Line $Bitmap 8 15 5 11 $jacketDark 1
    Draw-Line $Bitmap 22 15 27 14 $outline 2
    Draw-Line $Bitmap 22 15 27 14 $skin 1
    Draw-Rect $Bitmap 26 13 5 2 $outline
    Draw-Rect $Bitmap 27 14 3 1 $metal
    Draw-Rect $Bitmap 30 14 2 1 $yellow
    Draw-Rect $Bitmap 10 18 12 10 $outline
    Draw-Rect $Bitmap 12 19 8 7 $skin
    Draw-Rect $Bitmap 11 25 10 3 $hair
    Draw-Rect $Bitmap 13 23 6 2 $visor
    Draw-Rect $Bitmap 13 20 2 1 $outline
    Draw-Rect $Bitmap 18 20 2 1 $outline
    Draw-Rect $Bitmap 15 18 3 1 $hair
}

function Draw-Walker {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#142015"
    $skin = Color-Hex "#5cbf55"
    $skinDark = Color-Hex "#2c6a34"
    $shirt = Color-Hex "#36452c"
    $eye = Color-Hex "#f5d957"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 10 4 4 5 $outline
    Draw-Rect $Bitmap 11 5 2 4 $skinDark
    Draw-Rect $Bitmap 18 4 4 5 $outline
    Draw-Rect $Bitmap 19 5 2 4 $skinDark
    Draw-Rect $Bitmap 10 9 12 10 $outline
    Draw-Rect $Bitmap 12 10 8 8 $skin
    Draw-Rect $Bitmap 12 14 8 3 $shirt
    Draw-Line $Bitmap 10 16 5 14 $outline 2
    Draw-Line $Bitmap 10 16 5 14 $skin 1
    Draw-Line $Bitmap 21 16 27 13 $outline 2
    Draw-Line $Bitmap 21 16 27 13 $skin 1
    Draw-Rect $Bitmap 10 19 12 9 $outline
    Draw-Rect $Bitmap 12 20 8 6 $skin
    Draw-Rect $Bitmap 12 26 8 2 $skinDark
    Draw-Rect $Bitmap 13 22 2 1 $eye
    Draw-Rect $Bitmap 18 22 2 1 $eye
    Draw-Rect $Bitmap 15 20 4 1 $outline
    Draw-Rect $Bitmap 19 13 2 3 (Color-Hex "#9e2032")
}

function Draw-Runner {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#2b160f"
    $skin = Color-Hex "#ef6b3a"
    $skinDark = Color-Hex "#963321"
    $cloth = Color-Hex "#4a3027"
    $eye = Color-Hex "#ffd96a"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 9 5 5 3 $outline
    Draw-Rect $Bitmap 10 5 3 2 $skinDark
    Draw-Rect $Bitmap 19 3 3 6 $outline
    Draw-Rect $Bitmap 20 4 1 4 $skinDark
    Draw-Rect $Bitmap 11 9 11 10 $outline
    Draw-Rect $Bitmap 13 10 7 8 $skin
    Draw-Rect $Bitmap 14 14 6 3 $cloth
    Draw-Line $Bitmap 11 16 5 18 $outline 2
    Draw-Line $Bitmap 11 16 5 18 $skin 1
    Draw-Line $Bitmap 21 15 27 20 $outline 2
    Draw-Line $Bitmap 21 15 27 20 $skin 1
    Draw-Rect $Bitmap 11 19 11 8 $outline
    Draw-Rect $Bitmap 13 20 7 5 $skin
    Draw-Rect $Bitmap 13 26 7 2 $skinDark
    Draw-Rect $Bitmap 14 22 2 1 $eye
    Draw-Rect $Bitmap 18 22 2 1 $eye
    Draw-Rect $Bitmap 15 20 4 1 $outline
}

function Draw-Spitter {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#25300d"
    $skin = Color-Hex "#cbd346"
    $dark = Color-Hex "#71831f"
    $acid = Color-Hex "#a7ff2d"
    $eye = Color-Hex "#fff178"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 11 4 4 5 $outline
    Draw-Rect $Bitmap 12 5 2 4 $dark
    Draw-Rect $Bitmap 18 4 4 5 $outline
    Draw-Rect $Bitmap 19 5 2 4 $dark
    Draw-Rect $Bitmap 10 9 13 12 $outline
    Draw-Rect $Bitmap 12 10 9 9 $skin
    Draw-Rect $Bitmap 14 11 5 5 $acid
    Draw-Rect $Bitmap 15 12 3 3 (Color-Hex "#e6ff75")
    Draw-Line $Bitmap 10 17 5 16 $outline 2
    Draw-Line $Bitmap 10 17 5 16 $skin 1
    Draw-Line $Bitmap 22 17 27 16 $outline 2
    Draw-Line $Bitmap 22 17 27 16 $skin 1
    Draw-Rect $Bitmap 11 20 11 8 $outline
    Draw-Rect $Bitmap 13 21 7 5 $skin
    Draw-Rect $Bitmap 15 22 4 2 $acid
    Draw-Rect $Bitmap 13 24 2 1 $eye
    Draw-Rect $Bitmap 18 24 2 1 $eye
    Draw-Rect $Bitmap 20 18 2 2 $acid
}

function Draw-Tanker {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#171d22"
    $armor = Color-Hex "#8999a5"
    $armorDark = Color-Hex "#485760"
    $skin = Color-Hex "#5aad59"
    $eye = Color-Hex "#f2e56c"
    $rust = Color-Hex "#9c6638"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 9 3 5 7 $outline
    Draw-Rect $Bitmap 10 5 3 4 $armorDark
    Draw-Rect $Bitmap 19 3 5 7 $outline
    Draw-Rect $Bitmap 20 5 3 4 $armorDark
    Draw-Rect $Bitmap 8 9 16 13 $outline
    Draw-Rect $Bitmap 10 11 12 9 $armor
    Draw-Rect $Bitmap 11 13 4 6 $armorDark
    Draw-Rect $Bitmap 17 13 4 6 $armorDark
    Draw-Line $Bitmap 9 17 5 13 $outline 3
    Draw-Line $Bitmap 9 17 5 13 $skin 1
    Draw-Line $Bitmap 23 17 27 13 $outline 3
    Draw-Line $Bitmap 23 17 27 13 $skin 1
    Draw-Rect $Bitmap 10 22 12 7 $outline
    Draw-Rect $Bitmap 12 23 8 4 $skin
    Draw-Rect $Bitmap 13 25 2 1 $eye
    Draw-Rect $Bitmap 18 25 2 1 $eye
    Draw-Rect $Bitmap 20 15 2 2 $rust
}

function Draw-Boss {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#2a0d16"
    $flesh = Color-Hex "#d4364c"
    $dark = Color-Hex "#852033"
    $eye = Color-Hex "#ffe36b"
    $claw = Color-Hex "#e5d4ba"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 8 6 5 9 $outline
    Draw-Rect $Bitmap 9 8 3 6 $dark
    Draw-Rect $Bitmap 20 6 5 9 $outline
    Draw-Rect $Bitmap 21 8 3 6 $dark
    Draw-Rect $Bitmap 8 14 17 13 $outline
    Draw-Rect $Bitmap 11 16 11 9 $flesh
    Draw-Rect $Bitmap 13 18 7 4 $dark
    Draw-Line $Bitmap 8 22 3 19 $outline 3
    Draw-Line $Bitmap 24 22 29 19 $outline 3
    Draw-Rect $Bitmap 2 18 4 2 $claw
    Draw-Rect $Bitmap 27 18 4 2 $claw
    Draw-Rect $Bitmap 10 27 13 4 $outline
    Draw-Rect $Bitmap 12 28 9 2 $flesh
    Draw-Rect $Bitmap 12 29 2 1 $eye
    Draw-Rect $Bitmap 16 29 2 1 $eye
    Draw-Rect $Bitmap 20 29 2 1 $eye
    Draw-Rect $Bitmap 15 21 3 3 (Color-Hex "#ff7582")
}

function Draw-FinalBoss {
    param([System.Drawing.Bitmap]$Bitmap)

    $outline = Color-Hex "#120b20"
    $shell = Color-Hex "#7532c4"
    $dark = Color-Hex "#2d1856"
    $core = Color-Hex "#ff4d7d"
    $eye = Color-Hex "#fff067"
    $glow = Color-Hex "#caa7ff"

    Draw-Shadow $Bitmap
    Draw-Rect $Bitmap 7 6 5 10 $outline
    Draw-Rect $Bitmap 8 8 3 7 $dark
    Draw-Rect $Bitmap 21 6 5 10 $outline
    Draw-Rect $Bitmap 22 8 3 7 $dark
    Draw-Rect $Bitmap 7 14 18 14 $outline
    Draw-Rect $Bitmap 10 16 12 10 $shell
    Draw-Rect $Bitmap 12 18 8 6 $dark
    Draw-Rect $Bitmap 15 20 3 3 $core
    Draw-Rect $Bitmap 16 21 1 1 (Color-Hex "#ffb0ca")
    Draw-Line $Bitmap 7 23 2 27 $glow 1
    Draw-Line $Bitmap 24 23 30 27 $glow 1
    Draw-Rect $Bitmap 9 28 15 3 $outline
    Draw-Rect $Bitmap 12 29 9 1 $shell
    Draw-Rect $Bitmap 12 30 2 1 $eye
    Draw-Rect $Bitmap 16 30 2 1 $eye
    Draw-Rect $Bitmap 20 30 2 1 $eye
}

Save-Sprite "player" ${function:Draw-Player}
Save-Sprite "walker" ${function:Draw-Walker}
Save-Sprite "walker_wounded" ${function:Draw-Walker} 1
Save-Sprite "walker_critical" ${function:Draw-Walker} 2
Save-Sprite "runner" ${function:Draw-Runner}
Save-Sprite "runner_wounded" ${function:Draw-Runner} 1
Save-Sprite "runner_critical" ${function:Draw-Runner} 2
Save-Sprite "spitter" ${function:Draw-Spitter}
Save-Sprite "spitter_wounded" ${function:Draw-Spitter} 1
Save-Sprite "spitter_critical" ${function:Draw-Spitter} 2
Save-Sprite "tanker" ${function:Draw-Tanker}
Save-Sprite "tanker_wounded" ${function:Draw-Tanker} 1
Save-Sprite "tanker_critical" ${function:Draw-Tanker} 2
Save-Sprite "boss" ${function:Draw-Boss}
Save-Sprite "boss_wounded" ${function:Draw-Boss} 1
Save-Sprite "boss_critical" ${function:Draw-Boss} 2
Save-Sprite "final_boss" ${function:Draw-FinalBoss}
Save-Sprite "final_boss_wounded" ${function:Draw-FinalBoss} 1
Save-Sprite "final_boss_critical" ${function:Draw-FinalBoss} 2

Write-Host "Generated pixel character sprites in $root"
