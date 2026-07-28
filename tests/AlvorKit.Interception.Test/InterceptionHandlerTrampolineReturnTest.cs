namespace AlvorKit.Interception.Test;

/// <summary>Verifies exact managed-reference and ref-struct return lifetimes.</summary>
[TestClass]
public sealed class InterceptionHandlerTrampolineReturnTest
{
    /// <summary>Mutable and readonly aliases survive trampoline retirement with exact identity.</summary>
    [TestMethod]
    public unsafe void ManagedReferenceReturnsPreserveAliasAfterRetirement()
    {
        var target = new ExactAliasTarget([13, 21]);
        var handler = new ExactAliasHandler([55, 89]);
        var mutable = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactAliasTarget>(nameof(ExactAliasTarget.Mutable)),
            handler,
            Method<ExactAliasHandler>(nameof(ExactAliasHandler.Mutable)));
        var readOnly = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactAliasTarget>(nameof(ExactAliasTarget.ReadOnly)),
            handler,
            Method<ExactAliasHandler>(nameof(ExactAliasHandler.ReadOnly)));

        Assert.IsTrue(mutable.TryAcquire(out var mutableEntry));
        Assert.IsTrue(readOnly.TryAcquire(out var readOnlyEntry));
        ref int mutableAlias =
            ref ((delegate* managed<ExactAliasTarget, ref int>)
                mutableEntry)(target);
        ref readonly int readOnlyAlias =
            ref ((delegate* managed<
                ExactAliasTarget,
                ref readonly int>)readOnlyEntry)(target);
        mutable.Dispose();
        readOnly.Dispose();

        ref int expectedMutable = ref handler.Mutable(target);
        ref readonly int expectedReadOnly =
            ref handler.ReadOnly(target);
        Assert.IsTrue(Unsafe.AreSame(
            ref mutableAlias,
            ref expectedMutable));
        Assert.IsTrue(Unsafe.AreSame(
            ref Unsafe.AsRef(in readOnlyAlias),
            ref Unsafe.AsRef(in expectedReadOnly)));
        mutableAlias = 144;
        Assert.AreEqual(144, expectedMutable);
        Assert.IsFalse(mutable.TryAcquire(out _));
        Assert.IsFalse(readOnly.TryAcquire(out _));
    }

    /// <summary>A borrowed ref-struct result remains valid after its trampoline retires.</summary>
    [TestMethod]
    public unsafe void RefStructReturnPreservesBorrowedShapeAfterRetirement()
    {
        var target = new ExactWindowTarget();
        var handler = new ExactWindowHandler([3, 5, 8]);
        var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactWindowTarget>(nameof(ExactWindowTarget.Window)),
            handler,
            Method<ExactWindowHandler>(nameof(ExactWindowHandler.Window)));

        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        ExactBorrowedWindow result =
            ((delegate* managed<
                ExactWindowTarget,
                ExactBorrowedWindow>)entryPoint)(target);
        trampoline.Dispose();

        Assert.IsTrue(result.Values.SequenceEqual([3, 5, 8]));
        Assert.AreEqual(1, handler.Calls);
        Assert.IsFalse(trampoline.TryAcquire(out _));
    }

    /// <summary>A contained ref-struct failure returns its safe default and deactivates.</summary>
    [TestMethod]
    public unsafe void RefStructReturnContainmentReturnsDefaultAndDeactivates()
    {
        using var trampoline = InterceptionHandlerTrampolineFactory.Create(
            Method<ExactWindowTarget>(nameof(ExactWindowTarget.Window)),
            new ThrowingExactWindowHandler(),
            Method<ThrowingExactWindowHandler>(
                nameof(ThrowingExactWindowHandler.Window)),
            InterceptionHandlerExceptionPolicy.ContainAndDeactivate);

        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        ExactBorrowedWindow result =
            ((delegate* managed<
                ExactWindowTarget,
                ExactBorrowedWindow>)entryPoint)(new());

        Assert.IsTrue(result.Values.IsEmpty);
        Assert.IsInstanceOfType<InvalidOperationException>(
            trampoline.Failure);
        Assert.IsFalse(trampoline.TryAcquire(out _));
    }

    /// <summary>Containment rejects managed-reference returns before publication.</summary>
    [TestMethod]
    public void ManagedReferenceReturnRejectsContainmentPolicy()
    {
        var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                Method<ExactAliasTarget>(nameof(ExactAliasTarget.Mutable)),
                new ExactAliasHandler([1, 2]),
                Method<ExactAliasHandler>(nameof(ExactAliasHandler.Mutable)),
                InterceptionHandlerExceptionPolicy.ContainAndDeactivate));

        StringAssert.Contains(exception.Message, "Propagate");
    }

    /// <summary>Managed references to ref-struct and pointer elements remain rejected.</summary>
    [TestMethod]
    public void UnsafeManagedReferenceElementsRemainRejected()
    {
        foreach (var name in new[]
                 {
                     nameof(UnsupportedAliasTarget.RefStruct),
                     nameof(UnsupportedAliasTarget.Pointer),
                 })
        {
            var exception = Assert.ThrowsExactly<NotSupportedException>(() =>
                InterceptionHandlerTrampolineFactory.Create(
                    Method<UnsupportedAliasTarget>(name),
                    new UnsupportedAliasHandler(),
                    Method<UnsupportedAliasHandler>(name)));
            StringAssert.Contains(exception.Message, "unsupported");
        }
    }

    private static MethodInfo Method<T>(string name) =>
        typeof(T).GetMethod(name) ??
        throw new InvalidOperationException(
            $"Method '{typeof(T).FullName}.{name}' was not found.");
}

