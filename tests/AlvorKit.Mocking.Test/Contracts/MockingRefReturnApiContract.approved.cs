namespace AlvorKit;

// Mock.When<T> and ReturnFactory preserve exact by-value ref-struct return
// types. Ordinary Return, ReturnSequence, and Answer remain heap-safe extension
// methods and keep their existing source syntax for ordinary T. ReturnOwned
// copies setup input once for mutable or read-only span-returning clauses.
internal static class MockingRefReturnApiContract
{
    internal static void BorrowedFactories(
        IRefReturnContractTarget target,
        RefReturnContractOwner owner)
    {
        Mock.When(target.Mutable)
            .ReturnFactory(owner.Mutable);
        Mock.When(target.ReadOnly)
            .ReturnFactory(owner.ReadOnly);
        Mock.When(target.Window)
            .ReturnFactory(owner.Window);
        Mock.When(() => target.Generic<ReadOnlySpan<int>>())
            .ReturnFactory(owner.ReadOnly);
    }

    internal static void OrdinarySourceCompatibility(
        IRefReturnContractTarget target)
    {
        Mock.When(target.Value).Return(3);
        Mock.When(target.Value).ReturnSequence(5, 8);
        Mock.When(target.Value).Answer(_ => 13);
    }

    internal static void OwnedSpanReturns(
        IRefReturnContractTarget target,
        int[] array,
        Span<int> mutable,
        ReadOnlySpan<int> readOnly)
    {
        Mock.When(target.Mutable)
            .ReturnOwned(array);
        Mock.When(target.Mutable)
            .ReturnOwned(mutable);
        Mock.When(target.ReadOnly)
            .ReturnOwned(array);
        Mock.When(target.ReadOnly)
            .ReturnOwned(readOnly);
    }
}

internal interface IRefReturnContractTarget
{
    Span<int> Mutable();
    ReadOnlySpan<int> ReadOnly();
    RefReturnContractWindow Window();
    T Generic<T>()
        where T : allows ref struct;
    int Value();
}

internal readonly ref struct RefReturnContractWindow
{
    private readonly ReadOnlySpan<int> values;

    internal RefReturnContractWindow(ReadOnlySpan<int> values)
    {
        this.values = values;
    }

    internal ReadOnlySpan<int> Values => values;
}

internal sealed class RefReturnContractOwner(int[] values)
{
    internal Span<int> Mutable() => values;

    internal ReadOnlySpan<int> ReadOnly() => values;

    internal RefReturnContractWindow Window() => new(values);
}
