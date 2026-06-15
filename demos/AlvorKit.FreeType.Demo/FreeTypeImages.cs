using BigGustave;

namespace AlvorKit.FreeType.Demo;

internal readonly struct DemoColor(byte r, byte g, byte b)
{
    public byte R { get; } = r;

    public byte G { get; } = g;

    public byte B { get; } = b;

    public static DemoColor Background { get; } = new(14, 16, 22);

    public static DemoColor Guide { get; } = new(62, 70, 88);

    public static DemoColor White { get; } = new(238, 241, 248);

    public static DemoColor Gold { get; } = new(245, 194, 95);

    public static DemoColor Cyan { get; } = new(103, 196, 220);

    public static DemoColor Green { get; } = new(132, 214, 146);

    public static DemoColor Red { get; } = new(235, 104, 104);
}

internal enum GlyphPixelView
{
    Coverage,

    OwnColor,
}

internal static class FreeTypeValues
{
    public const int FixedOne = 65536;

    public const int PixelOne = 64;

    public static long ToInt64(CLong value) => value.Value.ToInt64();

    public static ulong ToUInt64(CULong value) => value.Value.ToUInt64();

    public static CLong Fixed(double value) => new((nint)Math.Round(value * FixedOne));

    public static CLong Long(int value) => new(value);

    public static CULong ULong(uint value) => new(value);

    public static int Pixel26Dot6(CLong value) => (int)Math.Round(ToInt64(value) / (double)PixelOne);
}

internal sealed class GlyphImage
{
    private GlyphImage(int width, int height, int bitmapLeft, int bitmapTop, int advanceX, byte[] rgba, bool usesOwnColor)
    {
        Width = width;
        Height = height;
        BitmapLeft = bitmapLeft;
        BitmapTop = bitmapTop;
        AdvanceX = advanceX;
        Rgba = rgba;
        UsesOwnColor = usesOwnColor;
    }

    public int Width { get; }

    public int Height { get; }

    public int BitmapLeft { get; }

    public int BitmapTop { get; }

    public int AdvanceX { get; }

    public byte[] Rgba { get; }

    public bool UsesOwnColor { get; }

    public static GlyphImage FromSlot(FtGlyphSlotRec slot, GlyphPixelView view = GlyphPixelView.Coverage)
    {
        var bitmap = slot.Bitmap;
        var mode = (FtPixelMode)bitmap.PixelMode;
        return mode switch
        {
            FtPixelMode.PixelModeMono => ReadMono(slot, bitmap),
            FtPixelMode.PixelModeGray => ReadGray(slot, bitmap, view),
            FtPixelMode.PixelModeGray2 => ReadPackedGray(slot, bitmap, bitsPerPixel: 2),
            FtPixelMode.PixelModeGray4 => ReadPackedGray(slot, bitmap, bitsPerPixel: 4),
            FtPixelMode.PixelModeLcd => ReadLcd(slot, bitmap),
            FtPixelMode.PixelModeLcdV => ReadLcdV(slot, bitmap),
            FtPixelMode.PixelModeBgra => ReadBgra(slot, bitmap),
            _ => CreateEmpty(slot),
        };
    }

    public void SaveCentered(string path, DemoColor background, DemoColor tint)
    {
        var canvas = new PngCanvas(Math.Max(180, Width + 80), Math.Max(180, Height + 80), background);
        var targetX = (canvas.Width - Width) / 2;
        var targetY = (canvas.Height - Height) / 2;
        var originX = targetX - BitmapLeft;
        var baselineY = targetY + BitmapTop;
        canvas.DrawGlyph(this, originX, baselineY, tint);
        canvas.Save(path);
    }

