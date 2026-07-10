namespace AlvorKit.ECS;

internal readonly record struct EntArchFieldLayout
{
    private readonly int encoded;

    internal bool ContainsReferences => encoded < 0;

    internal int BytePrefix => encoded;

    internal int TypeColumn => ~encoded;

    internal static EntArchFieldLayout ReferenceFree(int bytePrefix) => new(bytePrefix);

    internal static EntArchFieldLayout ReferenceContaining(int typeColumn) => new(~typeColumn);

    private EntArchFieldLayout(int encoded)
    {
        this.encoded = encoded;
    }
}
