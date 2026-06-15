using BigGustave;

namespace AlvorKit.FreeType.Demo;

/// <summary>Small RGB value used by the demo canvas and glyph compositing helpers.</summary>
/// <param name="r">The red channel.</param>
/// <param name="g">The green channel.</param>
/// <param name="b">The blue channel.</param>
internal readonly struct DemoColor(byte r, byte g, byte b)
{
    /// <summary>The red channel.</summary>
    public byte R { get; } = r;

    /// <summary>The green channel.</summary>
    public byte G { get; } = g;

    /// <summary>The blue channel.</summary>
    public byte B { get; } = b;

    /// <summary>Dark background used for exported demo sheets.</summary>
    public static DemoColor Background { get; } = new(14, 16, 22);

    /// <summary>Muted guide color used for outlines and baselines.</summary>
    public static DemoColor Guide { get; } = new(62, 70, 88);

    /// <summary>White text and glyph tint.</summary>
    public static DemoColor White { get; } = new(238, 241, 248);

    /// <summary>Warm accent used for rendered glyphs.</summary>
    public static DemoColor Gold { get; } = new(245, 194, 95);

    /// <summary>Cool accent used for comparison glyphs.</summary>
    public static DemoColor Cyan { get; } = new(103, 196, 220);

    /// <summary>Green accent used when kerning or a transformed value is enabled.</summary>
    public static DemoColor Green { get; } = new(132, 214, 146);

    /// <summary>Red accent used for unkerned or before-state comparisons.</summary>
    public static DemoColor Red { get; } = new(235, 104, 104);
}

/// <summary>Controls how a copied FreeType bitmap is converted into RGBA pixels.</summary>
internal enum GlyphPixelView
{
    /// <summary>Treat grayscale values as coverage and tint them while compositing.</summary>
    Coverage,

    /// <summary>Preserve subpixel or signed-distance values as visible RGB colors.</summary>
    OwnColor,
}

/// <summary>Helpers for converting native-sized FreeType values into managed numbers.</summary>
internal static class FreeTypeValues
{
    /// <summary>Number of fractional units in one 16.16 fixed-point unit.</summary>
    public const int FixedOne = 65536;

    /// <summary>Number of fractional units in one 26.6 pixel unit.</summary>
    public const int PixelOne = 64;

    /// <summary>Converts an FT_Long-shaped value to <see cref="long"/>.</summary>
    /// <param name="value">The native-sized signed value.</param>
    /// <returns>The managed integer value.</returns>
    public static long ToInt64(CLong value) => value.Value.ToInt64();

    /// <summary>Converts an FT_ULong-shaped value to <see cref="ulong"/>.</summary>
    /// <param name="value">The native-sized unsigned value.</param>
    /// <returns>The managed integer value.</returns>
    public static ulong ToUInt64(CULong value) => value.Value.ToUInt64();

    /// <summary>Creates a native long from a 16.16 fixed-point floating value.</summary>
    /// <param name="value">The floating-point value to encode.</param>
    /// <returns>The value encoded as a native C long.</returns>
    public static CLong Fixed(double value) => new((nint)Math.Round(value * FixedOne));

    /// <summary>Creates a native long from an integer value.</summary>
    /// <param name="value">The integer value to encode.</param>
    /// <returns>The value encoded as a native C long.</returns>
    public static CLong Long(int value) => new(value);

    /// <summary>Creates an FT_ULong-shaped value from a Unicode scalar value.</summary>
    /// <param name="value">The Unicode scalar value.</param>
    /// <returns>The scalar encoded as a native unsigned C long.</returns>
    public static CULong ULong(uint value) => new(value);

    /// <summary>Converts a 26.6 pixel value into rounded integer pixels.</summary>
    /// <param name="value">The 26.6 value returned by FreeType.</param>
    /// <returns>The rounded pixel count.</returns>
    public static int Pixel26Dot6(CLong value) => (int)Math.Round(ToInt64(value) / (double)PixelOne);
}

