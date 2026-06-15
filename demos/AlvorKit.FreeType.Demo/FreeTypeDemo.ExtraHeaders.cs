namespace AlvorKit.FreeType.Demo;

internal sealed unsafe partial class FreeTypeDemo
{
    /// <summary>Reports representative metadata, palette, variation, and size APIs from the extra FreeType headers.</summary>
    public void ReportExtraHeaderApis()
    {
        var face = pathFace;
        ft.GetFontFormat(face, out var fontFormat);
        ft.GetX11FontFormat(face, out var x11FontFormat);
        Console.WriteLine($"FT_Get_Font_Format / FT_Get_X11_Font_Format: {fontFormat ?? "(none)"} / {x11FontFormat ?? "(none)"}");
        var gaspFlags = (FtGaspFlags)ft.GetGasp(face, 16);
        Console.WriteLine($"FT_Get_Gasp at 16 ppem: {gaspFlags} (0x{(int)gaspFlags:X})");

        ReportSfntTables(face);
        ReportFormatSpecificMetadata(face);
        ReportPaletteAndVariations(face);
        ReportSizeObjectRoundTrip(face);
        ReportGeneratedMacroEnumSamples();
    }

    /// <summary>Reports representative SFNT table and name metadata.</summary>
    private void ReportSfntTables(FtFaceRec* face)
    {
        var head = ft.GetSfntTable(face, FtSfntTag.SfntHead);
        var nameCount = ft.GetSfntNameCount(face);
        Console.WriteLine($"FT_Get_Sfnt_Table(head): 0x{head:X}");
        Console.WriteLine($"FT_Get_Sfnt_Name_Count: {nameCount}");

        if (nameCount > 0)
        {
            Require("FT_Get_Sfnt_Name", ft.GetSfntName(face, 0, out var name));
            Console.WriteLine(
                "FT_Get_Sfnt_Name[0]: " +
                $"nameId={name.NameId}, platform={name.PlatformId}, text={DecodeSfntName(name)}");
        }

        CULong firstTableTag = default;
        CULong firstTableLength = default;
        var tableInfo = ft.SfntTableInfo(face, 0, (nint)(&firstTableTag), (nint)(&firstTableLength));
        if (tableInfo == 0)
        {
            Console.WriteLine(
                "FT_Sfnt_Table_Info[0]: " +
                $"{FormatSfntTag(firstTableTag)} ({FreeTypeValues.ToUInt64(firstTableLength)} bytes)");
        }
        else
        {
            ReportOptional("FT_Sfnt_Table_Info", tableInfo);
        }
    }

    /// <summary>Reports optional BDF and Windows FNT metadata APIs.</summary>
    private void ReportFormatSpecificMetadata(FtFaceRec* face)
    {
        var bdfError = ft.GetBdfProperty(face, "FONT_ASCENT", out var bdfProperty);
        ReportOptional("FT_Get_BDF_Property", bdfError);
        if (bdfError == 0)
            Console.WriteLine($"FT_Get_BDF_Property FONT_ASCENT: {FormatBdfProperty(bdfProperty)}");

        var winError = ft.GetWinFNTHeader(face, out var winHeader);
        ReportOptional("FT_Get_WinFNT_Header", winError);
        if (winError == 0)
            Console.WriteLine(
                "FT_Get_WinFNT_Header: " +
                $"{winHeader.PixelWidth}x{winHeader.PixelHeight}, charset {(FtWinFntId)winHeader.Charset}, ascent {winHeader.Ascent}");
    }

    /// <summary>Reports palette and multiple-master variation metadata.</summary>
    private void ReportPaletteAndVariations(FtFaceRec* face)
    {
        var paletteError = ft.PaletteDataGet(face, out var palette);
        ReportOptional("FT_Palette_Data_Get", paletteError);
        if (paletteError == 0)
        {
            var firstPaletteFlags = palette.NumPalettes > 0 && palette.PaletteFlags != 0
                ? (FtPaletteFlags)(ushort)Marshal.ReadInt16(palette.PaletteFlags)
                : (FtPaletteFlags)0;
            Console.WriteLine(
                "FT_Palette_Data_Get: " +
                $"{palette.NumPalettes} palettes, {palette.NumPaletteEntries} entries, first flags {firstPaletteFlags}");
        }

        var mmError = ft.GetMmVar(face, out var mmVar);
        ReportOptional("FT_Get_MM_Var", mmError);
        if (mmError == 0 && mmVar != null)
        {
            var axisFlags = (FtVarAxisFlags)0;
            if (mmVar->NumAxis > 0 && ft.GetVarAxisFlags(mmVar, 0, out var rawAxisFlags) == 0)
                axisFlags = (FtVarAxisFlags)rawAxisFlags;
            Console.WriteLine(
                "FT_Get_MM_Var: " +
                $"{mmVar->NumAxis} axes, {mmVar->NumNamedstyles} named styles, first axis {FormatFirstVariationAxis(mmVar)}, flags {axisFlags}");
            Require("FT_Done_MM_Var", ft.DoneMmVar(library, mmVar));
        }

        var instanceError = ft.GetDefaultNamedInstance(face, out var defaultInstance);
        ReportOptional("FT_Get_Default_Named_Instance", instanceError);
        if (instanceError == 0)
            Console.WriteLine($"FT_Get_Default_Named_Instance: {defaultInstance}");
    }

