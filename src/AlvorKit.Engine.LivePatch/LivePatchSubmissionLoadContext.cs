namespace AlvorKit.Engine;

/// <summary>Collectible submitted-code context that shares the running game's loaded assemblies.</summary>
internal sealed class LivePatchSubmissionLoadContext() : AssemblyLoadContext(isCollectible: true)
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
