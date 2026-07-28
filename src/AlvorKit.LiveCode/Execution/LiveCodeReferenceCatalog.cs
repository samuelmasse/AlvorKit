namespace AlvorKit.LiveCode;

/// <summary>Builds a compilation manifest from framework and currently loaded target assemblies.</summary>
internal sealed class LiveCodeReferenceCatalog(LiveCodeHostOptions options)
{
    internal LiveCodeReferenceManifest Create()
    {
        var paths = new HashSet<string>(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var trusted = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (trusted is not null)
        {
            foreach (var path in trusted.Split(Path.PathSeparator))
                paths.Add(path);
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                paths.Add(assembly.Location);
        }

        return new(
            [.. paths.Order(StringComparer.Ordinal)],
            [.. options.GlobalUsings]);
    }
}
