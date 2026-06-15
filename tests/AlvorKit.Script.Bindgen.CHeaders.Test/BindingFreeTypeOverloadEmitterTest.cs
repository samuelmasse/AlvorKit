namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class BindingFreeTypeOverloadEmitterTest
{
    /// <summary>FreeType opt-in overloads cover character codes and glyph names without adding binding-level utility helpers.</summary>
    [TestMethod]
    public void Emit_FreeTypeConvenienceOverloads()
    {
        using var workspace = TempWorkspace.Create();
        var config = FreeTypeConfig();

        new BindingCodeEmitter(config, "1.0.0").Emit(FreeTypeModel(), workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "FtOverloads.cs"));
        StringAssert.Contains(overloads, "using System.Text;");
        StringAssert.Contains(overloads, "public unsafe partial class Ft");
        StringAssert.Contains(overloads, "public int LoadChar(FtFaceRec* face, char character, int load_flags)");
        StringAssert.Contains(overloads, "public int LoadChar(FtFaceRec* face, Rune character, int load_flags)");
        StringAssert.Contains(overloads, "public uint GetCharIndex(FtFaceRec* face, char character)");
        StringAssert.Contains(overloads, "public unsafe int GetGlyphName(FtFaceRec* face, uint glyph_index, out string? value)");
        StringAssert.Contains(overloads, "public int RequestSize(FtFaceRec* face, in FtSizeRequestRec req)");
        StringAssert.Contains(overloads, "public void SetTransform(FtFaceRec* face, in FtMatrix matrix, in FtVector delta)");
        StringAssert.Contains(overloads, "public int FaceProperties(FtFaceRec* face, ReadOnlySpan<FtParameter> properties)");
        StringAssert.Contains(overloads, "Span<byte> buffer,");
        StringAssert.Contains(overloads, "out ReadOnlySpan<byte> value)");
        StringAssert.Contains(overloads, "MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pointer)");
        Assert.IsFalse(overloads.Contains("DescribeError", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("ThrowIfError", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("GetFaceRec", StringComparison.Ordinal));
        Assert.IsFalse(overloads.Contains("GetGlyphSlot", StringComparison.Ordinal));
    }

    /// <summary>FreeType convenience skips the GetCharIndex helpers when only FT_Load_Char is generated.</summary>
    [TestMethod]
    public void Emit_FreeTypeConvenienceAllowsPartialCharacterApis()
    {
        using var workspace = TempWorkspace.Create();
        var config = FreeTypeConfig();
        var model = EmptyModel() with { Functions = [LoadChar()] };

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "FtOverloads.cs"));
        StringAssert.Contains(overloads, "public int LoadChar(FtFaceRec* face, char character, int load_flags)");
        Assert.IsFalse(overloads.Contains("public uint GetCharIndex(FtFaceRec* face, char character)", StringComparison.Ordinal));
    }

    /// <summary>FreeType convenience also supports the character-index helper without FT_Load_Char.</summary>
    [TestMethod]
    public void Emit_FreeTypeConvenienceAllowsCharacterIndexOnly()
    {
        using var workspace = TempWorkspace.Create();
        var config = FreeTypeConfig();
        var model = EmptyModel() with { Functions = [GetCharIndex()] };

        new BindingCodeEmitter(config, "1.0.0").Emit(model, workspace.Root, "1.0.0", "1.0.0");

        var overloads = File.ReadAllText(Path.Combine(workspace.Root, config.ApiProject, "FtOverloads.cs"));
        StringAssert.Contains(overloads, "public uint GetCharIndex(FtFaceRec* face, char character)");
        Assert.IsFalse(overloads.Contains("public int LoadChar(FtFaceRec* face, char character, int load_flags)", StringComparison.Ordinal));
    }

    /// <summary>FreeType convenience emits no overload file when every underlying native member is absent.</summary>
    [TestMethod]
    public void Emit_FreeTypeConvenienceSkipsMissingGroups()
    {
        using var workspace = TempWorkspace.Create();
        var config = FreeTypeConfig();

        new BindingCodeEmitter(config, "1.0.0").Emit(EmptyModel(), workspace.Root, "1.0.0", "1.0.0");

        Assert.IsFalse(File.Exists(Path.Combine(workspace.Root, config.ApiProject, "FtOverloads.cs")));
    }

    /// <summary>Returns a fixture config with FreeType convenience enabled.</summary>
    private static BindgenConfig FreeTypeConfig()
    {
        var config = CHeaderTestConfig.Create();
        config.ApiClass = "Ft";
        config.FreeTypeConvenience = true;
        return config;
    }

    /// <summary>Creates a model containing the FreeType members needed by the first convenience batch.</summary>
    private static BindingModel FreeTypeModel() => EmptyModel() with
    {
        Functions = [LoadChar(), GetCharIndex(), GetGlyphName(), RequestSize(), SetTransform(), FaceProperties()],
    };

    /// <summary>Returns an empty binding model.</summary>
    private static BindingModel EmptyModel() => new([], [], [], [], [], [], []);

    /// <summary>Returns an FT_Load_Char fixture function.</summary>
    private static BindingFunction LoadChar() =>
        new("FT_Load_Char", "LoadChar", "int", "int",
            [new("face", "FtFaceRec*", "FtFaceRec*", "", false), new("char_code", "CULong", "CULong", "", false), new("load_flags", "int", "int", "", false)],
            null);

    /// <summary>Returns an FT_Get_Char_Index fixture function.</summary>
    private static BindingFunction GetCharIndex() =>
        new("FT_Get_Char_Index", "GetCharIndex", "uint", "uint",
            [new("face", "FtFaceRec*", "FtFaceRec*", "", false), new("charcode", "CULong", "CULong", "", false)],
            null);

    /// <summary>Returns an FT_Get_Glyph_Name fixture function.</summary>
    private static BindingFunction GetGlyphName() =>
        new("FT_Get_Glyph_Name", "GetGlyphName", "int", "int",
            [
                new("face", "FtFaceRec*", "FtFaceRec*", "", false),
                new("glyph_index", "uint", "uint", "", false),
                new("buffer", "nint", "nint", "", false),
                new("buffer_max", "uint", "uint", "", false),
            ],
            null);

    /// <summary>Returns an FT_Request_Size fixture function.</summary>
    private static BindingFunction RequestSize() =>
        new("FT_Request_Size", "RequestSize", "int", "int",
            [new("face", "FtFaceRec*", "FtFaceRec*", "", false), new("req", "FtSizeRequestRec*", "FtSizeRequestRec*", "", false)],
            null);

    /// <summary>Returns an FT_Set_Transform fixture function.</summary>
    private static BindingFunction SetTransform() =>
        new("FT_Set_Transform", "SetTransform", "void", "void",
            [
                new("face", "FtFaceRec*", "FtFaceRec*", "", false),
                new("matrix", "FtMatrix*", "FtMatrix*", "", false),
                new("delta", "FtVector*", "FtVector*", "", false),
            ],
            null);

    /// <summary>Returns an FT_Face_Properties fixture function.</summary>
    private static BindingFunction FaceProperties() =>
        new("FT_Face_Properties", "FaceProperties", "int", "int",
            [
                new("face", "FtFaceRec*", "FtFaceRec*", "", false),
                new("num_properties", "uint", "uint", "", false),
                new("properties", "FtParameter*", "FtParameter*", "", false),
            ],
            null);
}