    private static GlyphImage CreateEmpty(FtGlyphSlotRec slot) =>
        new(1, 1, slot.BitmapLeft, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), [0, 0, 0, 0], usesOwnColor: false);

    private static GlyphImage ReadMono(FtGlyphSlotRec slot, FtBitmap bitmap)
    {
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows);
        var rgba = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y);
            for (var x = 0; x < width; x++)
            {
                var packed = Marshal.ReadByte(row, x >> 3);
                var alpha = (packed & (0x80 >> (x & 7))) == 0 ? (byte)0 : (byte)255;
                WriteCoverage(rgba, width, x, y, alpha);
            }
        }

        return new GlyphImage(width, height, slot.BitmapLeft, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), rgba, usesOwnColor: false);
    }

    private static GlyphImage ReadGray(FtGlyphSlotRec slot, FtBitmap bitmap, GlyphPixelView view)
    {
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows);
        var rgba = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y);
            for (var x = 0; x < width; x++)
            {
                var sample = Marshal.ReadByte(row, x);
                if (view == GlyphPixelView.OwnColor)
                    WriteOwnColor(rgba, width, x, y, sample, (byte)Math.Abs(sample - 128), (byte)(255 - sample), 255);
                else
                    WriteCoverage(rgba, width, x, y, sample);
            }
        }

        return new GlyphImage(
            width,
            height,
            slot.BitmapLeft,
            slot.BitmapTop,
            FreeTypeValues.Pixel26Dot6(slot.Advance.X),
            rgba,
            view == GlyphPixelView.OwnColor);
    }

    private static GlyphImage ReadPackedGray(FtGlyphSlotRec slot, FtBitmap bitmap, int bitsPerPixel)
    {
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows);
        var rgba = new byte[width * height * 4];
        var mask = (1 << bitsPerPixel) - 1;
        var max = mask;

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y);
            for (var x = 0; x < width; x++)
            {
                var packed = Marshal.ReadByte(row, x * bitsPerPixel / 8);
                var shift = 8 - bitsPerPixel - x * bitsPerPixel % 8;
                var sample = (byte)(((packed >> shift) & mask) * 255 / max);
                WriteCoverage(rgba, width, x, y, sample);
            }
        }

        return new GlyphImage(width, height, slot.BitmapLeft, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), rgba, usesOwnColor: false);
    }

    private static GlyphImage ReadLcd(FtGlyphSlotRec slot, FtBitmap bitmap)
    {
        var width = checked((int)bitmap.Width) / 3;
        var height = checked((int)bitmap.Rows);
        var rgba = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y);
            for (var x = 0; x < width; x++)
            {
                var r = Marshal.ReadByte(row, x * 3);
                var g = Marshal.ReadByte(row, x * 3 + 1);
                var b = Marshal.ReadByte(row, x * 3 + 2);
                WriteOwnColor(rgba, width, x, y, r, g, b, Math.Max(r, Math.Max(g, b)));
            }
        }

        return new GlyphImage(width, height, slot.BitmapLeft / 3, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), rgba, usesOwnColor: true);
    }

    private static GlyphImage ReadLcdV(FtGlyphSlotRec slot, FtBitmap bitmap)
    {
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows) / 3;
        var rgba = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            var rowR = RowPointer(bitmap, y * 3);
            var rowG = RowPointer(bitmap, y * 3 + 1);
            var rowB = RowPointer(bitmap, y * 3 + 2);
            for (var x = 0; x < width; x++)
            {
                var r = Marshal.ReadByte(rowR, x);
                var g = Marshal.ReadByte(rowG, x);
                var b = Marshal.ReadByte(rowB, x);
                WriteOwnColor(rgba, width, x, y, r, g, b, Math.Max(r, Math.Max(g, b)));
            }
        }

        return new GlyphImage(width, height, slot.BitmapLeft, slot.BitmapTop / 3, FreeTypeValues.Pixel26Dot6(slot.Advance.X), rgba, usesOwnColor: true);
    }

    private static GlyphImage ReadBgra(FtGlyphSlotRec slot, FtBitmap bitmap)
    {
        var width = checked((int)bitmap.Width);
        var height = checked((int)bitmap.Rows);
        var rgba = new byte[width * height * 4];

        for (var y = 0; y < height; y++)
        {
            var row = RowPointer(bitmap, y);
            for (var x = 0; x < width; x++)
            {
                var offset = x * 4;
                var b = Marshal.ReadByte(row, offset);
                var g = Marshal.ReadByte(row, offset + 1);
                var r = Marshal.ReadByte(row, offset + 2);
                var a = Marshal.ReadByte(row, offset + 3);
                WriteOwnColor(rgba, width, x, y, r, g, b, a);
            }
        }

        return new GlyphImage(width, height, slot.BitmapLeft, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), rgba, usesOwnColor: true);
    }

    private static nint RowPointer(FtBitmap bitmap, int y)
    {
        var pitch = bitmap.Pitch;
        return pitch >= 0 ? bitmap.Buffer + y * pitch : bitmap.Buffer + (checked((int)bitmap.Rows) - 1 - y) * -pitch;
    }

    private static void WriteCoverage(byte[] rgba, int width, int x, int y, byte alpha) => WriteOwnColor(rgba, width, x, y, 255, 255, 255, alpha);

    private static void WriteOwnColor(byte[] rgba, int width, int x, int y, byte r, byte g, byte b, byte a)
    {
        var offset = (y * width + x) * 4;
        rgba[offset] = r;
        rgba[offset + 1] = g;
        rgba[offset + 2] = b;
        rgba[offset + 3] = a;
    }
}

internal sealed class PngCanvas
{
    private readonly byte[] pixels;

