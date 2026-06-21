namespace AlvorKit.Script.Bindgen;

/// <summary>Emits standalone generated type files for enums, structs, handles, and delegates.</summary>
internal sealed class BindingTypeEmitter(BindingEmitterContext context)
{
    /// <summary>Emits a managed enum type.</summary>
    public string Enum(BindingEnum enumType)
    {
        var members = string.Join("", enumType.Members.Select(EnumMember));
        return TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/enum.cs.tmpl",
            ("TypeHeader", TypeHeader()),
            ("Documentation", BindingTypeDocs.Enum(enumType)),
            ("Flags", enumType.IsFlags ? "[Flags]" + Environment.NewLine : ""),
            ("ManagedName", enumType.ManagedName),
            ("UnderlyingType", enumType.UnderlyingType == "int" ? "" : " : " + enumType.UnderlyingType),
            ("Members", members));
    }

    /// <summary>Emits a managed struct or union type.</summary>
    public string Struct(BindingStruct structType)
    {
        var fields = string.Join("", structType.Fields.Select(field => StructField(structType, field)));
        var nestedBuffers = string.Join("", structType.NestedBuffers.Select(InlineBuffer));
        return TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/struct.cs.tmpl",
            ("TypeHeader", TypeHeader()),
            ("Documentation", BindingDocs.NativeSummary(
                structType.NativeName,
                structType.Documentation,
                $"Native record <c>{structType.NativeName}</c>.")),
            ("StructLayout", structType.IsUnion ? "[StructLayout(LayoutKind.Explicit)]" : "[StructLayout(LayoutKind.Sequential)]"),
            ("Unsafe", structType.Fields.Any(field => field.ManagedType.Contains('*')) ? "unsafe " : ""),
            ("ManagedName", structType.ManagedName),
            ("Fields", fields),
            ("NestedBuffers", nestedBuffers),
            ("ConversionMembers", UInt128ConversionMembers(structType)));
    }

    /// <summary>Emits an opaque native handle type.</summary>
    public string Handle(BindingHandle handle)
        => TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/handle.cs.tmpl",
            ("TypeHeader", TypeHeader()),
            ("NativeName", handle.NativeName),
            ("ManagedName", handle.ManagedName));

    /// <summary>Emits the xxHash streaming-secret helper type.</summary>
    public string XxHashSecret() =>
        TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/xxhash-secret.cs.tmpl",
            ("TypeHeader", TypeHeader()));

    /// <summary>Emits a native callback delegate type.</summary>
    public string Delegate(BindingDelegate callback)
    {
        var signature = string.Join(", ", callback.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        var parameters = string.Join("", callback.Parameters.Select(parameter => TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/delegate-param.csfrag.tmpl",
            ("ManagedName", parameter.ManagedName.TrimStart('@')),
            ("Documentation", BindingTypeDocs.DelegateParameter(callback, parameter)))));
        return TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/delegate.cs.tmpl",
            ("TypeHeader", TypeHeader()),
            ("Documentation", BindingTypeDocs.Delegate(callback)),
            ("Parameters", parameters),
            ("ReturnType", callback.ReturnType),
            ("Unsafe", callback.ReturnType.Contains('*') || callback.Parameters.Any(parameter => parameter.ManagedType.Contains('*')) ? "unsafe " : ""),
            ("ManagedName", callback.ManagedName),
            ("Signature", signature));
    }

    /// <summary>Renders the namespace header used by generated type files.</summary>
    private string TypeHeader() =>
        TemplateResource.RenderFragment(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/type-header.csfrag.tmpl",
            ("SourceHeader", context.SourceHeader().ToString()),
            ("Namespace", context.Config.Namespace));

    /// <summary>Renders one generated enum member.</summary>
    private static string EnumMember(BindingEnumMember member) =>
        TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/enum-member.csfrag.tmpl",
            ("Documentation", BindingDocs.NativeSummary(member.NativeName, member.Documentation, $"<c>{member.NativeName}</c>.")),
            ("ManagedName", member.ManagedName),
            ("Value", member.Value.ToString()));

    /// <summary>Renders one generated struct field.</summary>
    private static string StructField(BindingStruct structType, BindingField field) =>
        TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/struct-field.csfrag.tmpl",
            ("Documentation", BindingTypeDocs.Field(field)),
            ("FieldOffset", structType.IsUnion ? "[FieldOffset(0)] " : ""),
            ("ManagedType", field.ManagedType),
            ("ManagedName", field.ManagedName));

    /// <summary>Renders a nested inline-array helper type.</summary>
    private static string InlineBuffer(InlineBufferDefinition buffer) =>
        TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/inline-buffer.csfrag.tmpl",
            ("Count", buffer.Count.ToString()),
            ("NativeName", buffer.NativeName),
            ("ElementType", buffer.ElementType),
            ("ManagedName", buffer.ManagedName));

    /// <summary>Renders helpers for native 128-bit structs publicly projected as <see cref="UInt128"/>.</summary>
    private string UInt128ConversionMembers(BindingStruct structType)
    {
        var publicAlias = context.Config.TypeAliases.GetValueOrDefault(structType.NativeName);
        var interopAlias = context.Config.InteropTypeAliases.GetValueOrDefault(structType.NativeName);
        if (publicAlias != "UInt128" || interopAlias != structType.ManagedName)
            return "";

        return TemplateResource.Render(
            typeof(BindingTypeEmitter),
            "res/templates/bindgen/c-headers/csharp/uint128-conversions.csfrag.tmpl",
            ("ManagedName", structType.ManagedName));
    }
}
