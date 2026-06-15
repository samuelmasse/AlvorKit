namespace AlvorKit.Script.Bindgen;

/// <summary>Emits FreeType-specific convenience members over the generated freetype.h API.</summary>
internal static partial class BindingFreeTypeOverloadEmitter
{
    /// <summary>Emits the first batch of FreeType convenience methods.</summary>
    /// <param name="output">The overload file body being built.</param>
    /// <param name="model">The generated binding model.</param>
    /// <param name="apiClass">The generated FreeType API class name.</param>
    public static void FreeTypeOverloads(StringBuilder output, BindingModel model, string apiClass)
    {
        if (HasFunction(model, "FT_Load_Char") || HasFunction(model, "FT_Get_Char_Index"))
            CharacterCodeOverloads(output, model, apiClass);
        if (HasFunction(model, "FT_Get_Glyph_Name"))
            GlyphNameOverload(output, model, apiClass);
        StructuredOverloads(output, model, apiClass);
    }

    /// <summary>Emits char and Rune overloads for FT character-code APIs.</summary>
    private static void CharacterCodeOverloads(StringBuilder output, BindingModel model, string apiClass)
    {
        if (Function(model, "FT_Load_Char") is { } loadChar)
        {
            BindingDocs.InheritedConvenience(
                output,
                $"{apiClass}.LoadChar({BindingSignature.Cref(loadChar.Parameters)})",
                "Converts a UTF-16 code unit to an FT_ULong character code.");
            output.AppendLine($"    public int LoadChar({loadChar.Parameters[0].ManagedType} face, char character, int load_flags) =>");
            output.AppendLine("        LoadChar(face, new CULong((uint)character), load_flags);");
            output.AppendLine();

            BindingDocs.InheritedConvenience(
                output,
                $"{apiClass}.LoadChar({BindingSignature.Cref(loadChar.Parameters)})",
                "Converts a Unicode scalar value to an FT_ULong character code.");
            output.AppendLine($"    public int LoadChar({loadChar.Parameters[0].ManagedType} face, Rune character, int load_flags) =>");
            output.AppendLine("        LoadChar(face, new CULong((uint)character.Value), load_flags);");
            output.AppendLine();
        }

        if (Function(model, "FT_Get_Char_Index") is not { } getCharIndex)
            return;

        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetCharIndex({BindingSignature.Cref(getCharIndex.Parameters)})",
            "Converts a UTF-16 code unit to an FT_ULong character code.");
        output.AppendLine($"    public uint GetCharIndex({getCharIndex.Parameters[0].ManagedType} face, char character) =>");
        output.AppendLine("        GetCharIndex(face, new CULong((uint)character));");
        output.AppendLine();

        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetCharIndex({BindingSignature.Cref(getCharIndex.Parameters)})",
            "Converts a Unicode scalar value to an FT_ULong character code.");
        output.AppendLine($"    public uint GetCharIndex({getCharIndex.Parameters[0].ManagedType} face, Rune character) =>");
        output.AppendLine("        GetCharIndex(face, new CULong((uint)character.Value));");
        output.AppendLine();
    }

    /// <summary>Emits an FT_Get_Glyph_Name overload that returns managed text while preserving FT_Error.</summary>
    private static void GlyphNameOverload(StringBuilder output, BindingModel model, string apiClass)
    {
        var function = Function(model, "FT_Get_Glyph_Name")!;
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetGlyphName({BindingSignature.Cref(function.Parameters)})",
            "Uses a stack buffer, decodes the returned UTF-8 glyph name, and preserves the FT_Error return value.");
        output.AppendLine($"    public unsafe int GetGlyphName({function.Parameters[0].ManagedType} face, uint glyph_index, out string? value)");
        output.AppendLine("    {");
        output.AppendLine("        Span<byte> buffer = stackalloc byte[256];");
        output.AppendLine("        fixed (byte* pointer = buffer)");
        output.AppendLine("        {");
        output.AppendLine("            var error = GetGlyphName(face, glyph_index, (nint)pointer, (uint)buffer.Length);");
        output.AppendLine("            value = error == 0 ? Marshal.PtrToStringUTF8((nint)pointer) : null;");
        output.AppendLine("            return error;");
        output.AppendLine("        }");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>Returns true when a native function was generated.</summary>
    private static bool HasFunction(BindingModel model, string nativeName) =>
        model.Functions.Any(function => function.NativeName == nativeName);

    /// <summary>Returns the generated function for a native name when present.</summary>
    private static BindingFunction? Function(BindingModel model, string nativeName) =>
        model.Functions.FirstOrDefault(function => function.NativeName == nativeName);
}