    /// <summary>Shows generated macro enum groups that are not otherwise exercised by this font walkthrough.</summary>
    private void ReportGeneratedMacroEnumSamples()
    {
        var moduleFlags = FtModuleFlags.FontDriver | FtModuleFlags.DriverScalable;
        var rasterFlags = FtRasterFlags.Aa | FtRasterFlags.Clip;
        Console.WriteLine($"Generated FT_MODULE_* / FT_RASTER_FLAG_* enums: {moduleFlags}; {rasterFlags}");
    }

    /// <summary>Creates, activates, and disposes an extra FreeType size object.</summary>
    private void ReportSizeObjectRoundTrip(FtFaceRec* face)
    {
        var originalSize = FreeTypeDrawing.ReadFace(face).Size;
        var newSizeError = ft.NewSize(face, out var size);
        ReportOptional("FT_New_Size", newSizeError);
        if (newSizeError != 0 || size == null)
            return;

        try
        {
            Require("FT_Activate_Size", ft.ActivateSize(size));
            Require("FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 24));
            Console.WriteLine("FT_New_Size / FT_Activate_Size / FT_Done_Size: extra size round-trip complete");
        }
        finally
        {
            if (originalSize != null)
                _ = ft.ActivateSize(originalSize);
            _ = ft.DoneSize(size);
        }
    }

    /// <summary>Reports a detached glyph bounding box and bitmap conversion result.</summary>
    private void ReportDetachedGlyphBox(FtFaceRec* face, uint glyphIndex)
    {
        Require("FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, FtLoadFlags.Default));
        Require("FT_Get_Glyph", ft.GetGlyph(FreeTypeDrawing.GlyphSlot(face), out var glyphHandle));

        try
        {
            ft.GlyphGetCBox(glyphHandle, FtGlyphBBoxMode.GlyphBboxPixels, out var glyphBox);
            Console.WriteLine($"FT_Get_Glyph / FT_Glyph_Get_CBox 'B': {FormatBoxPixels(glyphBox)}");

            var glyphToBitmapError = ft.GlyphToBitmap((nint)(&glyphHandle), FtRenderMode.RenderModeNormal, null, destroy: true);
            ReportOptional("FT_Glyph_To_Bitmap", glyphToBitmapError);
        }
        finally
        {
            if (glyphHandle != null)
                ft.DoneGlyph(glyphHandle);
        }
    }

    /// <summary>Decodes an SFNT name record into display text.</summary>
    private string DecodeSfntName(FtSfntName name)
    {
        if (name.String == 0 || name.StringLen == 0)
            return "(empty)";

        var bytes = new ReadOnlySpan<byte>((void*)name.String, checked((int)name.StringLen));
        if ((name.PlatformId is 0 or 3) && bytes.Length % 2 == 0)
            return Encoding.BigEndianUnicode.GetString(bytes);

        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>Formats a BDF property union for console output.</summary>
    private string FormatBdfProperty(FtBdfPropertyRec property) =>
        property.Type switch
        {
            FtBdfPropertyType.BdfPropertyTypeAtom => Marshal.PtrToStringUTF8(property.U.Atom) ?? "(null)",
            FtBdfPropertyType.BdfPropertyTypeInteger => property.U.Integer.ToString(),
            FtBdfPropertyType.BdfPropertyTypeCardinal => property.U.Cardinal.ToString(),
            _ => property.Type.ToString(),
        };

    /// <summary>Formats the first variation axis in an MM var record.</summary>
    private string FormatFirstVariationAxis(FtMmVar* mmVar)
    {
        if (mmVar->NumAxis == 0 || mmVar->Axis == null)
            return "(none)";

        var axis = mmVar->Axis[0];
        var name = Marshal.PtrToStringUTF8(axis.Name) ?? FormatSfntTag(axis.Tag);
        return $"{name} {FreeTypeValues.Fixed16Dot16(axis.Minimum):0.##}-" +
            $"{FreeTypeValues.Fixed16Dot16(axis.Maximum):0.##}";
    }

    /// <summary>Formats a numeric SFNT table tag as four printable characters.</summary>
    private string FormatSfntTag(CULong tag)
    {
        var value = (uint)FreeTypeValues.ToUInt64(tag);
        Span<char> chars =
        [
            PrintableAscii((byte)(value >> 24)),
            PrintableAscii((byte)(value >> 16)),
            PrintableAscii((byte)(value >> 8)),
            PrintableAscii((byte)value),
        ];
        return new string(chars);
    }

    /// <summary>Returns a printable ASCII character or a placeholder.</summary>
    private char PrintableAscii(byte value) => value is >= 0x20 and <= 0x7E ? (char)value : '?';

    /// <summary>Formats a 26.6 fixed-point bounding box in pixels.</summary>
    private string FormatBox26Dot6(FtBBox box) =>
        $"[{FreeTypeValues.Pixel26Dot6(box.XMin)}, {FreeTypeValues.Pixel26Dot6(box.YMin)}].." +
        $"[{FreeTypeValues.Pixel26Dot6(box.XMax)}, {FreeTypeValues.Pixel26Dot6(box.YMax)}] px";

    /// <summary>Formats an integer-pixel bounding box.</summary>
    private string FormatBoxPixels(FtBBox box) =>
        $"[{FreeTypeValues.ToInt64(box.XMin)}, {FreeTypeValues.ToInt64(box.YMin)}].." +
        $"[{FreeTypeValues.ToInt64(box.XMax)}, {FreeTypeValues.ToInt64(box.YMax)}] px";
}

