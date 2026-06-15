using BigGustave;

namespace AlvorKit.FreeType.Demo;

/// <summary>Small RGB color value used by generated demo images.</summary>
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

/// <summary>Controls whether a glyph bitmap is treated as coverage or self-colored pixels.</summary>
internal enum GlyphPixelView
{
    Coverage,

    OwnColor,
}

/// <summary>Numeric helpers for FreeType fixed-point and native-sized values.</summary>
internal static class FreeTypeValues
{
    public const int FixedOne = 65536;

    public const int PixelOne = 64;

    /// <summary>Converts a CLong wrapper to a managed signed integer.</summary>
    public static long ToInt64(CLong value) => value.Value.ToInt64();

    /// <summary>Converts a CULong wrapper to a managed unsigned integer.</summary>
    public static ulong ToUInt64(CULong value) => value.Value.ToUInt64();

    /// <summary>Converts a floating-point scalar to FreeType 16.16 fixed point.</summary>
    public static CLong Fixed(double value) => new((nint)Math.Round(value * FixedOne));

    /// <summary>Wraps a managed integer as a CLong value.</summary>
    public static CLong Long(int value) => new(value);

    /// <summary>Wraps a managed unsigned integer as a CULong value.</summary>
    public static CULong ULong(uint value) => new(value);

    /// <summary>Converts a FreeType 26.6 pixel value to an integer pixel.</summary>
    public static int Pixel26Dot6(CLong value) => (int)Math.Round(ToInt64(value) / (double)PixelOne);

    /// <summary>Converts a FreeType 16.16 fixed-point value to a floating-point scalar.</summary>
    public static double Fixed16Dot16(CLong value) => ToInt64(value) / (double)FixedOne;
}

/// <summary>Managed RGBA glyph image plus placement metrics read from FreeType bitmaps.</summary>
internal sealed class GlyphImage
{
    /// <summary>Creates a managed glyph image with placement metrics.</summary>
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

    /// <summary>Reads the current slot bitmap into a managed RGBA image.</summary>
    public static GlyphImage FromSlot(FtGlyphSlotRec slot, GlyphPixelView view = GlyphPixelView.Coverage)
    {
        var advanceX = FreeTypeValues.Pixel26Dot6(slot.Advance.X);
        return FromBitmap(slot.Bitmap, slot.BitmapLeft, slot.BitmapTop, advanceX, view);
    }

    /// <summary>Reads a caller-owned FreeType bitmap with explicit glyph placement metrics.</summary>
    public static GlyphImage FromBitmap(
        FtBitmap bitmap,
        int bitmapLeft,
        int bitmapTop,
        int advanceX,
        GlyphPixelView view = GlyphPixelView.Coverage)
    {
        var mode = (FtPixelMode)bitmap.PixelMode;
        return mode switch
        {
            FtPixelMode.PixelModeMono => ReadMono(bitmap, bitmapLeft, bitmapTop, advanceX),
            FtPixelMode.PixelModeGray => ReadGray(bitmap, bitmapLeft, bitmapTop, advanceX, view),
            FtPixelMode.PixelModeGray2 => ReadPackedGray(bitmap, bitmapLeft, bitmapTop, advanceX, bitsPerPixel: 2),
            FtPixelMode.PixelModeGray4 => ReadPackedGray(bitmap, bitmapLeft, bitmapTop, advanceX, bitsPerPixel: 4),
            FtPixelMode.PixelModeLcd => ReadLcd(bitmap, bitmapLeft, bitmapTop, advanceX),
            FtPixelMode.PixelModeLcdV => ReadLcdV(bitmap, bitmapLeft, bitmapTop, advanceX),
            FtPixelMode.PixelModeBgra => ReadBgra(bitmap, bitmapLeft, bitmapTop, advanceX),
            _ => CreateEmpty(bitmapLeft, bitmapTop, advanceX),
        };
    }

    /// <summary>Saves this glyph centered on a small PNG canvas.</summary>
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

    /// <summary>Creates a transparent placeholder for unsupported or empty bitmap modes.</summary>
    private static GlyphImage CreateEmpty(int bitmapLeft, int bitmapTop, int advanceX) =>
        new(1, 1, bitmapLeft, bitmapTop, advanceX, [0, 0, 0, 0], usesOwnColor: false);

    /// <summary>Reads a one-bit monochrome FreeType bitmap into RGBA coverage pixels.</summary>
    private static GlyphImage ReadMono(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX)
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

