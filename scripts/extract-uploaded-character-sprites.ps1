param(
    [Parameter(Mandatory = $true)]
    [string]$Source,
    [string]$OutputRoot = "Game/Assets/Art/Generated"
)

$ErrorActionPreference = "Stop"

$code = @"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

public static class UploadedCharacterSpriteExtractor
{
    private struct Component
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int Area;
    }

    private static readonly string[] Names =
    {
        "player", "walker", "runner", "spitter", "tanker", "boss", "final_boss"
    };

    public static void Extract(string sourcePath, string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        using (var source = new Bitmap(sourcePath))
        {
            var pixels = ReadPixels(source);
            var components = FindComponents(pixels, source.Width, source.Height)
                .Where(c => c.Y < source.Height * 0.82f)
                .OrderByDescending(c => c.Area)
                .Take(7)
                .OrderBy(c => c.X)
                .ToArray();

            if (components.Length != 7)
            {
                throw new InvalidOperationException("Expected 7 sprites, found " + components.Length + ".");
            }

            for (int i = 0; i < components.Length; i++)
            {
                var name = Names[i];
                var component = components[i];
                using (var cut = CropForeground(source, component, 8))
                using (var trimmed = TrimAlpha(cut))
                using (var sprite = ResizeNearest(trimmed, 64))
                {
                    CleanGreenFringe(sprite);
                    sprite.Save(Path.Combine(outputRoot, name + ".png"), ImageFormat.Png);
                    if (name != "player")
                    {
                        using (var wounded = MakeDamageVariant(sprite, 0.12, 0.92))
                        {
                            wounded.Save(Path.Combine(outputRoot, name + "_wounded.png"), ImageFormat.Png);
                        }

                        using (var critical = MakeDamageVariant(sprite, 0.24, 0.78))
                        {
                            critical.Save(Path.Combine(outputRoot, name + "_critical.png"), ImageFormat.Png);
                        }
                    }
                }

                Console.WriteLine(String.Format("{0,-16} source=({1},{2},{3},{4}) area={5}",
                    name, component.X, component.Y, component.Width, component.Height, component.Area));
            }
        }
    }

    private static byte[] ReadPixels(Bitmap bitmap)
    {
        var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        var clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(clone))
        {
            g.DrawImage(bitmap, rect);
        }

        var data = clone.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            var bytes = new byte[data.Stride * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            return bytes;
        }
        finally
        {
            clone.UnlockBits(data);
            clone.Dispose();
        }
    }

    private static bool IsKeyPixel(byte b, byte g, byte r, byte a)
    {
        if (a <= 8)
        {
            return true;
        }

        return g >= 130 && g - r >= 70 && g - b >= 70;
    }

    private static bool IsForeground(byte[] pixels, int index)
    {
        return !IsKeyPixel(pixels[index], pixels[index + 1], pixels[index + 2], pixels[index + 3]);
    }

    private static List<Component> FindComponents(byte[] pixels, int width, int height)
    {
        var visited = new bool[width * height];
        var components = new List<Component>();
        var queue = new Queue<int>(4096);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = y * width + x;
                if (visited[offset])
                {
                    continue;
                }

                visited[offset] = true;
                if (!IsForeground(pixels, offset * 4))
                {
                    continue;
                }

                int minX = x;
                int maxX = x;
                int minY = y;
                int maxY = y;
                int area = 0;
                queue.Clear();
                queue.Enqueue(offset);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    int cx = current % width;
                    int cy = current / width;
                    area++;
                    if (cx < minX) minX = cx;
                    if (cx > maxX) maxX = cx;
                    if (cy < minY) minY = cy;
                    if (cy > maxY) maxY = cy;

                    TryVisit(current - 1, cx > 0, pixels, visited, queue);
                    TryVisit(current + 1, cx < width - 1, pixels, visited, queue);
                    TryVisit(current - width, cy > 0, pixels, visited, queue);
                    TryVisit(current + width, cy < height - 1, pixels, visited, queue);
                }

                int componentWidth = maxX - minX + 1;
                int componentHeight = maxY - minY + 1;
                if (area >= 900 && componentWidth >= 25 && componentHeight >= 35)
                {
                    components.Add(new Component
                    {
                        X = minX,
                        Y = minY,
                        Width = componentWidth,
                        Height = componentHeight,
                        Area = area
                    });
                }
            }
        }

        return components;
    }

    private static void TryVisit(int next, bool inBounds, byte[] pixels, bool[] visited, Queue<int> queue)
    {
        if (!inBounds || visited[next])
        {
            return;
        }

        visited[next] = true;
        if (IsForeground(pixels, next * 4))
        {
            queue.Enqueue(next);
        }
    }

    private static Bitmap CropForeground(Bitmap source, Component component, int padding)
    {
        int x = Math.Max(0, component.X - padding);
        int y = Math.Max(0, component.Y - padding);
        int right = Math.Min(source.Width, component.X + component.Width + padding);
        int bottom = Math.Min(source.Height, component.Y + component.Height + padding);
        var result = new Bitmap(right - x, bottom - y, PixelFormat.Format32bppArgb);

        for (int py = 0; py < result.Height; py++)
        {
            for (int px = 0; px < result.Width; px++)
            {
                var color = source.GetPixel(x + px, y + py);
                if (IsKeyPixel(color.B, color.G, color.R, color.A))
                {
                    result.SetPixel(px, py, Color.Transparent);
                }
                else
                {
                    result.SetPixel(px, py, color);
                }
            }
        }

        return result;
    }

    private static Bitmap TrimAlpha(Bitmap source)
    {
        int minX = source.Width;
        int minY = source.Height;
        int maxX = -1;
        int maxY = -1;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (source.GetPixel(x, y).A > 0)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        if (maxX < minX || maxY < minY)
        {
            return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        }

        var result = new Bitmap(maxX - minX + 1, maxY - minY + 1, PixelFormat.Format32bppArgb);
        for (int y = 0; y < result.Height; y++)
        {
            for (int x = 0; x < result.Width; x++)
            {
                result.SetPixel(x, y, source.GetPixel(minX + x, minY + y));
            }
        }

        return result;
    }

    private static Bitmap ResizeNearest(Bitmap source, int canvasSize)
    {
        var result = new Bitmap(canvasSize, canvasSize, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.Transparent);
        }

        double scale = Math.Min((canvasSize - 4.0) / Math.Max(1, source.Width), (canvasSize - 4.0) / Math.Max(1, source.Height));
        int drawWidth = Math.Max(1, (int)Math.Round(source.Width * scale));
        int drawHeight = Math.Max(1, (int)Math.Round(source.Height * scale));
        int dstX = (int)Math.Round((canvasSize - drawWidth) / 2.0);
        int dstY = canvasSize - drawHeight - 1;

        for (int y = 0; y < drawHeight; y++)
        {
            int sy = Math.Min(source.Height - 1, (int)Math.Floor(y / scale));
            for (int x = 0; x < drawWidth; x++)
            {
                int sx = Math.Min(source.Width - 1, (int)Math.Floor(x / scale));
                result.SetPixel(dstX + x, dstY + y, source.GetPixel(sx, sy));
            }
        }

        return result;
    }

    private static Bitmap MakeDamageVariant(Bitmap source, double bloodAmount, double darken)
    {
        var result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var color = source.GetPixel(x, y);
                if (color.A == 0)
                {
                    result.SetPixel(x, y, Color.Transparent);
                    continue;
                }

                int r = Clamp((int)Math.Round(color.R * darken));
                int g = Clamp((int)Math.Round(color.G * darken));
                int b = Clamp((int)Math.Round(color.B * darken));
                double noise = ((x * 37 + y * 59 + (x & 7) * 11) % 100) / 100.0;

                if (noise < bloodAmount && y > 14 && x > 8 && x < 56)
                {
                    r = Clamp((int)Math.Round(r * 0.45 + 152 * 0.55));
                    g = Clamp((int)Math.Round(g * 0.45 + 20 * 0.55));
                    b = Clamp((int)Math.Round(b * 0.45 + 24 * 0.55));
                }

                result.SetPixel(x, y, Color.FromArgb(color.A, r, g, b));
            }
        }

        return result;
    }

    private static void CleanGreenFringe(Bitmap bitmap)
    {
        for (int pass = 0; pass < 3; pass++)
        {
            var clear = new bool[bitmap.Width, bitmap.Height];
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.A == 0)
                    {
                        continue;
                    }

                    if (TouchesTransparent(bitmap, x, y, 3) && IsEdgeGreenSpill(color))
                    {
                        clear[x, y] = true;
                    }
                }
            }

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (clear[x, y])
                    {
                        bitmap.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }

        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                if (color.A == 0 || !IsMildGreenSpill(color))
                {
                    continue;
                }

                int r = Clamp((int)Math.Round(color.R * 1.10));
                int g = TouchesTransparent(bitmap, x, y, 4)
                    ? Clamp((int)Math.Round(Math.Min(color.R, color.B) * 0.82))
                    : Clamp((int)Math.Round((color.R + color.B) * 0.54));
                int b = Clamp((int)Math.Round(color.B * 1.04));
                bitmap.SetPixel(x, y, Color.FromArgb(color.A, r, g, b));
            }
        }
    }

    private static bool IsEdgeGreenSpill(Color color)
    {
        return color.G >= 50 && color.G - color.R >= 14 && color.G - color.B >= 14;
    }

    private static bool IsMildGreenSpill(Color color)
    {
        return color.G >= 55 && color.G - color.R >= 12 && color.G - color.B >= 12;
    }

    private static bool TouchesTransparent(Bitmap bitmap, int x, int y, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx == 0 && dy == 0)
                {
                    continue;
                }

                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= bitmap.Width || ny >= bitmap.Height)
                {
                    return true;
                }

                if (bitmap.GetPixel(nx, ny).A == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int Clamp(int value)
    {
        if (value < 0) return 0;
        if (value > 255) return 255;
        return value;
    }
}
"@

Add-Type -TypeDefinition $code -ReferencedAssemblies "System.Drawing"

$sourcePath = (Resolve-Path -LiteralPath $Source).Path
$outputRootPath = (Resolve-Path -LiteralPath $OutputRoot).Path
[UploadedCharacterSpriteExtractor]::Extract($sourcePath, $outputRootPath)
Write-Host "Extracted uploaded character sprites to $outputRootPath"
