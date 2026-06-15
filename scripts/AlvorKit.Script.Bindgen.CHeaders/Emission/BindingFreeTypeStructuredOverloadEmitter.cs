namespace AlvorKit.Script.Bindgen;

/// <summary>Emits FreeType overloads for small struct and caller-owned buffer shapes.</summary>
internal static partial class BindingFreeTypeOverloadEmitter
{
    /// <summary>Adds FreeType overloads that only adapt parameters for one native call.</summary>
    private static void StructuredOverloads(StringBuilder output, BindingModel model, string apiClass)
    {
        if (Function(model, "FT_Request_Size") is { } requestSize)
            RequestSizeOverload(output, requestSize, apiClass);
        if (Function(model, "FT_Set_Transform") is { } setTransform)
            SetTransformOverload(output, setTransform, apiClass);
        if (Function(model, "FT_Face_Properties") is { } faceProperties)
            FacePropertiesOverload(output, faceProperties, apiClass);
        if (Function(model, "FT_Get_Glyph_Name") is { } glyphName)
            GlyphNameBufferOverload(output, glyphName, apiClass);
    }

    /// <summary>Emits a caller-owned request struct overload for FT_Request_Size.</summary>
    private static void RequestSizeOverload(StringBuilder output, BindingFunction function, string apiClass)
    {
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.RequestSize({BindingSignature.Cref(function.Parameters)})",
            "Passes a caller-owned FT_Size_RequestRec by readonly reference.");
        output.AppendLine($"    public int RequestSize({function.Parameters[0].ManagedType} face, in FtSizeRequestRec req) =>");
        output.AppendLine("        RequestSize(face, (FtSizeRequestRec*)Unsafe.AsPointer(ref Unsafe.AsRef(in req)));");
        output.AppendLine();
    }

    /// <summary>Emits a caller-owned transform overload for FT_Set_Transform.</summary>
    private static void SetTransformOverload(StringBuilder output, BindingFunction function, string apiClass)
    {
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.SetTransform({BindingSignature.Cref(function.Parameters)})",
            "Passes caller-owned matrix and delta values by readonly reference.");
        output.AppendLine($"    public void SetTransform({function.Parameters[0].ManagedType} face, in FtMatrix matrix, in FtVector delta)");
        output.AppendLine("    {");
        output.AppendLine("        var matrixPointer = (FtMatrix*)Unsafe.AsPointer(ref Unsafe.AsRef(in matrix));");
        output.AppendLine("        var deltaPointer = (FtVector*)Unsafe.AsPointer(ref Unsafe.AsRef(in delta));");
        output.AppendLine("        SetTransform(face, matrixPointer, deltaPointer);");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>Emits a span overload for FT_Face_Properties.</summary>
    private static void FacePropertiesOverload(StringBuilder output, BindingFunction function, string apiClass)
    {
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.FaceProperties({BindingSignature.Cref(function.Parameters)})",
            "Pins a caller-owned FT_Parameter span and supplies the property count.");
        output.AppendLine($"    public int FaceProperties({function.Parameters[0].ManagedType} face, ReadOnlySpan<FtParameter> properties)");
        output.AppendLine("    {");
        output.AppendLine("        fixed (FtParameter* propertiesPointer = properties)");
        output.AppendLine("            return FaceProperties(face, (uint)properties.Length, propertiesPointer);");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>Emits a caller-owned byte buffer overload for FT_Get_Glyph_Name.</summary>
    private static void GlyphNameBufferOverload(StringBuilder output, BindingFunction function, string apiClass)
    {
        BindingDocs.InheritedConvenience(
            output,
            $"{apiClass}.GetGlyphName({BindingSignature.Cref(function.Parameters)})",
            "Writes the glyph name into a caller-owned byte buffer and returns the NUL-terminated slice.");
        output.AppendLine("    public unsafe int GetGlyphName(");
        output.AppendLine($"        {function.Parameters[0].ManagedType} face,");
        output.AppendLine("        uint glyph_index,");
        output.AppendLine("        Span<byte> buffer,");
        output.AppendLine("        out ReadOnlySpan<byte> value)");
        output.AppendLine("    {");
        output.AppendLine("        fixed (byte* pointer = buffer)");
        output.AppendLine("        {");
        output.AppendLine("            var error = GetGlyphName(face, glyph_index, (nint)pointer, (uint)buffer.Length);");
        output.AppendLine("            value = error == 0 && pointer != null ? MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pointer) : default;");
        output.AppendLine("            return error;");
        output.AppendLine("        }");
        output.AppendLine("    }");
        output.AppendLine();
    }
}
