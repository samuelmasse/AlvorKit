namespace AlvorKit;

/// <summary>Emits native compiler layout expressions and managed reflection manifests for every configured ABI.</summary>
public class BindingAbiEmitter(BindgenConfig config)
{
    /// <summary>Writes per-runtime manifests and updates or strictly checks the native verification table.</summary>
    public void Emit(Dictionary<string, BindingModel> models, string libraryDirectory, string outputRoot, bool strict)
    {
        if (config.AbiShim == null)
            throw new InvalidOperationException("Runtime-specific bindings require an abiShim for native layout verification.");

        var sections = new StringBuilder();

        foreach (var (runtime, model) in models)
        {
            var entries = Entries(model);
            var manifest = Path.Combine(outputRoot, $"{config.NativeLibrary}-abi-{runtime}.json");
            File.WriteAllText(manifest, JsonSerializer.Serialize(entries));
            var values = string.Concat(entries.Select(entry => TemplateResource.Render(
                typeof(BindingAbiEmitter), "res/templates/bindgen/c-headers/abi-value.c.tmpl",
                ("Expression", entry.NativeExpression))));
            sections.Append(TemplateResource.Render(
                typeof(BindingAbiEmitter), "res/templates/bindgen/c-headers/abi-runtime.c.tmpl",
                ("Condition", BindingRuntimeTarget.Condition(runtime)), ("Values", values)));
        }

        var contents = TemplateResource.Render(
            typeof(BindingAbiEmitter), "res/templates/bindgen/c-headers/abi-table.c.tmpl",
            ("Sections", sections.ToString()));
        var path = Path.Combine(libraryDirectory, config.AbiShim);

        if (File.Exists(path) && File.ReadAllText(path).Replace("\r\n", "\n") == contents.Replace("\r\n", "\n"))
            return;

        if (strict)
            throw new InvalidOperationException($"{config.AbiShim} is stale. Regenerate and release the native package first.");

        File.WriteAllText(path, contents);
    }

    /// <summary>Returns native sizeof and offsetof checks, including every addressable nested record field.</summary>
    public static List<BindingAbiEntry> Entries(BindingModel model)
    {
        var entries = new List<BindingAbiEntry>();
        var types = model.Structs.ToDictionary(type => type.ManagedName);

        foreach (var type in model.Structs.Where(type => type.IsNativeType).OrderBy(type => type.NativeName, StringComparer.Ordinal))
        {
            entries.Add(new(type.ManagedName, "", $"sizeof({type.NativeName})"));
            AddFields(entries, types, type, type, "", "");
        }

        return entries;
    }

    /// <summary>Walks embedded records without following pointers or manufacturing names for anonymous native fields.</summary>
    private static void AddFields(
        List<BindingAbiEntry> entries, Dictionary<string, BindingStruct> types,
        BindingStruct root, BindingStruct current, string managedPath, string nativePath)
    {
        foreach (var field in current.Fields)
        {
            var managed = managedPath + field.ManagedName;
            var anonymous = field.NativeName?.StartsWith("anonymous", StringComparison.Ordinal) == true;
            var native = anonymous ? nativePath.TrimEnd('.') : nativePath + field.NativeName;

            if (!anonymous)
                entries.Add(new(root.ManagedName, managed, $"offsetof({root.NativeName}, {native})"));

            if (types.TryGetValue(field.ManagedType, out var nested))
                AddFields(entries, types, root, nested, managed + ".", native.Length == 0 ? "" : native + ".");
        }
    }
}