    public PngCanvas(int width, int height, DemoColor background)
    {
        Width = width;
        Height = height;
        pixels = new byte[width * height * 3];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
                SetPixel(x, y, background);
        }
    }

    public int Width { get; }

    public int Height { get; }

    public void DrawGlyph(GlyphImage glyph, int originX, int baselineY, DemoColor tint)
    {
        var targetX = originX + glyph.BitmapLeft;
        var targetY = baselineY - glyph.BitmapTop;

        for (var y = 0; y < glyph.Height; y++)
        {
            for (var x = 0; x < glyph.Width; x++)
            {
                var source = (y * glyph.Width + x) * 4;
                var alpha = glyph.Rgba[source + 3];
                if (alpha == 0)
                    continue;

                var r = glyph.UsesOwnColor ? glyph.Rgba[source] : tint.R;
                var g = glyph.UsesOwnColor ? glyph.Rgba[source + 1] : tint.G;
                var b = glyph.UsesOwnColor ? glyph.Rgba[source + 2] : tint.B;
                BlendPixel(targetX + x, targetY + y, r, g, b, alpha);
            }
        }
    }

    public void DrawRectangle(int x, int y, int width, int height, DemoColor color)
    {
        for (var row = Math.Max(0, y); row < Math.Min(Height, y + height); row++)
        {
            for (var column = Math.Max(0, x); column < Math.Min(Width, x + width); column++)
                SetPixel(column, row, color);
        }
    }

    public void DrawLine(int x0, int y0, int x1, int y1, DemoColor color)
    {
        var dx = x1 - x0;
        var dy = y1 - y0;
        var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
        {
            SetPixel(x0, y0, color);
            return;
        }

        for (var i = 0; i <= steps; i++)
            SetPixel(x0 + dx * i / steps, y0 + dy * i / steps, color);
    }

    public void DrawCircle(int centerX, int centerY, int radius, DemoColor color)
    {
        var radiusSquared = radius * radius;
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radiusSquared)
                    SetPixel(centerX + x, centerY + y, color);
            }
        }
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        var png = PngBuilder.Create(Width, Height, false);

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var offset = (y * Width + x) * 3;
                png.SetPixel(new Pixel(pixels[offset], pixels[offset + 1], pixels[offset + 2]), x, y);
            }
        }

        using var output = File.Create(path);
        png.Save(output);
    }

    private void SetPixel(int x, int y, DemoColor color)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;

        var offset = (y * Width + x) * 3;
        pixels[offset] = color.R;
        pixels[offset + 1] = color.G;
        pixels[offset + 2] = color.B;
    }

    private void BlendPixel(int x, int y, byte r, byte g, byte b, byte alpha)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;

        var offset = (y * Width + x) * 3;
        var inverse = 255 - alpha;
        pixels[offset] = (byte)((r * alpha + pixels[offset] * inverse + 127) / 255);
        pixels[offset + 1] = (byte)((g * alpha + pixels[offset + 1] * inverse + 127) / 255);
        pixels[offset + 2] = (byte)((b * alpha + pixels[offset + 2] * inverse + 127) / 255);
    }
}

internal static unsafe class FreeTypeDrawing
{
    public static FtFaceRec ReadFace(FtFaceRec* face) => *face;

    public static FtGlyphSlotRec* GlyphSlot(FtFaceRec* face) => face->Glyph;

    public static FtGlyphSlotRec ReadGlyphSlot(FtFaceRec* face) => *GlyphSlot(face);

    public static GlyphImage CurrentGlyph(FtFaceRec* face, GlyphPixelView view = GlyphPixelView.Coverage) => GlyphImage.FromSlot(ReadGlyphSlot(face), view);

    public static void DrawTextCurrentSize(
        Ft ft,
        FtFaceRec* face,
        PngCanvas canvas,
        string text,
        int x,
        int baselineY,
        bool useKerning,
        DemoColor color)
    {
        var penX = x;
        uint previousGlyph = 0;

        foreach (var character in text)
        {
            if (character == '\n')
                continue;

            var glyphIndex = ft.GetCharIndex(face, character);
            if (glyphIndex == 0)
                continue;

            if (useKerning && previousGlyph != 0)
                penX += KerningPixels(ft, face, previousGlyph, glyphIndex);

            FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, Ft.LoadRender));
            var glyph = CurrentGlyph(face);
            canvas.DrawGlyph(glyph, penX, baselineY, color);
            penX += glyph.AdvanceX;
            previousGlyph = glyphIndex;
        }
    }

    private static int KerningPixels(Ft ft, FtFaceRec* face, uint leftGlyph, uint rightGlyph)
    {
        var kerning = new FtVector();
        FreeTypeStatus.Require(ft, "FT_Get_Kerning", ft.GetKerning(face, leftGlyph, rightGlyph, (uint)FtKerningMode.KerningDefault, &kerning));
        return FreeTypeValues.Pixel26Dot6(kerning.X);
    }
}
