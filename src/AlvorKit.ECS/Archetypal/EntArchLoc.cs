namespace AlvorKit.ECS;

/// <summary>Locates an Ent inside one archetypal group's row-set and arch catalogs.</summary>
internal struct EntArchLoc(int rowSetId, int archId, int row)
{
    /// <summary>Indexes the flat closed-generic component directories.</summary>
    internal int RowSetId = rowSetId;
    /// <summary>Identifies the immutable component signature used by structural operations.</summary>
    internal int ArchId = archId;
    /// <summary>Indexes aligned Ent and component arrays inside the row set.</summary>
    internal int Row = row;
}
