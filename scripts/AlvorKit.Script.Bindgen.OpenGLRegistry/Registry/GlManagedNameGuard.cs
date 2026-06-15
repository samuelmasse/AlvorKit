namespace AlvorKit.Script.Bindgen;

/// <summary>Validation helper for generated managed names.</summary>
internal static class GlManagedNameGuard
{
    /// <summary>Throws when two native declarations project to the same managed name.</summary>
    public static void AssertUnique(IEnumerable<(string Managed, string Native)> names, string what)
    {
        var collisions = names
            .GroupBy(name => name.Managed)
            .Where(group => group.Count() > 1)
            .ToList();
        if (collisions.Count == 0)
            return;

        var details = collisions
            .Take(5)
            .Select(group => $"{group.Key} ({string.Join(", ", group.Select(name => name.Native))})");
        throw new InvalidOperationException($"Colliding managed {what} names: {string.Join("; ", details)}");
    }
}