/// <summary>A FreeType glyph slot bitmap copied into managed RGBA pixels.</summary>
internal sealed class GlyphImage
{
    /// <summary>Creates a managed glyph image from copied bitmap pixels.</summary>
    /// <param name="width">The number of output pixels in each row.</param>
    /// <param name="height">The number of output rows.</param>
    /// <param name="bitmapLeft">Horizontal bitmap bearing from the glyph origin.</param>
    /// <param name="bitmapTop">Vertical bitmap bearing above the glyph baseline.</param>
    /// <param name="advanceX">Horizontal pen advance in integer pixels.</param>
    /// <param name="rgba">Copied RGBA pixels in row-major order.</param>
    /// <param name="usesOwnColor">Whether compositing should preserve the stored RGB channels.</param>
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

    /// <summary>The number of output pixels in each row.</summary>
    public int Width { get; }

    /// <summary>The number of output rows.</summary>
    public int Height { get; }

    /// <summary>Horizontal bitmap bearing from the glyph origin.</summary>
    public int BitmapLeft { get; }

    /// <summary>Vertical bitmap bearing above the glyph baseline.</summary>
    public int BitmapTop { get; }

    /// <summary>Horizontal pen advance in integer pixels.</summary>
    public int AdvanceX { get; }

    /// <summary>Copied RGBA pixels in row-major order.</summary>
    public byte[] Rgba { get; }

    /// <summary>Whether compositing should preserve the stored RGB channels.</summary>
    public bool UsesOwnColor { get; }

    /// <summary>Copies the current FreeType glyph slot bitmap into managed memory.</summary>
    /// <param name="slot">The glyph slot returned through FT_FaceRec.glyph.</param>
    /// <param name="view">The desired visual interpretation of the bitmap samples.</param>
    /// <returns>A managed glyph image ready for PNG compositing.</returns>
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

    /// <summary>Saves this glyph centered on a simple dark PNG canvas.</summary>
    /// <param name="path">The PNG path to write.</param>
    /// <param name="background">The canvas background color.</param>
    /// <param name="tint">The tint used for coverage-only glyphs.</param>
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

    /// <summary>Creates an empty glyph image for bitmap modes that did not produce pixels.</summary>
    /// <param name="slot">The slot whose metrics still describe the glyph advance.</param>
    /// <returns>A one-pixel transparent glyph image.</returns>
    private static GlyphImage CreateEmpty(FtGlyphSlotRec slot) =>
        new(1, 1, slot.BitmapLeft, slot.BitmapTop, FreeTypeValues.Pixel26Dot6(slot.Advance.X), [0, 0, 0, 0], usesOwnColor: false);

    /// <summary>Decodes an FT_PIXEL_MODE_MONO bitmap into alpha coverage.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Decodes an FT_PIXEL_MODE_GRAY bitmap into either alpha coverage or an own-color diagnostic view.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <param name="view">The desired visual interpretation of the grayscale samples.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Decodes FT_PIXEL_MODE_GRAY2 or FT_PIXEL_MODE_GRAY4 into alpha coverage.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <param name="bitsPerPixel">The packed bits per pixel, either 2 or 4.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Decodes an FT_PIXEL_MODE_LCD bitmap by grouping horizontal RGB subpixels.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Decodes an FT_PIXEL_MODE_LCD_V bitmap by grouping vertical RGB subpixels.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Decodes an FT_PIXEL_MODE_BGRA bitmap into own-color pixels.</summary>
    /// <param name="slot">The source glyph slot.</param>
    /// <param name="bitmap">The source FreeType bitmap.</param>
    /// <returns>A managed glyph image.</returns>
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

    /// <summary>Returns a pointer to one bitmap row, handling either pitch direction.</summary>
    /// <param name="bitmap">The FreeType bitmap whose row is needed.</param>
    /// <param name="y">The top-down row index.</param>
    /// <returns>A pointer to the requested row.</returns>
    private static nint RowPointer(FtBitmap bitmap, int y)
    {
        var pitch = bitmap.Pitch;
        return pitch >= 0 ? bitmap.Buffer + y * pitch : bitmap.Buffer + (checked((int)bitmap.Rows) - 1 - y) * -pitch;
    }

    /// <summary>Writes a tintable coverage sample into an RGBA buffer.</summary>
    /// <param name="rgba">The destination RGBA buffer.</param>
    /// <param name="width">The number of pixels in each row.</param>
    /// <param name="x">The x coordinate to write.</param>
    /// <param name="y">The y coordinate to write.</param>
    /// <param name="alpha">The alpha coverage sample.</param>
    private static void WriteCoverage(byte[] rgba, int width, int x, int y, byte alpha) => WriteOwnColor(rgba, width, x, y, 255, 255, 255, alpha);

