namespace AlvorKit;

/// <summary>Owns one external FastNoise2 node reference and releases it during finalization.</summary>
internal class FnNodeHandle : SafeHandle
{
    private readonly Fn fn;

    /// <summary>Creates an owning managed handle for a native node reference.</summary>
    internal FnNodeHandle(Fn fn, FnNode node) : base(IntPtr.Zero, true)
    {
        this.fn = fn;
        SetHandle(node.Handle);
    }

    /// <inheritdoc />
    public override bool IsInvalid => handle == IntPtr.Zero;

    /// <summary>Releases the external reference through native <c>fnDeleteNodeRef</c>.</summary>
    protected override bool ReleaseHandle()
    {
        fn.DeleteNodeRef(new(handle));
        return true;
    }
}
