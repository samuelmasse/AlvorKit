namespace AlvorKit;

/// <summary>Rejects runtime assemblies whose callable API would differ from the package's compile-time reference.</summary>
public static class RuntimeBindingContract
{
    /// <summary>Requires identical managed and P/Invoke function signatures for every runtime.</summary>
    public static void Validate(IReadOnlyDictionary<string, BindingModel> models)
    {
        var reference = models.First();
        var expected = Signatures(reference.Value);

        foreach (var (runtime, model) in models)
        {
            var actual = Signatures(model);

            if (!expected.SequenceEqual(actual))
            {
                var differences = expected.Except(actual).Concat(actual.Except(expected));
                throw new InvalidOperationException($"{runtime} differs from {reference.Key}: {string.Join("; ", differences)}");
            }
        }
    }

    /// <summary>Orders signatures without upstream documentation or platform-dependent record internals.</summary>
    private static IEnumerable<string> Signatures(BindingModel model) => model.Functions.Select(function =>
        $"{function.NativeName}:{function.ReturnType}:{function.ReturnInteropType}(" +
        string.Join(",", function.Parameters.Select(parameter =>
            $"{parameter.Modifier}:{parameter.ManagedType}:{parameter.InteropType}")) + ")")
        .Order(StringComparer.Ordinal);
}
