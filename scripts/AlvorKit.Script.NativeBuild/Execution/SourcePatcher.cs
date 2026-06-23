namespace AlvorKit.Script.NativeBuild;

/// <summary>Applies manifest-owned text patches to extracted native source archives.</summary>
internal static class SourcePatcher
{
    /// <summary>Applies every source patch configured for the library.</summary>
    public static void Apply(LibraryBuildContext library)
    {
        foreach (var patch in library.Build.SourcePatches)
            Apply(library, patch);
    }

    /// <summary>Applies one idempotent source text replacement.</summary>
    private static void Apply(LibraryBuildContext library, SourcePatchConfig patch)
    {
        var sourceRoot = Path.GetFullPath(library.SourceDirectory);
        var path = Path.GetFullPath(Path.Combine(sourceRoot, patch.Path));
        if (!path.StartsWith(sourceRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException($"{library.Name}: source patch path escapes source directory: {patch.Path}");

        var text = File.ReadAllText(path);
        if (text.Contains(patch.Search, StringComparison.Ordinal))
        {
            File.WriteAllText(path, text.Replace(patch.Search, patch.Replace, StringComparison.Ordinal));
            return;
        }

        if (!text.Contains(patch.Replace, StringComparison.Ordinal))
            throw new InvalidOperationException($"{library.Name}: source patch search text was not found in {patch.Path}.");
    }
}
