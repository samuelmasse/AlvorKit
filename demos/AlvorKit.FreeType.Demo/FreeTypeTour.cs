namespace AlvorKit.FreeType.Demo;

internal static unsafe class FreeTypeTour
{
    public static void ReportErrorString(Ft ft)
    {
        ft.ErrorString(0, out var successText);
        ft.ErrorString(1, out var sampleText);
        Console.WriteLine($"FT_Error_String: 0 => {successText ?? "(no string)"}, 1 => {sampleText ?? "(no string)"}");
    }

    public static void ReportLibraryVersion(Ft ft, nint library)
    {
        ft.LibraryVersion(library, out var major, out var minor, out var patch);
        Console.WriteLine($"FT_Library_Version: {major}.{minor}.{patch}");
    }

    public static void ReportFaceBasics(Ft ft, FtFaceRec* face, string fontPath)
    {
        var faceRec = FreeTypeDrawing.ReadFace(face);
        var family = Marshal.PtrToStringUTF8(faceRec.FamilyName) ?? "(no family)";
        var style = Marshal.PtrToStringUTF8(faceRec.StyleName) ?? "(no style)";
        ft.GetPostscriptName(face, out var postscriptName);

        Console.WriteLine($"FT_FaceRec: {family} {style}, {FreeTypeValues.ToInt64(faceRec.NumGlyphs)} glyphs");
        Console.WriteLine($"FT_Get_Postscript_Name: {postscriptName ?? "(none)"}");
        Console.WriteLine($"FT_Get_FSType_Flags: 0x{ft.GetFSTypeFlags(face):X4}");
        Console.WriteLine($"FT_Face_Properties: no-op reset call for {Path.GetFileName(fontPath)}");
        FreeTypeStatus.Require(ft, "FT_Face_Properties", ft.FaceProperties(face, []));
        Console.WriteLine($"FT_Face_CheckTrueTypePatents: {ft.FaceCheckTrueTypePatents(face)}");
        Console.WriteLine($"FT_Face_SetUnpatentedHinting: {ft.FaceSetUnpatentedHinting(face, true)}");
    }

    public static void ReportReferenceCounting(Ft ft, FtFaceRec* face)
    {
        FreeTypeStatus.Require(ft, "FT_Reference_Face", ft.ReferenceFace(face));
        FreeTypeStatus.Require(ft, "FT_Done_Face", ft.DoneFace(face));
        Console.WriteLine("FT_Reference_Face / FT_Done_Face: extra reference round-trip complete");
    }

