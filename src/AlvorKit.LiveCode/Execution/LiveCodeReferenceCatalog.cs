namespace AlvorKit;

/// <summary>Builds a compilation manifest from the target application, framework, and loaded extension assemblies.</summary>
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

        if (options.CompilationAssembly is { } compilationAssembly)
            LiveCodeDependencyCatalog.AddTo(paths, compilationAssembly);

        var globalUsings = new HashSet<string>(StringComparer.Ordinal);
        if (options.CompilationAssembly is { } importAssembly)
        {
            foreach (var attribute in importAssembly.GetCustomAttributes<LiveCodeGlobalUsingAttribute>())
                globalUsings.Add(attribute.Clause);
        }
        foreach (var clause in options.GlobalUsings)
            globalUsings.Add(clause);

        return new(
            [.. paths.Order(StringComparer.Ordinal)],
            [.. globalUsings.Order(StringComparer.Ordinal)]);
    }
}
