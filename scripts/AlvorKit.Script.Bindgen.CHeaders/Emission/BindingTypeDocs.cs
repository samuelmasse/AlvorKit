namespace AlvorKit.Script.Bindgen;

/// <summary>Formats XML documentation for standalone generated binding types.</summary>
internal static class BindingTypeDocs
{
    /// <summary>Returns enum documentation while preserving configured synthetic group prose.</summary>
    public static string Enum(BindingEnum enumType) =>
        enumType.NativeName == enumType.ManagedName
            ? enumType.Documentation ?? $"Native constants exposed as <c>{enumType.ManagedName}</c>."
            : BindingDocs.NativeSummary(
                enumType.NativeName,
                enumType.Documentation,
                $"Native enum <c>{enumType.NativeName}</c>.");

    /// <summary>Returns symbol-anchored documentation for one generated field.</summary>
    public static string Field(BindingField field)
    {
        var nativeName = field.NativeName ?? field.ManagedName;
        return BindingDocs.NativeSummary(
            nativeName,
            field.Documentation,
            $"Native <c>{nativeName}</c> field at byte offset {field.Offset}.");
    }

    /// <summary>Returns symbol-anchored documentation for a generated delegate type.</summary>
    public static string Delegate(BindingDelegate callback)
    {
        var nativeName = callback.NativeName ?? callback.ManagedName;
        return BindingDocs.NativeSummary(
            nativeName,
            callback.Documentation?.Summary,
            $"Native callback typedef <c>{nativeName}</c>.");
    }

    /// <summary>Returns symbol-anchored documentation for one generated delegate parameter.</summary>
    public static string DelegateParameter(BindingDelegate callback, BindingParameter parameter)
    {
        var nativeName = parameter.ManagedName.TrimStart('@');
        return BindingDocs.NativeSummary(
            nativeName,
            callback.Documentation?.Parameters.GetValueOrDefault(nativeName),
            $"Native <c>{nativeName}</c> callback parameter.");
    }
}
