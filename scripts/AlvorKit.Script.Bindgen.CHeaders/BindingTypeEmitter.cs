namespace AlvorKit.Script.Bindgen;

/// <summary>Emits standalone generated type files for enums, structs, handles, and delegates.</summary>
internal sealed class BindingTypeEmitter(BindingEmitterContext context)
{
    /// <summary>Emits a managed enum type.</summary>
    public string Enum(BindingEnum enumType)
    {
        var output = TypeHeader();
        output.AppendLine($"/// <summary>{enumType.Documentation ?? $"Maps <c>{enumType.NativeName}</c>."}</summary>");
        if (enumType.IsFlags)
            output.AppendLine("[Flags]");
        output.AppendLine($"public enum {enumType.ManagedName}{(enumType.UnderlyingType == "int" ? "" : " : " + enumType.UnderlyingType)}");
        output.AppendLine("{");
        foreach (var member in enumType.Members)
        {
            output.AppendLine($"    /// <summary>{member.Documentation ?? $"Maps <c>{member.ManagedName}</c>."}</summary>");
            output.AppendLine($"    {member.ManagedName} = {member.Value},");
        }
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits a managed struct or union type.</summary>
    public string Struct(BindingStruct structType)
    {
        var output = TypeHeader();
        output.AppendLine($"/// <summary>{structType.Documentation ?? $"Maps <c>{structType.NativeName}</c>."}</summary>");
        output.AppendLine(structType.IsUnion ? "[StructLayout(LayoutKind.Explicit)]" : "[StructLayout(LayoutKind.Sequential)]");
        output.AppendLine($"public struct {structType.ManagedName}");
        output.AppendLine("{");
        foreach (var field in structType.Fields)
        {
            output.AppendLine($"    /// <summary>{field.Documentation ?? $"Maps the native field at byte offset {field.Offset}."}</summary>");
            output.AppendLine($"    {(structType.IsUnion ? "[FieldOffset(0)] " : "")}public {field.ManagedType} {field.ManagedName};");
        }
        foreach (var buffer in structType.NestedBuffers)
            InlineBuffer(output, buffer);
        output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Emits an opaque native handle type.</summary>
    public string Handle(BindingHandle handle)
    {
        var output = TypeHeader();
        output.AppendLine($"/// <summary>An opaque <c>{handle.NativeName}</c> handle; <c>default</c> is the null handle.</summary>");
        output.AppendLine("/// <param name=\"Handle\">Native pointer value.</param>");
        output.AppendLine($"public readonly record struct {handle.ManagedName}(nint Handle);");
        return output.ToString();
    }

    /// <summary>Emits a native callback delegate type.</summary>
    public string Delegate(BindingDelegate callback)
    {
        var output = TypeHeader();
        output.AppendLine("/// <summary>A native callback delegate; install it through the matching <c>Set*Callback</c> method.</summary>");
        foreach (var parameter in callback.Parameters)
            output.AppendLine($"/// <param name=\"{parameter.ManagedName.TrimStart('@')}\">Native callback parameter.</param>");
        output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Cdecl)]");
        var signature = string.Join(", ", callback.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        output.AppendLine($"public delegate {callback.ReturnType} {callback.ManagedName}({signature});");
        return output.ToString();
    }

    /// <summary>Emits the namespace header used by generated type files.</summary>
    private StringBuilder TypeHeader() => context.SourceHeader()
        .AppendLine($"namespace {context.Config.Namespace};")
        .AppendLine();

    /// <summary>Emits a nested inline-array helper type.</summary>
    private static void InlineBuffer(StringBuilder output, InlineBufferDefinition buffer)
    {
        output.AppendLine();
        output.AppendLine($"    /// <summary>A fixed buffer of {buffer.Count} <see cref=\"{buffer.ElementType}\"/> values.</summary>");
        output.AppendLine($"    [InlineArray({buffer.Count})]");
        output.AppendLine($"    public struct {buffer.ManagedName}");
        output.AppendLine("    {");
        output.AppendLine("        /// <summary>First element storage used by the compiler-expanded inline array.</summary>");
        output.AppendLine($"        private {buffer.ElementType} element;");
        output.AppendLine("    }");
    }
}
