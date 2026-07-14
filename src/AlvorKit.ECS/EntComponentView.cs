namespace AlvorKit.ECS;

/// <summary>Describes sparse and archetypal components for diagnostics and string rendering.</summary>
internal abstract class EntComponentView
{
    /// <summary>Gets the marker type that names the component.</summary>
    internal abstract Type NameType();

    /// <summary>Gets the component value type.</summary>
    internal abstract Type ValueType();

    /// <summary>Gets the archetypal group type, or null for sparse storage.</summary>
    internal abstract Type? ArchGroupType();

    /// <summary>Returns whether the supplied Ent currently has the component.</summary>
    internal abstract bool Has(Ent ent);

    /// <summary>Gets the component value for diagnostics.</summary>
    internal abstract object? Get(Ent ent);

    /// <summary>Builds the debugger name, qualifying archetypal components by group.</summary>
    internal string DebugName()
    {
        string name = NameType().Name;
        Type? group = ArchGroupType();
        return group == null ? name : $"{group.Name}.{name}";
    }

    /// <summary>Builds the string-rendering name and removes the conventional Component suffix.</summary>
    internal string StringName()
    {
        string name = NameType().Name.Replace("Component", "");
        Type? group = ArchGroupType();
        return group == null ? name : $"{group.Name}.{name}";
    }
}
