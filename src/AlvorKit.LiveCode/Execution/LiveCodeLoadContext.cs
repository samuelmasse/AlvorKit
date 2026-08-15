namespace AlvorKit;

/// <summary>Collectible context that reuses the target's already-loaded game and AlvorKit assemblies.</summary>
internal sealed class LiveCodeLoadContext() : AssemblyLoadContext(isCollectible: true)
{
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        foreach (var assembly in Default.Assemblies)
        {
            if (AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), assemblyName))
                return assembly;
        }

        return null;
    }
}
