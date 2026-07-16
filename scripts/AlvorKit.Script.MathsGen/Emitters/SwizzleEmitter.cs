namespace AlvorKit.Script.MathsGen;

/// <summary>Emits generated coordinate, color, and texture swizzle properties.</summary>
internal static class SwizzleEmitter
{
    /// <summary>Alias groups used for swizzle names and storage access.</summary>
    private static readonly string[][] AliasGroups =
    [
        ["X", "Y", "Z", "W"],
        ["R", "G", "B", "A"],
        ["S", "T", "P", "Q"],
    ];

    /// <summary>Appends all 2-, 3-, and 4-component swizzles for <paramref name="vector"/>.</summary>
    public static void Emit(VectorSpec vector, MemberBlock members)
    {
        foreach (var aliases in AliasGroups)
        {
            for (var length = 2; length <= 4; length++)
            {
                foreach (var indices in Sequences(vector.Dimension, length))
                    EmitSwizzle(vector, members, aliases, indices);
            }
        }
    }

    /// <summary>Emits one swizzle property.</summary>
    private static void EmitSwizzle(VectorSpec vector, MemberBlock members, string[] aliases, int[] indices)
    {
        var name = string.Concat(indices.Select((index, slot) =>
            slot == 0 ? aliases[index] : aliases[index].ToLowerInvariant()));
        var returnType = vector.Scalar.VectorName(indices.Length);
        var getArgs = string.Join(", ", indices.Select(index => aliases[index]));
        if (indices.Distinct().Count() != indices.Length)
        {
            members.Append(MathsTemplate.Fragment("swizzle-readonly.csfrag.tmpl", ("Name", name), ("ReturnType", returnType),
                ("GetArguments", getArgs)));
            return;
        }

        members.Append(MathsTemplate.Fragment("swizzle-writable.csfrag.tmpl", ("Name", name), ("ReturnType", returnType),
            ("GetArguments", getArgs), ("SetAssignments", SetAssignments(aliases, indices))));
    }

    /// <summary>Returns setter assignments for a writable swizzle.</summary>
    private static string SetAssignments(string[] aliases, IReadOnlyList<int> indices)
    {
        var valueComponents = VectorCatalog.Components.Take(indices.Count).ToArray();
        return string.Join(Environment.NewLine, indices.Select((index, i) => $"            {aliases[index]} = value.{valueComponents[i]};"));
    }

    /// <summary>Returns every ordered sequence of length <paramref name="length"/> over component indices.</summary>
    private static IEnumerable<int[]> Sequences(int dimension, int length)
    {
        var indices = new int[length];
        foreach (var sequence in Sequences(dimension, indices, slot: 0))
            yield return sequence;
    }

    /// <summary>Recursively fills ordered component-index sequences.</summary>
    private static IEnumerable<int[]> Sequences(int dimension, int[] indices, int slot)
    {
        if (slot == indices.Length)
        {
            yield return indices.ToArray();
            yield break;
        }

        for (var index = 0; index < dimension; index++)
        {
            indices[slot] = index;
            foreach (var sequence in Sequences(dimension, indices, slot + 1))
                yield return sequence;
        }
    }
}
