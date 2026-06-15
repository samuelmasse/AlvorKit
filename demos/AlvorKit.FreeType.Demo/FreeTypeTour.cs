namespace AlvorKit.FreeType.Demo;

internal static unsafe partial class FreeTypeTour
{
    /// <summary>Reports the generated string overload for FreeType error text.</summary>
    public static void ReportErrorString(Ft ft)
    {
        ft.ErrorString(0, out var successText);
        ft.ErrorString(1, out var sampleText);
        Console.WriteLine($"FT_Error_String: 0 => {successText ?? "(no string)"}, 1 => {sampleText ?? "(no string)"}");
    }

    /// <summary>Reports the loaded FreeType runtime version.</summary>
    public static void ReportLibraryVersion(Ft ft, nint library)
    {
        ft.LibraryVersion(library, out var major, out var minor, out var patch);
        Console.WriteLine($"FT_Library_Version: {major}.{minor}.{patch}");
    }

    /// <summary>Reports basic face metadata and simple face-level APIs.</summary>
    public static void ReportFaceBasics(Ft ft, FtFaceRec* face, string fontPath)
    {
        var faceRec = FreeTypeDrawing.ReadFace(face);
        var family = Marshal.PtrToStringUTF8(faceRec.FamilyName) ?? "(no family)";
        var style = Marshal.PtrToStringUTF8(faceRec.StyleName) ?? "(no style)";
        var faceFlags = (FtFaceFlags)FreeTypeValues.ToInt64(faceRec.FaceFlags);
        var rawStyleFlags = (int)FreeTypeValues.ToInt64(faceRec.StyleFlags);
        var fsTypeFlags = (FtFSTypeFlags)ft.GetFSTypeFlags(face);
        ft.GetPostscriptName(face, out var postscriptName);

        Console.WriteLine($"FT_FaceRec: {family} {style}, {FreeTypeValues.ToInt64(faceRec.NumGlyphs)} glyphs");
        Console.WriteLine($"FT_FaceRec flags: {faceFlags}; style flags: {FormatKnownStyleFlags(rawStyleFlags)}");
        Console.WriteLine($"FT_Get_Postscript_Name: {postscriptName ?? "(none)"}");
        Console.WriteLine($"FT_Get_FSType_Flags: {fsTypeFlags} (0x{(int)fsTypeFlags:X4})");
        Console.WriteLine($"FT_Face_Properties: no-op reset call for {Path.GetFileName(fontPath)}");
        FreeTypeStatus.Require(ft, "FT_Face_Properties", ft.FaceProperties(face, []));
        Console.WriteLine($"FT_Face_CheckTrueTypePatents: {ft.FaceCheckTrueTypePatents(face)}");
        Console.WriteLine($"FT_Face_SetUnpatentedHinting: {ft.FaceSetUnpatentedHinting(face, true)}");
    }

    /// <summary>Shows the extra reference and release round-trip for a face handle.</summary>
    public static void ReportReferenceCounting(Ft ft, FtFaceRec* face)
    {
        FreeTypeStatus.Require(ft, "FT_Reference_Face", ft.ReferenceFace(face));
        FreeTypeStatus.Require(ft, "FT_Done_Face", ft.DoneFace(face));
        Console.WriteLine("FT_Reference_Face / FT_Done_Face: extra reference round-trip complete");
    }

    /// <summary>Reports optional attachment APIs against the demo font.</summary>
    public static void ReportOptionalAttachments(Ft ft, FtFaceRec* face, string fontPath)
    {
        FreeTypeStatus.ReportOptional(ft, "FT_Attach_File", ft.AttachFile(face, fontPath));

        var pathBytes = Encoding.UTF8.GetBytes(fontPath + '\0');
        fixed (byte* path = pathBytes)
        {
            var args = new FtOpenArgs
            {
                Flags = (uint)FtOpenFlags.Pathname,
                Pathname = (nint)path,
            };
            FreeTypeStatus.ReportOptional(ft, "FT_Attach_Stream", ft.AttachStream(face, &args));
        }
    }