    public static void ReportOptionalAttachments(Ft ft, FtFaceRec* face, string fontPath)
    {
        FreeTypeStatus.ReportOptional(ft, "FT_Attach_File", ft.AttachFile(face, fontPath));

        var pathBytes = Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* path = pathBytes)
        {
            var args = new FtOpenArgs
            {
                Flags = Ft.OpenPathname,
                Pathname = (nint)path,
            };
            FreeTypeStatus.ReportOptional(ft, "FT_Attach_Stream", ft.AttachStream(face, &args));
        }
    }

    public static void ReportOptionalBitmapStrike(Ft ft, FtFaceRec* face)
    {
        var faceRec = FreeTypeDrawing.ReadFace(face);
        Console.WriteLine($"FT_FaceRec.num_fixed_sizes: {faceRec.NumFixedSizes}");
        FreeTypeStatus.ReportOptional(ft, "FT_Select_Size", ft.SelectSize(face, 0));
    }

    public static string ExportNewFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'F', Ft.LoadRender));

        var path = Path.Combine(outputRoot, "01-ft-new-face-load-char.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Gold);
        return Path.GetFullPath(path);
    }

    public static string ExportMemoryFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'M', Ft.LoadRender));

        var path = Path.Combine(outputRoot, "02-ft-new-memory-face.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Cyan);
        return Path.GetFullPath(path);
    }

    public static string ExportOpenFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        var glyphIndex = ft.GetCharIndex(face, 'O');
        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, Ft.LoadRender));

        var path = Path.Combine(outputRoot, "03-ft-open-face-load-glyph.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Green);
        return Path.GetFullPath(path);
    }

    public static string ExportSizingApis(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(980, 300, DemoColor.Background);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 34));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Set_Pixel_Sizes", 34, 78, useKerning: true, DemoColor.Gold);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "pixels choose target bitmap height", 390, 78, useKerning: true, DemoColor.White);

        FreeTypeStatus.Require(
            ft,
            "FT_Set_Char_Size",
            ft.SetCharSize(face, FreeTypeValues.Long(0), FreeTypeValues.Long(34 * 64), 96, 96));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Set_Char_Size", 34, 158, useKerning: true, DemoColor.Cyan);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "points plus device resolution", 390, 158, useKerning: true, DemoColor.White);

        var request = new FtSizeRequestRec
        {
            Type = FtSizeRequestType.SizeRequestTypeNominal,
            Height = FreeTypeValues.Long(34 * 64),
            HoriResolution = 96,
            VertResolution = 96,
        };
        FreeTypeStatus.Require(ft, "FT_Request_Size", ft.RequestSize(face, in request));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Request_Size", 34, 238, useKerning: true, DemoColor.Green);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "explicit FT_Size_RequestRec", 390, 238, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "04-sizing-apis.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportRenderModes(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(1100, 300, DemoColor.Background);
        var modes = new (string Label, FtRenderMode Mode, GlyphPixelView View)[]
        {
            ("NORMAL", FtRenderMode.RenderModeNormal, GlyphPixelView.Coverage),
            ("LIGHT", FtRenderMode.RenderModeLight, GlyphPixelView.Coverage),
            ("MONO", FtRenderMode.RenderModeMono, GlyphPixelView.Coverage),
            ("LCD", FtRenderMode.RenderModeLcd, GlyphPixelView.OwnColor),
            ("LCD_V", FtRenderMode.RenderModeLcdV, GlyphPixelView.OwnColor),
            ("SDF", FtRenderMode.RenderModeSdf, GlyphPixelView.OwnColor),
        };

        for (var i = 0; i < modes.Length; i++)
        {
            var x = 36 + i * 176;
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 88));
            FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '@', Ft.LoadDefault));
            var error = ft.RenderGlyph(FreeTypeDrawing.GlyphSlot(face), modes[i].Mode);
            if (error == 0)
                canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face, modes[i].View), x + 42, 154, DemoColor.White);
            else
                FreeTypeStatus.ReportOptional(ft, $"FT_Render_Glyph {modes[i].Label}", error);

            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, modes[i].Label, x, 245, useKerning: true, DemoColor.White);
        }

        var path = Path.Combine(outputRoot, "05-render-modes.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportLoadFlags(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(1050, 300, DemoColor.Background);
        var flags = new (string Label, int Flags, DemoColor Color)[]
        {
            ("FT_LOAD_RENDER", Ft.LoadRender, DemoColor.Gold),
            ("NO_HINTING", Ft.LoadNoHinting | Ft.LoadRender, DemoColor.Cyan),
            ("FORCE_AUTOHINT", Ft.LoadForceAutohint | Ft.LoadRender, DemoColor.Green),
            ("MONOCHROME", Ft.LoadMonochrome | Ft.LoadRender, DemoColor.White),
            ("NO_BITMAP", Ft.LoadNoBitmap | Ft.LoadRender, DemoColor.Red),
        };

        for (var i = 0; i < flags.Length; i++)
        {
            var x = 40 + i * 200;
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 88));
            FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'g', flags[i].Flags));
            canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), x + 54, 154, flags[i].Color);
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 18));
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, flags[i].Label, x, 245, useKerning: true, DemoColor.White);
        }

        var path = Path.Combine(outputRoot, "06-load-flags.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportTransform(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(720, 300, DemoColor.Background);
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 112));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'T', Ft.LoadRender));
        canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), 110, 176, DemoColor.Cyan);

        var matrix = RotationMatrix(-14);
        var delta = new FtVector { X = FreeTypeValues.Long(14 * 64) };
        ft.SetTransform(face, in matrix, in delta);

        ft.GetTransform(face, out var roundTripMatrix, out var roundTripDelta);
        Console.WriteLine(
            "FT_Set_Transform / FT_Get_Transform: " +
            $"xx={FreeTypeValues.ToInt64(roundTripMatrix.Xx)}, deltaX={FreeTypeValues.ToInt64(roundTripDelta.X)}");

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'T', Ft.LoadRender));
        canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), 390, 176, DemoColor.Gold);
        ft.SetTransform(face, null, null);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "identity", 95, 250, useKerning: true, DemoColor.White);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Set_Transform", 350, 250, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "07-transform.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportKerning(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(880, 330, DemoColor.Background);
        var a = ft.GetCharIndex(face, 'A');
        var v = ft.GetCharIndex(face, 'V');
        FreeTypeStatus.Require(ft, "FT_Get_Kerning", ft.GetKerning(face, a, v, FtKerningMode.KerningDefault, out var kerning));
        Console.WriteLine($"FT_Get_Kerning A/V: {FreeTypeValues.Pixel26Dot6(kerning.X)} px");
        FreeTypeStatus.ReportOptional(ft, "FT_Get_Track_Kerning", ft.GetTrackKerning(face, FreeTypeValues.Long(12 * 64), 0, out _));

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 76));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "AVATAR", 70, 130, useKerning: false, DemoColor.Red);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "AVATAR", 70, 250, useKerning: true, DemoColor.Green);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 19));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "without FT_Get_Kerning", 500, 122, useKerning: true, DemoColor.White);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "with FT_Get_Kerning", 500, 242, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "08-kerning.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportCharmapGrid(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var firstCharmap = (FtCharMapRec*)Marshal.ReadIntPtr(FreeTypeDrawing.ReadFace(face).Charmaps);
        if (firstCharmap != null)
        {
            FreeTypeStatus.Require(ft, "FT_Set_Charmap", ft.SetCharmap(face, firstCharmap));
            Console.WriteLine($"FT_Get_Charmap_Index first charmap: {ft.GetCharmapIndex(firstCharmap)}");
        }

        FreeTypeStatus.Require(ft, "FT_Select_Charmap", ft.SelectCharmap(face, FtEncoding.EncodingUnicode));
        var activeCharmap = FreeTypeDrawing.ReadFace(face).Charmap;
        Console.WriteLine($"FT_Get_Charmap_Index unicode charmap: {ft.GetCharmapIndex(activeCharmap)}");
        Console.WriteLine($"FT_Get_Char_Index 'A': {ft.GetCharIndex(face, 'A')}");

        var glyphs = new List<uint>();
        var charCode = ft.GetFirstChar(face, out var glyphIndex);
        while (glyphIndex != 0 && glyphs.Count < 48)
        {
            var scalar = FreeTypeValues.ToUInt64(charCode);
            if (scalar is >= 0x21 and <= 0x7E)
                glyphs.Add(glyphIndex);

            charCode = ft.GetNextChar(face, charCode, out glyphIndex);
        }

        var canvas = new PngCanvas(760, 430, DemoColor.Background);
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 21));
        FreeTypeDrawing.DrawTextCurrentSize(
            ft,
            face,
            canvas,
            "FT_Get_First_Char / FT_Get_Next_Char",
            36,
            48,
            useKerning: true,
            DemoColor.White);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 34));
        for (var i = 0; i < glyphs.Count; i++)
        {
            FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphs[i], Ft.LoadRender));
            var x = 44 + i % 12 * 58;
            var y = 112 + i / 12 * 70;
            canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), x, y, DemoColor.Gold);
        }

        var path = Path.Combine(outputRoot, "09-charmap-grid.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportNamesAndComposite(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var nameIndex = ft.GetNameIndex(face, "A");
        if (nameIndex == 0)
            nameIndex = ft.GetCharIndex(face, 'A');

        FreeTypeStatus.ReportOptional(ft, "FT_Get_Glyph_Name", ft.GetGlyphName(face, nameIndex, out var glyphNameText));

        Console.WriteLine($"FT_Get_Name_Index \"A\": {nameIndex}");
        Console.WriteLine($"FT_Get_Glyph_Name: {glyphNameText ?? "(none)"}");

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 104));
        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, nameIndex, Ft.LoadRender));
        var nameGlyph = FreeTypeDrawing.CurrentGlyph(face);

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '\u00E9', Ft.LoadNoRecurse));
        var compositeSlot = FreeTypeDrawing.ReadGlyphSlot(face);
        if (compositeSlot.NumSubglyphs > 0)
            ReportFirstSubglyph(ft, FreeTypeDrawing.GlyphSlot(face));
        else
            Console.WriteLine("FT_Get_SubGlyph_Info: current font did not expose a decomposed U+00E9 composite");

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '\u00E9', Ft.LoadRender));
        var compositeGlyph = FreeTypeDrawing.CurrentGlyph(face);

        var canvas = new PngCanvas(680, 300, DemoColor.Background);
        canvas.DrawGlyph(nameGlyph, 120, 174, DemoColor.Cyan);
        canvas.DrawGlyph(compositeGlyph, 410, 174, DemoColor.Gold);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Get_Name_Index", 62, 250, useKerning: true, DemoColor.White);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Get_SubGlyph_Info", 350, 250, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "10-names-and-composite.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportOutline(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 160));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'S', Ft.LoadNoBitmap | Ft.LoadNoHinting));

        var slot = FreeTypeDrawing.ReadGlyphSlot(face);
        var outline = slot.Outline;
        var points = ReadOutlinePoints(outline);
        var contours = ReadOutlineContours(outline);
        var canvas = new PngCanvas(620, 420, DemoColor.Background);

        if (points.Count > 0)
            DrawOutline(canvas, points, contours);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 21));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Outline from FT_GlyphSlotRec", 36, 386, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "11-outline-points.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static string ExportFixedPointAndVector(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var mulDiv = FreeTypeValues.ToInt64(ft.MulDiv(FreeTypeValues.Long(21), FreeTypeValues.Long(11), FreeTypeValues.Long(3)));
        var mulFix = FreeTypeValues.ToInt64(ft.MulFix(FreeTypeValues.Long(42), FreeTypeValues.Fixed(1.5)));
        var divFix = FreeTypeValues.ToInt64(ft.DivFix(FreeTypeValues.Long(1), FreeTypeValues.Long(3)));
        var round = FreeTypeValues.ToInt64(ft.RoundFix(FreeTypeValues.Fixed(2.4)));
        var ceil = FreeTypeValues.ToInt64(ft.CeilFix(FreeTypeValues.Fixed(2.4)));
        var floor = FreeTypeValues.ToInt64(ft.FloorFix(FreeTypeValues.Fixed(2.4)));

        var vector = new FtVector { X = FreeTypeValues.Long(120), Y = FreeTypeValues.Long(35) };
        var originalX = FreeTypeValues.ToInt64(vector.X);
        var originalY = FreeTypeValues.ToInt64(vector.Y);
        var matrix = RotationMatrix(28);
        ft.VectorTransform(&vector, &matrix);

        var canvas = new PngCanvas(820, 430, DemoColor.Background);
        DrawBar(canvas, 60, 110, (int)mulDiv, DemoColor.Gold);
        DrawBar(canvas, 160, 110, (int)mulFix, DemoColor.Cyan);
        DrawBar(canvas, 260, 110, (int)(divFix / 1024), DemoColor.Green);
        DrawBar(canvas, 360, 110, (int)(round / FreeTypeValues.FixedOne * 30), DemoColor.White);
        DrawBar(canvas, 460, 110, (int)(ceil / FreeTypeValues.FixedOne * 30), DemoColor.White);
        DrawBar(canvas, 560, 110, (int)(floor / FreeTypeValues.FixedOne * 30), DemoColor.White);

        var originX = 640;
        var originY = 285;
        canvas.DrawLine(originX, originY, originX + (int)originalX, originY - (int)originalY, DemoColor.Red);
        canvas.DrawLine(
            originX,
            originY,
            originX + (int)FreeTypeValues.ToInt64(vector.X),
            originY - (int)FreeTypeValues.ToInt64(vector.Y),
            DemoColor.Green);
        canvas.DrawCircle(originX, originY, 4, DemoColor.White);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 18));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "MulDiv MulFix DivFix Round Ceil Floor", 40, 360, useKerning: true, DemoColor.White);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Vector_Transform", 600, 360, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "12-fixed-point-vector.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    public static void ReportVariationSelectors(Ft ft, FtFaceRec* face)
    {
        const uint heart = 0x2764;
        const uint emojiVariation = 0xFE0F;

        Console.WriteLine($"FT_Face_GetCharVariantIndex: {ft.FaceGetCharVariantIndex(face, FreeTypeValues.ULong(heart), FreeTypeValues.ULong(emojiVariation))}");
        Console.WriteLine(
            "FT_Face_GetCharVariantIsDefault: " +
            ft.FaceGetCharVariantIsDefault(face, FreeTypeValues.ULong(heart), FreeTypeValues.ULong(emojiVariation)));
        Console.WriteLine($"FT_Face_GetVariantSelectors: {FormatUInt32List(ft.FaceGetVariantSelectors(face))}");
        Console.WriteLine($"FT_Face_GetVariantsOfChar: {FormatUInt32List(ft.FaceGetVariantsOfChar(face, FreeTypeValues.ULong(heart)))}");
        Console.WriteLine($"FT_Face_GetCharsOfVariant: {FormatUInt32List(ft.FaceGetCharsOfVariant(face, FreeTypeValues.ULong(emojiVariation)))}");
    }

    private static FtMatrix RotationMatrix(double degrees)
    {
        var radians = degrees * Math.PI / 180.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return new FtMatrix
        {
            Xx = FreeTypeValues.Fixed(cos),
            Xy = FreeTypeValues.Fixed(-sin),
            Yx = FreeTypeValues.Fixed(sin),
            Yy = FreeTypeValues.Fixed(cos),
        };
    }

    private static void ReportFirstSubglyph(Ft ft, FtGlyphSlotRec* glyphSlot)
    {
        FreeTypeStatus.Require(
            ft,
            "FT_Get_SubGlyph_Info",
            ft.GetSubGlyphInfo(glyphSlot, 0, out var index, out var flags, out var arg1, out var arg2, out _));
        Console.WriteLine($"FT_Get_SubGlyph_Info: index={index}, flags=0x{flags:X}, args=({arg1}, {arg2})");
    }

    private static List<FtVector> ReadOutlinePoints(FtOutline outline)
    {
        var points = new List<FtVector>(outline.NPoints);
        for (var i = 0; i < outline.NPoints; i++)
            points.Add(outline.Points[i]);

        return points;
    }

    private static List<short> ReadOutlineContours(FtOutline outline)
    {
        var contours = new List<short>(outline.NContours);
        for (var i = 0; i < outline.NContours; i++)
            contours.Add(Marshal.ReadInt16(outline.Contours, i * sizeof(short)));

        return contours;
    }

    private static void DrawOutline(PngCanvas canvas, List<FtVector> points, List<short> contours)
    {
        var minX = points.Min(point => FreeTypeValues.ToInt64(point.X));
        var maxX = points.Max(point => FreeTypeValues.ToInt64(point.X));
        var minY = points.Min(point => FreeTypeValues.ToInt64(point.Y));
        var maxY = points.Max(point => FreeTypeValues.ToInt64(point.Y));
        var scale = Math.Min(500.0 / Math.Max(1, maxX - minX), 310.0 / Math.Max(1, maxY - minY));

        var previousEnd = -1;
        foreach (var end in contours)
        {
            var start = previousEnd + 1;
            for (var i = start; i <= end; i++)
            {
                var next = i == end ? start : i + 1;
                var p0 = MapOutlinePoint(points[i], minX, maxY, scale);
                var p1 = MapOutlinePoint(points[next], minX, maxY, scale);
                canvas.DrawLine(p0.X, p0.Y, p1.X, p1.Y, DemoColor.Guide);
            }

            previousEnd = end;
        }

        foreach (var point in points)
        {
            var mapped = MapOutlinePoint(point, minX, maxY, scale);
            canvas.DrawCircle(mapped.X, mapped.Y, 3, DemoColor.Gold);
        }
    }

    private static (int X, int Y) MapOutlinePoint(FtVector point, long minX, long maxY, double scale) =>
        (
            60 + (int)Math.Round((FreeTypeValues.ToInt64(point.X) - minX) * scale),
            40 + (int)Math.Round((maxY - FreeTypeValues.ToInt64(point.Y)) * scale));

    private static void DrawBar(PngCanvas canvas, int x, int baselineY, int value, DemoColor color)
    {
        var height = Math.Clamp(Math.Abs(value), 8, 190);
        canvas.DrawRectangle(x, baselineY + 190 - height, 48, height, color);
    }

    private static string FormatUInt32List(uint* pointer)
    {
        if (pointer == null)
            return "(none)";

        var values = new List<uint>();
        for (var i = 0; i < 8; i++)
        {
            var value = pointer[i];
            if (value == 0)
                break;

            values.Add(value);
        }

        return values.Count == 0 ? "(empty)" : string.Join(", ", values.Select(value => $"U+{value:X4}"));
    }
}

internal static class FreeTypeStatus
{
    public static void Require(Ft ft, string cName, int error)
    {
        if (error == 0)
            return;

        throw new InvalidOperationException($"{cName} failed with FT_Error {error}: {Describe(ft, error)}");
    }

    public static void ReportOptional(Ft ft, string cName, int error)
    {
        if (error == 0)
            Console.WriteLine($"{cName}: supported");
        else
            Console.WriteLine($"{cName}: unavailable for this font ({error}, {Describe(ft, error)})");
    }

    public static string Describe(Ft ft, int error)
    {
        ft.ErrorString(error, out var value);
        return value ?? "native build did not expose an error string";
    }
}