    /// <summary>Writes a color sample into an RGBA buffer.</summary>
    /// <param name="rgba">The destination RGBA buffer.</param>
    /// <param name="width">The number of pixels in each row.</param>
    /// <param name="x">The x coordinate to write.</param>
    /// <param name="y">The y coordinate to write.</param>
    /// <param name="r">The red sample.</param>
    /// <param name="g">The green sample.</param>
    /// <param name="b">The blue sample.</param>
    /// <param name="a">The alpha sample.</param>
    private static void WriteOwnColor(byte[] rgba, int width, int x, int y, byte r, byte g, byte b, byte a)
    {
        var offset = (y * width + x) * 4;
        rgba[offset] = r;
        rgba[offset + 1] = g;
        rgba[offset + 2] = b;
        rgba[offset + 3] = a;
    }
}

/// <summary>Simple 24-bit canvas that writes PNG files through BigGustave.</summary>
internal sealed class PngCanvas
{
    private readonly byte[] pixels;

    /// <summary>Creates a new canvas filled with <paramref name="background"/>.</summary>
    /// <param name="width">The canvas width in pixels.</param>
    /// <param name="height">The canvas height in pixels.</param>
    /// <param name="background">The background color.</param>
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

    /// <summary>The canvas width in pixels.</summary>
    public int Width { get; }

    /// <summary>The canvas height in pixels.</summary>
    public int Height { get; }

    /// <summary>Draws a copied FreeType glyph at a pen origin and baseline.</summary>
    /// <param name="glyph">The glyph bitmap to draw.</param>
    /// <param name="originX">The pen origin x coordinate.</param>
    /// <param name="baselineY">The baseline y coordinate.</param>
    /// <param name="tint">The tint used when <paramref name="glyph"/> stores coverage only.</param>
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

    /// <summary>Draws a filled rectangle clipped to the canvas.</summary>
    /// <param name="x">The left coordinate.</param>
    /// <param name="y">The top coordinate.</param>
    /// <param name="width">The rectangle width.</param>
    /// <param name="height">The rectangle height.</param>
    /// <param name="color">The fill color.</param>
    public void DrawRectangle(int x, int y, int width, int height, DemoColor color)
    {
        for (var row = Math.Max(0, y); row < Math.Min(Height, y + height); row++)
        {
            for (var column = Math.Max(0, x); column < Math.Min(Width, x + width); column++)
                SetPixel(column, row, color);
        }
    }

    /// <summary>Draws a line with nearest-neighbor stepping.</summary>
    /// <param name="x0">The start x coordinate.</param>
    /// <param name="y0">The start y coordinate.</param>
    /// <param name="x1">The end x coordinate.</param>
    /// <param name="y1">The end y coordinate.</param>
    /// <param name="color">The line color.</param>
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

    /// <summary>Draws a small filled circle, used to mark outline points.</summary>
    /// <param name="centerX">The circle center x coordinate.</param>
    /// <param name="centerY">The circle center y coordinate.</param>
    /// <param name="radius">The circle radius in pixels.</param>
    /// <param name="color">The fill color.</param>
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

    /// <summary>Saves the canvas as a PNG file using BigGustave.</summary>
    /// <param name="path">The output path to write.</param>
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

    /// <summary>Writes an opaque pixel if it lies inside the canvas.</summary>
    /// <param name="x">The target x coordinate.</param>
    /// <param name="y">The target y coordinate.</param>
    /// <param name="color">The color to write.</param>
    private void SetPixel(int x, int y, DemoColor color)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height)
            return;

        var offset = (y * Width + x) * 3;
        pixels[offset] = color.R;
        pixels[offset + 1] = color.G;
        pixels[offset + 2] = color.B;
    }

    /// <summary>Alpha-blends one pixel if it lies inside the canvas.</summary>
    /// <param name="x">The target x coordinate.</param>
    /// <param name="y">The target y coordinate.</param>
    /// <param name="r">The source red channel.</param>
    /// <param name="g">The source green channel.</param>
    /// <param name="b">The source blue channel.</param>
    /// <param name="alpha">The source alpha channel.</param>
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

