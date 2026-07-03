namespace AlvorKit.Ranges;

/// <summary>Stores one free block plus intrusive links for the address and size indexes.</summary>
internal struct RangeFreeBlockNode
{
    /// <summary>Gets or sets the free block start index.</summary>
    internal long Index;

    /// <summary>Gets or sets the free block byte count.</summary>
    internal long Size;

    /// <summary>Gets or sets the deterministic treap priority.</summary>
    internal uint Priority;

    /// <summary>Gets or sets the left child in the address index.</summary>
    internal int AddressLeft;

    /// <summary>Gets or sets the right child in the address index.</summary>
    internal int AddressRight;

    /// <summary>Gets or sets the parent in the address index.</summary>
    internal int AddressParent;

    /// <summary>Gets or sets the left child in the size index.</summary>
    internal int SizeLeft;

    /// <summary>Gets or sets the right child in the size index.</summary>
    internal int SizeRight;

    /// <summary>Gets or sets the parent in the size index.</summary>
    internal int SizeParent;

    /// <summary>Gets or sets the next reusable node slot.</summary>
    internal int NextFree;
}
