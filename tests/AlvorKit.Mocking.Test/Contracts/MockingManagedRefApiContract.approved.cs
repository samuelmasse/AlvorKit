namespace AlvorKit.Mocking.Test.Contracts.ManagedRefs;

// Production preserves these exact mutable and readonly managed-reference
// distinctions.
internal static class MockingManagedRefApiContract
{
    internal static void Mutable(
        IManagedRefContractTarget target,
        ManagedRefContractOwner owner)
    {
        global::AlvorKit.Mocking.Mock.WhenRef(target.Mutable)
            .ReturnRef(owner.Mutable);
        global::AlvorKit.Mocking.Mock.WhenRef(
                () => ref target.Mutable())
            .ReturnRef(() => ref owner.Mutable());
        global::AlvorKit.Mocking.Mock.WhenRef(target.Mutable)
            .ReturnRef(13);
    }

    internal static void ReadOnly(
        IManagedRefContractTarget target,
        ManagedRefContractOwner owner)
    {
        global::AlvorKit.Mocking.Mock.WhenRefReadonly(
                target.ReadOnly)
            .ReturnRef(owner.ReadOnly);
        global::AlvorKit.Mocking.Mock.WhenRefReadonly(
                () => ref target.ReadOnly())
            .ReturnRef(() => ref owner.ReadOnly());
        global::AlvorKit.Mocking.Mock.WhenRefReadonly(
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