/// <summary>Rendering helpers that keep repetitive glyph-copying code out of the demo's main path.</summary>
internal static class FreeTypeDrawing
{
    /// <summary>Reads the face record for a live FT_Face handle.</summary>
    /// <param name="face">The FT_Face handle.</param>
    /// <returns>The current managed copy of FT_FaceRec.</returns>
    public static FtFaceRec ReadFace(nint face) => Marshal.PtrToStructure<FtFaceRec>(face);

    /// <summary>Returns the glyph slot pointer stored on a live face.</summary>
    /// <param name="face">The FT_Face handle.</param>
    /// <returns>The current FT_GlyphSlot pointer.</returns>
    public static nint GlyphSlot(nint face) => ReadFace(face).Glyph;

    /// <summary>Reads the current glyph slot record for a live face.</summary>
    /// <param name="face">The FT_Face handle.</param>
    /// <returns>The current managed copy of FT_GlyphSlotRec.</returns>
    public static FtGlyphSlotRec ReadGlyphSlot(nint face) => Marshal.PtrToStructure<FtGlyphSlotRec>(GlyphSlot(face));

    /// <summary>Copies the current glyph slot bitmap into a managed image.</summary>
    /// <param name="face">The FT_Face handle whose current glyph slot should be copied.</param>
    /// <param name="view">The desired bitmap interpretation.</param>
    /// <returns>A managed glyph image.</returns>
    public static GlyphImage CurrentGlyph(nint face, GlyphPixelView view = GlyphPixelView.Coverage) => GlyphImage.FromSlot(ReadGlyphSlot(face), view);

    /// <summary>Draws ASCII text using the face's current size, optionally applying kerning.</summary>
    /// <param name="ft">The FreeType binding used for glyph loading.</param>
    /// <param name="face">The FT_Face handle.</param>
    /// <param name="canvas">The destination canvas.</param>
    /// <param name="text">The ASCII text to draw.</param>
    /// <param name="x">The starting pen x coordinate.</param>
    /// <param name="baselineY">The text baseline y coordinate.</param>
    /// <param name="useKerning">Whether to apply FT_Get_Kerning between adjacent glyphs.</param>
    /// <param name="color">The text color.</param>
    public static void DrawTextCurrentSize(
        Ft ft,
        nint face,
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

            var glyphIndex = ft.GetCharIndex(face, FreeTypeValues.ULong(character));
            if (glyphIndex == 0)
                continue;

            if (useKerning && previousGlyph != 0)
                penX += KerningPixels(ft, face, previousGlyph, glyphIndex);

            RequireDraw(ft.LoadGlyph(face, glyphIndex, Ft.LoadRender), "FT_Load_Glyph");
            var glyph = CurrentGlyph(face);
            canvas.DrawGlyph(glyph, penX, baselineY, color);
            penX += glyph.AdvanceX;
            previousGlyph = glyphIndex;
        }
    }

    /// <summary>Computes horizontal kerning in integer pixels for two glyph indices.</summary>
    /// <param name="ft">The FreeType binding used for FT_Get_Kerning.</param>
    /// <param name="face">The FT_Face handle.</param>
    /// <param name="leftGlyph">The left glyph index.</param>
    /// <param name="rightGlyph">The right glyph index.</param>
    /// <returns>The horizontal kerning offset in pixels.</returns>
    private static unsafe int KerningPixels(Ft ft, nint face, uint leftGlyph, uint rightGlyph)
    {
        var kerning = new FtVector();
        RequireDraw(ft.GetKerning(face, leftGlyph, rightGlyph, (uint)FtKerningMode.KerningDefault, (nint)(&kerning)), "FT_Get_Kerning");
        return FreeTypeValues.Pixel26Dot6(kerning.X);
    }

    /// <summary>Throws if a draw-time FreeType call failed.</summary>
    /// <param name="error">The FT_Error value returned by a FreeType call.</param>
    /// <param name="cName">The C API name used in the failure message.</param>
    private static void RequireDraw(int error, string cName)
    {
        if (error != 0)
            throw new InvalidOperationException($"{cName} failed with FT_Error {error} during PNG drawing.");
    }
}