        return new GlyphImage(width, height, bitmapLeft, bitmapTop, advanceX, rgba, usesOwnColor: false);
    }

    /// <summary>Reads an eight-bit grayscale FreeType bitmap into RGBA pixels.</summary>
    private static GlyphImage ReadGray(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX, GlyphPixelView view)
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
            bitmapLeft,
            bitmapTop,
            advanceX,
            rgba,
            view == GlyphPixelView.OwnColor);
    }

    /// <summary>Reads a packed two-bit or four-bit grayscale FreeType bitmap.</summary>
    private static GlyphImage ReadPackedGray(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX, int bitsPerPixel)
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

        return new GlyphImage(width, height, bitmapLeft, bitmapTop, advanceX, rgba, usesOwnColor: false);
    }

    /// <summary>Reads a horizontal LCD subpixel FreeType bitmap.</summary>
    private static GlyphImage ReadLcd(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX)
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

        return new GlyphImage(width, height, bitmapLeft / 3, bitmapTop, advanceX, rgba, usesOwnColor: true);
    }

    /// <summary>Reads a vertical LCD subpixel FreeType bitmap.</summary>
    private static GlyphImage ReadLcdV(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX)
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

        return new GlyphImage(width, height, bitmapLeft, bitmapTop / 3, advanceX, rgba, usesOwnColor: true);
    }

    /// <summary>Reads a BGRA color FreeType bitmap.</summary>
    private static GlyphImage ReadBgra(FtBitmap bitmap, int bitmapLeft, int bitmapTop, int advanceX)
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

        return new GlyphImage(width, height, bitmapLeft, bitmapTop, advanceX, rgba, usesOwnColor: true);
    }

    /// <summary>Returns a row pointer while honoring positive or negative FreeType bitmap pitch.</summary>
    private static nint RowPointer(FtBitmap bitmap, int y)
    {
        var pitch = bitmap.Pitch;
        return pitch >= 0 ? bitmap.Buffer + y * pitch : bitmap.Buffer + (checked((int)bitmap.Rows) - 1 - y) * -pitch;
    }

    /// <summary>Writes a white coverage pixel into an RGBA buffer.</summary>
    private static void WriteCoverage(byte[] rgba, int width, int x, int y, byte alpha) => WriteOwnColor(rgba, width, x, y, 255, 255, 255, alpha);

    /// <summary>Writes a caller-colored pixel into an RGBA buffer.</summary>
    private static void WriteOwnColor(byte[] rgba, int width, int x, int y, byte r, byte g, byte b, byte a)
    {
        var offset = (y * width + x) * 4;
        rgba[offset] = r;
        rgba[offset + 1] = g;
        rgba[offset + 2] = b;
        rgba[offset + 3] = a;
    }
}

/// <summary>Minimal RGB canvas used to render demo PNG outputs.</summary>
internal sealed class PngCanvas
{
    private readonly byte[] pixels;

    /// <summary>Creates a canvas filled with a background color.</summary>
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

    /// <summary>Alpha-blends a glyph image onto the canvas at a baseline origin.</summary>
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

    /// <summary>Draws a filled rectangle clipped to the canvas bounds.</summary>
    public void DrawRectangle(int x, int y, int width, int height, DemoColor color)
    {
        for (var row = Math.Max(0, y); row < Math.Min(Height, y + height); row++)
        {
            for (var column = Math.Max(0, x); column < Math.Min(Width, x + width); column++)
                SetPixel(column, row, color);
        }
    }

    /// <summary>Draws a simple integer line clipped by pixel writes.</summary>
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

    /// <summary>Draws a filled circle clipped to the canvas bounds.</summary>
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

    /// <summary>Writes the canvas as a PNG file.</summary>
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

    /// <summary>Writes one opaque RGB pixel if it lies inside the canvas.</summary>
    private void SetPixel(int x, int y, DemoColor color)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;

        var offset = (y * Width + x) * 3;
        pixels[offset] = color.R;
        pixels[offset + 1] = color.G;
        pixels[offset + 2] = color.B;
    }

    /// <summary>Alpha-blends one source RGB pixel if it lies inside the canvas.</summary>
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

/// <summary>Drawing helpers that bridge FreeType glyph slots and the demo canvas.</summary>
internal static unsafe class FreeTypeDrawing
{
    /// <summary>Copies a face record from its native pointer.</summary>
    public static FtFaceRec ReadFace(FtFaceRec* face) => *face;

    /// <summary>Returns the active glyph slot pointer for a face.</summary>
    public static FtGlyphSlotRec* GlyphSlot(FtFaceRec* face) => face->Glyph;

    /// <summary>Copies the active glyph slot record for a face.</summary>
    public static FtGlyphSlotRec ReadGlyphSlot(FtFaceRec* face) => *GlyphSlot(face);

    /// <summary>Reads the active glyph slot into a managed glyph image.</summary>
    public static GlyphImage CurrentGlyph(FtFaceRec* face, GlyphPixelView view = GlyphPixelView.Coverage) => GlyphImage.FromSlot(ReadGlyphSlot(face), view);

    /// <summary>Converts typed load flags for FreeType character overloads that still take raw flag bits.</summary>
    public static int LoadFlagBits(FtLoadFlags flags) => (int)flags;

    /// <summary>Draws text at the current face size, optionally applying FreeType kerning.</summary>
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

            FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, FtLoadFlags.Render));
            var glyph = CurrentGlyph(face);
            canvas.DrawGlyph(glyph, penX, baselineY, color);
            penX += glyph.AdvanceX;
            previousGlyph = glyphIndex;
        }
    }

    /// <summary>Returns pair kerning in integer pixels for two glyph indices.</summary>
    private static int KerningPixels(Ft ft, FtFaceRec* face, uint leftGlyph, uint rightGlyph)
    {
        FreeTypeStatus.Require(ft, "FT_Get_Kerning", ft.GetKerning(face, leftGlyph, rightGlyph, FtKerningMode.KerningDefault, out var kerning));
        return FreeTypeValues.Pixel26Dot6(kerning.X);
    }
}
