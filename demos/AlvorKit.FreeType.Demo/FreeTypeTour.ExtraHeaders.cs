namespace AlvorKit.FreeType.Demo;

internal static unsafe partial class FreeTypeTour
{
    /// <summary>Reports representative metadata, palette, variation, and size APIs from the extra FreeType headers.</summary>
    public static void ReportExtraHeaderApis(Ft ft, nint library, FtFaceRec* face)
    {
        ft.GetFontFormat(face, out var fontFormat);
        ft.GetX11FontFormat(face, out var x11FontFormat);
        Console.WriteLine($"FT_Get_Font_Format / FT_Get_X11_Font_Format: {fontFormat ?? "(none)"} / {x11FontFormat ?? "(none)"}");
        Console.WriteLine($"FT_Get_Gasp at 16 ppem: 0x{ft.GetGasp(face, 16):X}");

        ReportSfntTables(ft, face);
        ReportFormatSpecificMetadata(ft, face);
        ReportPaletteAndVariations(ft, library, face);
        ReportSizeObjectRoundTrip(ft, face);
    }

    private static void ReportSfntTables(Ft ft, FtFaceRec* face)
    {
        var head = ft.GetSfntTable(face, FtSfntTag.SfntHead);
        var nameCount = ft.GetSfntNameCount(face);
        Console.WriteLine($"FT_Get_Sfnt_Table(head): 0x{head:X}");
        Console.WriteLine($"FT_Get_Sfnt_Name_Count: {nameCount}");

        if (nameCount > 0)
        {
            FreeTypeStatus.Require(ft, "FT_Get_Sfnt_Name", ft.GetSfntName(face, 0, out var name));
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
            FreeTypeStatus.ReportOptional(ft, "FT_Sfnt_Table_Info", tableInfo);
        }
    }

    private static void ReportFormatSpecificMetadata(Ft ft, FtFaceRec* face)
    {
        var bdfError = ft.GetBdfProperty(face, "FONT_ASCENT", out var bdfProperty);
        FreeTypeStatus.ReportOptional(ft, "FT_Get_BDF_Property", bdfError);
        if (bdfError == 0)
            Console.WriteLine($"FT_Get_BDF_Property FONT_ASCENT: {FormatBdfProperty(bdfProperty)}");

        var winError = ft.GetWinFNTHeader(face, out var winHeader);
        FreeTypeStatus.ReportOptional(ft, "FT_Get_WinFNT_Header", winError);
        if (winError == 0)
            Console.WriteLine($"FT_Get_WinFNT_Header: {winHeader.PixelWidth}x{winHeader.PixelHeight}, ascent {winHeader.Ascent}");
    }

    private static void ReportPaletteAndVariations(Ft ft, nint library, FtFaceRec* face)
    {
        var paletteError = ft.PaletteDataGet(face, out var palette);
        FreeTypeStatus.ReportOptional(ft, "FT_Palette_Data_Get", paletteError);
        if (paletteError == 0)
            Console.WriteLine($"FT_Palette_Data_Get: {palette.NumPalettes} palettes, {palette.NumPaletteEntries} entries");

        var mmError = ft.GetMmVar(face, out var mmVar);
        FreeTypeStatus.ReportOptional(ft, "FT_Get_MM_Var", mmError);
        if (mmError == 0 && mmVar != null)
        {
            Console.WriteLine(
                "FT_Get_MM_Var: " +
                $"{mmVar->NumAxis} axes, {mmVar->NumNamedstyles} named styles, first axis {FormatFirstVariationAxis(mmVar)}");
            FreeTypeStatus.Require(ft, "FT_Done_MM_Var", ft.DoneMmVar(library, mmVar));
        }

        var instanceError = ft.GetDefaultNamedInstance(face, out var defaultInstance);
        FreeTypeStatus.ReportOptional(ft, "FT_Get_Default_Named_Instance", instanceError);
        if (instanceError == 0)
            Console.WriteLine($"FT_Get_Default_Named_Instance: {defaultInstance}");
    }

