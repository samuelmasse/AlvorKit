namespace AlvorKit;

/// <summary>Finds the managed runtime dependency closure of one application assembly without loading it.</summary>
internal static class LiveCodeDependencyCatalog
{
    internal static void AddTo(HashSet<string> paths, Assembly assembly)
    {
        if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
            return;

        var resolver = new AssemblyDependencyResolver(assembly.Location);
        var visited = new HashSet<string>(
            OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        var pending = new Queue<string>();
        paths.Add(assembly.Location);
        visited.Add(assembly.Location);
        pending.Enqueue(assembly.Location);

        while (pending.TryDequeue(out var path))
        {
            using var stream = File.OpenRead(path);
            using var portableExecutable = new PEReader(stream);
            var metadata = portableExecutable.GetMetadataReader();
            foreach (var handle in metadata.AssemblyReferences)
            {
                var reference = metadata.GetAssemblyReference(handle);
                var name = new AssemblyName
                {
                    Name = metadata.GetString(reference.Name),
                    Version = reference.Version,
                };
                if (!reference.Culture.IsNil)
                    name.CultureName = metadata.GetString(reference.Culture);

                var resolved = resolver.ResolveAssemblyToPath(name);
                if (resolved is not null)
                {
                    paths.Add(resolved);
                    if (visited.Add(resolved))
                        pending.Enqueue(resolved);
                }
            }
        }
    }
}
