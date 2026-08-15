namespace AlvorKit;

// Production preserves these exact mutable and readonly managed-reference
// distinctions.
internal static class MockingManagedRefApiContract
{
    internal static void Mutable(
        IManagedRefContractTarget target,
        ManagedRefContractOwner owner)
    {
        global::AlvorKit.Mock.WhenRef(target.Mutable)
            .ReturnRef(owner.Mutable);
        global::AlvorKit.Mock.WhenRef(
                () => ref target.Mutable())
            .ReturnRef(() => ref owner.Mutable());
        global::AlvorKit.Mock.WhenRef(target.Mutable)
            .ReturnRef(13);
    }

    internal static void ReadOnly(
        IManagedRefContractTarget target,
        ManagedRefContractOwner owner)
    {
        global::AlvorKit.Mock.WhenRefReadonly(
                target.ReadOnly)
            .ReturnRef(owner.ReadOnly);
        global::AlvorKit.Mock.WhenRefReadonly(
                () => ref target.ReadOnly())
            .ReturnRef(() => ref owner.ReadOnly());
        global::AlvorKit.Mock.WhenRefReadonly(
                target.ReadOnly)
            .ReturnRef(21);
    }
}

internal interface IManagedRefContractTarget
{
    ref int Mutable();

    ref readonly int ReadOnly();
}

internal sealed class ManagedRefContractOwner
{
    private int mutable;
    private readonly int readOnly;

    internal ref int Mutable() => ref mutable;

    internal ref readonly int ReadOnly() => ref readOnly;
}