    private static void ReportSizeObjectRoundTrip(Ft ft, FtFaceRec* face)
    {
        var originalSize = FreeTypeDrawing.ReadFace(face).Size;
        var newSizeError = ft.NewSize(face, out var size);
        FreeTypeStatus.ReportOptional(ft, "FT_New_Size", newSizeError);
        if (newSizeError != 0 || size == null)
            return;

        try
        {
            FreeTypeStatus.Require(ft, "FT_Activate_Size", ft.ActivateSize(size));
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 24));
            Console.WriteLine("FT_New_Size / FT_Activate_Size / FT_Done_Size: extra size round-trip complete");
        }
        finally
        {
            if (originalSize != null)
                _ = ft.ActivateSize(originalSize);
            _ = ft.DoneSize(size);
        }
    }

    private static void ReportDetachedGlyphBox(Ft ft, FtFaceRec* face, uint glyphIndex)
    {
        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, Ft.LoadDefault));
        FreeTypeStatus.Require(ft, "FT_Get_Glyph", ft.GetGlyph(FreeTypeDrawing.GlyphSlot(face), out var glyphHandle));

        try
        {
            FtBBox glyphBox;
            ft.GlyphGetCBox(glyphHandle, FtGlyphBBoxMode.GlyphBboxPixels, &glyphBox);
            Console.WriteLine($"FT_Get_Glyph / FT_Glyph_Get_CBox 'B': {FormatBoxPixels(glyphBox)}");

            var glyphToBitmapError = ft.GlyphToBitmap((nint)(&glyphHandle), FtRenderMode.RenderModeNormal, null, destroy: true);
            FreeTypeStatus.ReportOptional(ft, "FT_Glyph_To_Bitmap", glyphToBitmapError);
        }
        finally
        {
            if (glyphHandle != null)
                ft.DoneGlyph(glyphHandle);
        }
    }

    private static string DecodeSfntName(FtSfntName name)
    {
        if (name.String == 0 || name.StringLen == 0)
            return "(empty)";

        var bytes = new ReadOnlySpan<byte>((void*)name.String, checked((int)name.StringLen));
        if ((name.PlatformId is 0 or 3) && bytes.Length % 2 == 0)
            return Encoding.BigEndianUnicode.GetString(bytes);

        return Encoding.UTF8.GetString(bytes);
    }

    private static string FormatBdfProperty(FtBdfPropertyRec property) =>
        property.Type switch
        {
            FtBdfPropertyType.BdfPropertyTypeAtom => Marshal.PtrToStringUTF8(property.U.Atom) ?? "(null)",
            FtBdfPropertyType.BdfPropertyTypeInteger => property.U.Integer.ToString(),
            FtBdfPropertyType.BdfPropertyTypeCardinal => property.U.Cardinal.ToString(),
            _ => property.Type.ToString(),
        };

    private static string FormatFirstVariationAxis(FtMmVar* mmVar)
    {
        if (mmVar->NumAxis == 0 || mmVar->Axis == null)
            return "(none)";

        var axis = mmVar->Axis[0];
        var name = Marshal.PtrToStringUTF8(axis.Name) ?? FormatSfntTag(axis.Tag);
        return $"{name} {FreeTypeValues.Fixed16Dot16(axis.Minimum):0.##}-" +
            $"{FreeTypeValues.Fixed16Dot16(axis.Maximum):0.##}";
    }

    private static string FormatSfntTag(CULong tag)
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

    private static char PrintableAscii(byte value) => value is >= 0x20 and <= 0x7E ? (char)value : '?';

    private static string FormatBox26Dot6(FtBBox box) =>
        $"[{FreeTypeValues.Pixel26Dot6(box.XMin)}, {FreeTypeValues.Pixel26Dot6(box.YMin)}].." +
        $"[{FreeTypeValues.Pixel26Dot6(box.XMax)}, {FreeTypeValues.Pixel26Dot6(box.YMax)}] px";

    private static string FormatBoxPixels(FtBBox box) =>
        $"[{FreeTypeValues.ToInt64(box.XMin)}, {FreeTypeValues.ToInt64(box.YMin)}].." +
        $"[{FreeTypeValues.ToInt64(box.XMax)}, {FreeTypeValues.ToInt64(box.YMax)}] px";
}