/// <summary>Defines mutable and readonly managed-reference target signatures.</summary>
public sealed class ExactAliasTarget(int[] values)
{
    /// <summary>Returns a mutable target alias.</summary>
    public ref int Mutable() => ref values[0];

    /// <summary>Returns a readonly target alias.</summary>
    public ref readonly int ReadOnly() => ref values[1];
}

/// <summary>Provides mutable and readonly replacement aliases.</summary>
public sealed class ExactAliasHandler(int[] values)
{
    /// <summary>Returns the replacement mutable alias.</summary>
    public ref int Mutable(ExactAliasTarget _) => ref values[0];

    /// <summary>Returns the replacement readonly alias.</summary>
    public ref readonly int ReadOnly(ExactAliasTarget _) =>
        ref values[1];
}

/// <summary>Represents a borrowed result returned through an exact trampoline.</summary>
public readonly ref struct ExactBorrowedWindow(ReadOnlySpan<int> values)
{
    /// <summary>Gets the borrowed values.</summary>
    public ReadOnlySpan<int> Values { get; } = values;
}

/// <summary>Defines one ref-struct return target signature.</summary>
public sealed class ExactWindowTarget
{
    /// <summary>Returns the target's empty borrowed result.</summary>
    public ExactBorrowedWindow Window() => new([]);
}

/// <summary>Returns a borrowed result from handler-owned storage.</summary>
public sealed class ExactWindowHandler(int[] values)
{
    /// <summary>Gets the number of handler calls.</summary>
    public int Calls { get; private set; }

    /// <summary>Returns a borrowed view over handler-owned storage.</summary>
    public ExactBorrowedWindow Window(ExactWindowTarget _)
    {
        Calls++;
        return new(values);
    }
}

/// <summary>Throws to verify safe ref-struct containment.</summary>
public sealed class ThrowingExactWindowHandler
{
    /// <summary>Throws before producing a borrowed result.</summary>
    public ExactBorrowedWindow Window(ExactWindowTarget _) =>
        throw new InvalidOperationException("contained borrowed failure");
}

/// <summary>Defines managed-reference element shapes outside the exact trampoline ABI.</summary>
public sealed class UnsupportedAliasTarget
{
    /// <summary>Never returns an unsupported ref-struct alias.</summary>
    public ref Span<int> RefStruct() =>
        throw new NotSupportedException();

    /// <summary>Never returns an unsupported pointer alias.</summary>
    public unsafe ref int* Pointer() =>
        throw new NotSupportedException();
}

/// <summary>Mirrors unsupported target signatures for pristine validation.</summary>
public sealed class UnsupportedAliasHandler
{
    /// <summary>Never handles an unsupported ref-struct alias.</summary>
    public ref Span<int> RefStruct(UnsupportedAliasTarget _) =>
        throw new NotSupportedException();

    /// <summary>Never handles an unsupported pointer alias.</summary>
    public unsafe ref int* Pointer(UnsupportedAliasTarget _) =>
        throw new NotSupportedException();
}
