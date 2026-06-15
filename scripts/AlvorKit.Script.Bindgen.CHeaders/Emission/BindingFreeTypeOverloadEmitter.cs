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
            AppendFreeTypeFragment(
                output,
                "freetype-load-char-code-unit-overload.csfrag.tmpl",
                ("FaceType", loadChar.Parameters[0].ManagedType));

            BindingDocs.InheritedConvenience(
                output,
                $"{apiClass}.LoadChar({BindingSignature.Cref(loadChar.Parameters)})",
                "Converts a Unicode scalar value to an FT_ULong character code.");
            AppendFreeTypeFragment(
                output,
                "freetype-load-char-rune-overload.csfrag.tmpl",
                ("FaceType", loadChar.Parameters[0].ManagedType));
        }

        if (Function(model, "FT_Get_Char_Index") is not { } getCharIndex)
            return;

        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetCharIndex({BindingSignature.Cref(getCharIndex.Parameters)})",
            "Converts a UTF-16 code unit to an FT_ULong character code.");
        AppendFreeTypeFragment(
            output,
            "freetype-get-char-index-code-unit-overload.csfrag.tmpl",
            ("FaceType", getCharIndex.Parameters[0].ManagedType));

        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetCharIndex({BindingSignature.Cref(getCharIndex.Parameters)})",
            "Converts a Unicode scalar value to an FT_ULong character code.");
        AppendFreeTypeFragment(
            output,
            "freetype-get-char-index-rune-overload.csfrag.tmpl",
            ("FaceType", getCharIndex.Parameters[0].ManagedType));
    }

    /// <summary>Emits an FT_Get_Glyph_Name overload that returns managed text while preserving FT_Error.</summary>
    private static void GlyphNameOverload(StringBuilder output, BindingModel model, string apiClass)
    {
        var function = Function(model, "FT_Get_Glyph_Name")!;
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetGlyphName({BindingSignature.Cref(function.Parameters)})",
            "Uses a stack buffer, decodes the returned UTF-8 glyph name, and preserves the FT_Error return value.");
        AppendFreeTypeFragment(
            output,
            "freetype-glyph-name-string-overload.csfrag.tmpl",
            ("FaceType", function.Parameters[0].ManagedType));
    }

    /// <summary>Renders a FreeType overload template fragment into the generated overload body.</summary>
    private static void AppendFreeTypeFragment(StringBuilder output, string templateName, params (string Name, string Value)[] values) =>
        output.Append(TemplateResource.RenderFragment(
            typeof(BindingFreeTypeOverloadEmitter),
            $"res/templates/bindgen/c-headers/csharp/{templateName}",
            values));

    /// <summary>Returns true when a native function was generated.</summary>
    private static bool HasFunction(BindingModel model, string nativeName) =>
        model.Functions.Any(function => function.NativeName == nativeName);

    /// <summary>Returns the generated function for a native name when present.</summary>
    private static BindingFunction? Function(BindingModel model, string nativeName) =>
        model.Functions.FirstOrDefault(function => function.NativeName == nativeName);
}
