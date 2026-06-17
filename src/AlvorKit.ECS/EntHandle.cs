namespace AlvorKit.ECS;

/// <summary>Stores the index and generation identity for an entity.</summary>
public readonly struct EntHandle
{
    internal readonly int Index;
    internal readonly int Generation;

    internal EntHandle(int index, int generation)
    {
        Index = index;
        Generation = generation;
    }

    internal bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => EntReg.PageGenerations[PageIndex][SubIndex] == Generation;
    }

    internal int PageIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Index >> EntReg.PageBits;
    }

    internal int SubIndex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        get => Index & EntReg.PageMask;
    }

    /// <summary>Formats this handle and selected components for diagnostics.</summary>
    public override string ToString() => ToStringCycleDetection([]);

    internal string ToStringCycleDetection(HashSet<(int, int)> seen)
    {
        if (!seen.Add((Index, Generation)))
            return "Ent { ... }";

        if (Index == 0)
            return "Ent Null";

        if (!IsAlive)
            return "Ent Disposed";

        var sb = new StringBuilder();
        sb.Append("Ent");
        sb.Append(" { ");

        var ent = new Ent(Index, Generation);
        var fields = EntReg.PageFields.Fields(PageIndex)
            .ToArray()
            .Where(x => x.Has(ent) &&
                x.NameType().CustomAttributes.Any(a => a.AttributeType.Name.Contains("ComponentToString")))
            .OrderBy(x => x.NameType().Name).ToArray();

        if (fields.Length > 0)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                string name = fields[i].NameType().Name.Replace("Component", "");
                object? value = fields[i].Get(ent);

                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(name);
                sb.Append(" = ");

                if (value != null && fields[i].ValueType().IsAssignableTo(typeof(IEnt)))
                    sb.Append(((IEnt)value).Handle.ToStringCycleDetection(seen));
                else sb.Append(value?.ToString());
            }

            sb.Append(' ');
        }

        sb.Append('}');
        return sb.ToString();
    }
}