    /// <summary>Reports whether the face exposes selectable bitmap strikes.</summary>
    public static void ReportOptionalBitmapStrike(Ft ft, FtFaceRec* face)
    {
        var faceRec = FreeTypeDrawing.ReadFace(face);
        Console.WriteLine($"FT_FaceRec.num_fixed_sizes: {faceRec.NumFixedSizes}");
        FreeTypeStatus.ReportOptional(ft, "FT_Select_Size", ft.SelectSize(face, 0));
    }

    /// <summary>Exports the first glyph image loaded from the path-based face.</summary>
    public static string ExportNewFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'F', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Render)));

        var path = Path.Combine(outputRoot, "01-ft-new-face-load-char.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Gold);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports a glyph loaded from the memory-backed face path.</summary>
    public static string ExportMemoryFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'M', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Render)));

        var path = Path.Combine(outputRoot, "02-ft-new-memory-face.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Cyan);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports a glyph loaded by glyph index through the open-face path.</summary>
    public static string ExportOpenFaceGlyph(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 118));
        var glyphIndex = ft.GetCharIndex(face, 'O');
        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, FtLoadFlags.Render));

        var path = Path.Combine(outputRoot, "03-ft-open-face-load-glyph.png");
        FreeTypeDrawing.CurrentGlyph(face).SaveCentered(path, DemoColor.Background, DemoColor.Green);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports a comparison of FreeType size-selection APIs.</summary>
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

    /// <summary>Exports sample glyphs rendered with each FreeType render mode.</summary>
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
            FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '@', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Default)));
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

    /// <summary>Exports a visual comparison of common glyph load flags.</summary>
    public static string ExportLoadFlags(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(1050, 300, DemoColor.Background);
        var flags = new (string Label, FtLoadFlags Flags, DemoColor Color)[]
        {
            ("FT_LOAD_RENDER", FtLoadFlags.Render, DemoColor.Gold),
            ("NO_HINTING", FtLoadFlags.NoHinting | FtLoadFlags.Render, DemoColor.Cyan),
            ("FORCE_AUTOHINT", FtLoadFlags.ForceAutohint | FtLoadFlags.Render, DemoColor.Green),
            ("MONOCHROME", FtLoadFlags.Monochrome | FtLoadFlags.Render, DemoColor.White),
            ("NO_BITMAP", FtLoadFlags.NoBitmap | FtLoadFlags.Render, DemoColor.Red),
        };

        for (var i = 0; i < flags.Length; i++)
        {
            var x = 40 + i * 200;
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 88));
            FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'g', FreeTypeDrawing.LoadFlagBits(flags[i].Flags)));
            canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), x + 54, 154, flags[i].Color);
            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 18));
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, flags[i].Label, x, 245, useKerning: true, DemoColor.White);
        }

        var path = Path.Combine(outputRoot, "06-load-flags.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports a before-and-after view of a face transform.</summary>
    public static string ExportTransform(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var canvas = new PngCanvas(720, 300, DemoColor.Background);
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 112));
        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'T', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Render)));
        canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), 110, 176, DemoColor.Cyan);

        var matrix = RotationMatrix(-14);
        var delta = new FtVector { X = FreeTypeValues.Long(14 * 64) };
        ft.SetTransform(face, in matrix, in delta);

        ft.GetTransform(face, out var roundTripMatrix, out var roundTripDelta);
        Console.WriteLine(
            "FT_Set_Transform / FT_Get_Transform: " +
            $"xx={FreeTypeValues.ToInt64(roundTripMatrix.Xx)}, deltaX={FreeTypeValues.ToInt64(roundTripDelta.X)}");

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, 'T', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Render)));
        canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), 390, 176, DemoColor.Gold);
        ft.SetTransform(face, null, null);

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "identity", 95, 250, useKerning: true, DemoColor.White);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Set_Transform", 350, 250, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "07-transform.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports kerning output with and without pair adjustment.</summary>
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

    /// <summary>Exports a grid of glyphs discovered by walking the active charmap.</summary>
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
            FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphs[i], FtLoadFlags.Render));
            var x = 44 + i % 12 * 58;
            var y = 112 + i / 12 * 70;
            canvas.DrawGlyph(FreeTypeDrawing.CurrentGlyph(face), x, y, DemoColor.Gold);
        }

        var path = Path.Combine(outputRoot, "09-charmap-grid.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports glyph-name and composite-subglyph API examples.</summary>
    public static string ExportNamesAndComposite(Ft ft, FtFaceRec* face, string outputRoot)
    {
        var nameIndex = ft.GetNameIndex(face, "A");
        if (nameIndex == 0)
            nameIndex = ft.GetCharIndex(face, 'A');

        FreeTypeStatus.ReportOptional(ft, "FT_Get_Glyph_Name", ft.GetGlyphName(face, nameIndex, out var glyphNameText));

        Console.WriteLine($"FT_Get_Name_Index \"A\": {nameIndex}");
        Console.WriteLine($"FT_Get_Glyph_Name: {glyphNameText ?? "(none)"}");

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 104));
        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, nameIndex, FtLoadFlags.Render));
        var nameGlyph = FreeTypeDrawing.CurrentGlyph(face);

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '\u00E9', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.NoRecurse)));
        var compositeSlot = FreeTypeDrawing.ReadGlyphSlot(face);
        if (compositeSlot.NumSubglyphs > 0)
            ReportFirstSubglyph(ft, FreeTypeDrawing.GlyphSlot(face));
        else
            Console.WriteLine("FT_Get_SubGlyph_Info: current font did not expose a decomposed U+00E9 composite");

        FreeTypeStatus.Require(ft, "FT_Load_Char", ft.LoadChar(face, '\u00E9', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.Render)));
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

    /// <summary>Exports raw outline points and contour connectivity for one glyph.</summary>
    public static string ExportOutline(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 160));
        FreeTypeStatus.Require(
            ft,
            "FT_Load_Char",
            ft.LoadChar(face, 'S', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.NoBitmap | FtLoadFlags.NoHinting)));

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

    /// <summary>Exports fixed-point arithmetic and vector transform examples.</summary>
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

    /// <summary>Exports a visual comparison of control and exact bounding boxes from the outline helper APIs.</summary>
    public static string ExportOutlineGeometry(Ft ft, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 160));
        FreeTypeStatus.Require(
            ft,
            "FT_Load_Char",
            ft.LoadChar(face, 'Q', FreeTypeDrawing.LoadFlagBits(FtLoadFlags.NoBitmap | FtLoadFlags.NoHinting)));

        var slot = FreeTypeDrawing.ReadGlyphSlot(face);
        var outline = slot.Outline;
        var firstCurveTag = outline.NPoints > 0 && outline.Tags != 0 ? (FtCurveTag)Marshal.ReadByte(outline.Tags) : (FtCurveTag)0;
        Console.WriteLine($"FT_Outline flags: {(FtOutlineFlags)outline.Flags}; first point tag: {firstCurveTag}");
        var points = ReadOutlinePoints(outline);
        var contours = ReadOutlineContours(outline);
        var canvas = new PngCanvas(760, 460, DemoColor.Background);

        FtBBox controlBox;
        FtBBox exactBox;
        ft.OutlineGetCBox(&outline, out controlBox);
        FreeTypeStatus.Require(ft, "FT_Outline_Get_BBox", ft.OutlineGetBBox(&outline, out exactBox));
        var orientation = ft.OutlineGetOrientation(&outline);
        Console.WriteLine($"FT_Outline_Get_CBox: {FormatBox26Dot6(controlBox)}");
        Console.WriteLine($"FT_Outline_Get_BBox: {FormatBox26Dot6(exactBox)}");
        Console.WriteLine($"FT_Outline_Get_Orientation: {orientation}");

        if (points.Count > 0)
        {
            var map = CreateOutlineMap(points);
            DrawOutline(canvas, points, contours, map);
            DrawOutlineBox(canvas, controlBox, map, DemoColor.Green);
            DrawOutlineBox(canvas, exactBox, map, DemoColor.Cyan);
        }

        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Outline_Get_CBox", 36, 392, useKerning: true, DemoColor.Green);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Outline_Get_BBox", 380, 392, useKerning: true, DemoColor.Cyan);
        FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, orientation.ToString(), 36, 430, useKerning: true, DemoColor.White);

        var path = Path.Combine(outputRoot, "13-outline-geometry.png");
        canvas.Save(path);
        return Path.GetFullPath(path);
    }

    /// <summary>Exports glyphs produced from slot, copied, and converted bitmaps while reporting detached glyph metrics.</summary>
    public static string ExportGlyphBitmapObjects(Ft ft, nint library, FtFaceRec* face, string outputRoot)
    {
        FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 116));
        var glyphIndex = ft.GetCharIndex(face, 'B');
        CLong advance = default;
        FreeTypeStatus.Require(ft, "FT_Get_Advance", ft.GetAdvance(face, glyphIndex, FtLoadFlags.Default, out advance));
        Console.WriteLine($"FT_Get_Advance 'B': {FreeTypeValues.Fixed16Dot16(advance):0.##} px");
        var fastAdvanceError = ft.GetAdvance(face, glyphIndex, (int)FtAdvanceFlags.FastOnly, out var fastAdvance);
        FreeTypeStatus.ReportOptional(ft, "FT_Get_Advance FAST_ONLY", fastAdvanceError);
        if (fastAdvanceError == 0)
            Console.WriteLine($"FT_Get_Advance FAST_ONLY 'B': {FreeTypeValues.Fixed16Dot16(fastAdvance):0.##} px");

        ReportDetachedGlyphBox(ft, face, glyphIndex);

        FreeTypeStatus.Require(ft, "FT_Load_Glyph", ft.LoadGlyph(face, glyphIndex, FtLoadFlags.Render));
        var slot = FreeTypeDrawing.ReadGlyphSlot(face);
        var directGlyph = GlyphImage.FromSlot(slot);
        var copied = default(FtBitmap);
        var converted = default(FtBitmap);
        ft.BitmapNew(&copied);
        ft.BitmapNew(&converted);

        try
        {
            var source = slot.Bitmap;
            FreeTypeStatus.Require(ft, "FT_Bitmap_Copy", ft.BitmapCopy(library, &source, &copied));
            FreeTypeStatus.Require(ft, "FT_Bitmap_Convert", ft.BitmapConvert(library, &source, &converted, alignment: 1));
            var copiedGlyph = GlyphImage.FromBitmap(copied, slot.BitmapLeft, slot.BitmapTop, directGlyph.AdvanceX);
            var convertedGlyph = GlyphImage.FromBitmap(converted, slot.BitmapLeft, slot.BitmapTop, directGlyph.AdvanceX);

            var canvas = new PngCanvas(780, 340, DemoColor.Background);
            canvas.DrawGlyph(directGlyph, 100, 186, DemoColor.Gold);
            canvas.DrawGlyph(copiedGlyph, 330, 186, DemoColor.Cyan);
            canvas.DrawGlyph(convertedGlyph, 570, 186, DemoColor.Green);

            FreeTypeStatus.Require(ft, "FT_Set_Pixel_Sizes", ft.SetPixelSizes(face, 0, 20));
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "slot bitmap", 52, 286, useKerning: true, DemoColor.White);
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Bitmap_Copy", 272, 286, useKerning: true, DemoColor.White);
            FreeTypeDrawing.DrawTextCurrentSize(ft, face, canvas, "FT_Bitmap_Convert", 500, 286, useKerning: true, DemoColor.White);

            var path = Path.Combine(outputRoot, "14-glyph-bitmap-objects.png");
            canvas.Save(path);
            return Path.GetFullPath(path);
        }
        finally
        {
            _ = ft.BitmapDone(library, &converted);
            _ = ft.BitmapDone(library, &copied);
        }
    }

    /// <summary>Reports variation selector lookup APIs for a sample Unicode scalar.</summary>
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

    /// <summary>Builds a FreeType fixed-point rotation matrix.</summary>
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

    /// <summary>Reports the first subglyph record for a composite glyph slot.</summary>
    private static void ReportFirstSubglyph(Ft ft, FtGlyphSlotRec* glyphSlot)
    {
        FreeTypeStatus.Require(
            ft,
            "FT_Get_SubGlyph_Info",
            ft.GetSubGlyphInfo(glyphSlot, 0, out var index, out var flags, out var arg1, out var arg2, out _));
        Console.WriteLine($"FT_Get_SubGlyph_Info: index={index}, flags={FormatKnownSubglyphFlags(unchecked((int)flags))}, args=({arg1}, {arg2})");
    }

    /// <summary>Formats public style flag bits while preserving any raw bits FreeType reports.</summary>
    private static string FormatKnownStyleFlags(int rawFlags)
    {
        const int knownMask = (int)(FtStyleFlags.Italic | FtStyleFlags.Bold);
        return FormatKnownFlags((FtStyleFlags)(rawFlags & knownMask), rawFlags, knownMask);
    }

    /// <summary>Formats public subglyph flag bits while preserving any raw bits FreeType reports.</summary>
    private static string FormatKnownSubglyphFlags(int rawFlags)
    {
        const int knownMask = (int)(
            FtSubglyphFlags.ArgsAreWords |
            FtSubglyphFlags.ArgsAreXyValues |
            FtSubglyphFlags.RoundXyToGrid |
            FtSubglyphFlags.Scale |
            FtSubglyphFlags.XyScale |
            FtSubglyphFlags.Num2x2 |
            FtSubglyphFlags.UseMyMetrics);
        return FormatKnownFlags((FtSubglyphFlags)(rawFlags & knownMask), rawFlags, knownMask);
    }

    /// <summary>Formats an enum value plus unknown raw bits for partially public native flag fields.</summary>
    private static string FormatKnownFlags<TEnum>(TEnum knownFlags, int rawFlags, int knownMask)
        where TEnum : struct, Enum
    {
        var knownBits = rawFlags & knownMask;
        var unknownBits = rawFlags & ~knownMask;
        var knownText = knownBits == 0 ? "None" : knownFlags.ToString();
        return unknownBits == 0 ? $"{knownText} (0x{rawFlags:X})" : $"{knownText}, unknown 0x{unknownBits:X} (0x{rawFlags:X})";
    }

    /// <summary>Copies outline points into a managed list for drawing.</summary>
    private static List<FtVector> ReadOutlinePoints(FtOutline outline)
    {
        var points = new List<FtVector>(outline.NPoints);
        for (var i = 0; i < outline.NPoints; i++)
            points.Add(outline.Points[i]);

        return points;
    }

    /// <summary>Copies outline contour end indices into a managed list for drawing.</summary>
    private static List<short> ReadOutlineContours(FtOutline outline)
    {
        var contours = new List<short>(outline.NContours);
        for (var i = 0; i < outline.NContours; i++)
            contours.Add(Marshal.ReadInt16(outline.Contours, i * sizeof(short)));

        return contours;
    }

    /// <summary>Draws outline contours using a map derived from the outline points.</summary>
    private static void DrawOutline(PngCanvas canvas, List<FtVector> points, List<short> contours)
    {
        DrawOutline(canvas, points, contours, CreateOutlineMap(points));
    }

    /// <summary>Draws outline contours and point markers using a precomputed coordinate map.</summary>
    private static void DrawOutline(
        PngCanvas canvas,
        List<FtVector> points,
        List<short> contours,
        (long MinX, long MaxY, double Scale) map)
    {
        var previousEnd = -1;
        foreach (var end in contours)
        {
            var start = previousEnd + 1;
            for (var i = start; i <= end; i++)
            {
                var next = i == end ? start : i + 1;
                var p0 = MapOutlinePoint(points[i], map);
                var p1 = MapOutlinePoint(points[next], map);
                canvas.DrawLine(p0.X, p0.Y, p1.X, p1.Y, DemoColor.Guide);
            }

            previousEnd = end;
        }

        foreach (var point in points)
        {
            var mapped = MapOutlinePoint(point, map);
            canvas.DrawCircle(mapped.X, mapped.Y, 3, DemoColor.Gold);
        }
    }

    /// <summary>Creates a canvas-space mapping for an outline point cloud.</summary>
    private static (long MinX, long MaxY, double Scale) CreateOutlineMap(List<FtVector> points)
    {
        var minX = points.Min(point => FreeTypeValues.ToInt64(point.X));
        var maxX = points.Max(point => FreeTypeValues.ToInt64(point.X));
        var minY = points.Min(point => FreeTypeValues.ToInt64(point.Y));
        var maxY = points.Max(point => FreeTypeValues.ToInt64(point.Y));
        var scale = Math.Min(500.0 / Math.Max(1, maxX - minX), 310.0 / Math.Max(1, maxY - minY));
        return (minX, maxY, scale);
    }

    /// <summary>Draws a FreeType bounding box in canvas space.</summary>
    private static void DrawOutlineBox(PngCanvas canvas, FtBBox box, (long MinX, long MaxY, double Scale) map, DemoColor color)
    {
        var bottomLeft = MapOutlinePoint(FreeTypeValues.ToInt64(box.XMin), FreeTypeValues.ToInt64(box.YMin), map);
        var bottomRight = MapOutlinePoint(FreeTypeValues.ToInt64(box.XMax), FreeTypeValues.ToInt64(box.YMin), map);
        var topRight = MapOutlinePoint(FreeTypeValues.ToInt64(box.XMax), FreeTypeValues.ToInt64(box.YMax), map);
        var topLeft = MapOutlinePoint(FreeTypeValues.ToInt64(box.XMin), FreeTypeValues.ToInt64(box.YMax), map);
        canvas.DrawLine(bottomLeft.X, bottomLeft.Y, bottomRight.X, bottomRight.Y, color);
        canvas.DrawLine(bottomRight.X, bottomRight.Y, topRight.X, topRight.Y, color);
        canvas.DrawLine(topRight.X, topRight.Y, topLeft.X, topLeft.Y, color);
        canvas.DrawLine(topLeft.X, topLeft.Y, bottomLeft.X, bottomLeft.Y, color);
    }

    /// <summary>Maps a FreeType vector into canvas coordinates.</summary>
    private static (int X, int Y) MapOutlinePoint(FtVector point, (long MinX, long MaxY, double Scale) map) =>
        MapOutlinePoint(FreeTypeValues.ToInt64(point.X), FreeTypeValues.ToInt64(point.Y), map);

    /// <summary>Maps raw FreeType coordinates into canvas coordinates.</summary>
    private static (int X, int Y) MapOutlinePoint(long x, long y, (long MinX, long MaxY, double Scale) map) =>
        (
            60 + (int)Math.Round((x - map.MinX) * map.Scale),
            40 + (int)Math.Round((map.MaxY - y) * map.Scale));

    /// <summary>Draws a compact bar for fixed-point arithmetic comparisons.</summary>
    private static void DrawBar(PngCanvas canvas, int x, int baselineY, int value, DemoColor color)
    {
        var height = Math.Clamp(Math.Abs(value), 8, 190);
        canvas.DrawRectangle(x, baselineY + 190 - height, 48, height, color);
    }

    /// <summary>Formats a null-terminated FreeType UInt32 list for console output.</summary>
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
    /// <summary>Throws when a required FreeType call returns an error.</summary>
    public static void Require(Ft ft, string cName, int error)
    {
        if (error == 0)
            return;

        throw new InvalidOperationException($"{cName} failed with FT_Error {error}: {Describe(ft, error)}");
    }

    /// <summary>Writes a console message for optional FreeType calls.</summary>
    public static void ReportOptional(Ft ft, string cName, int error)
    {
        if (error == 0)
            Console.WriteLine($"{cName}: supported");
        else
            Console.WriteLine($"{cName}: unavailable for this font ({error}, {Describe(ft, error)})");
    }

    /// <summary>Formats a FreeType error code for console diagnostics.</summary>
    public static string Describe(Ft ft, int error)
    {
        ft.ErrorString(error, out var value);
        return value ?? "native build did not expose an error string";
    }
}
