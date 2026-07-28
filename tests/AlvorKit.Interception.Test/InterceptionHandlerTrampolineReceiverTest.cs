namespace AlvorKit.Interception.Test;

/// <summary>Verifies exact managed-reference value receiver call shapes.</summary>
[TestClass]
public sealed class InterceptionHandlerTrampolineReceiverTest
{
    /// <summary>Mutable, readonly, and record receivers retain live storage.</summary>
    [TestMethod]
    public unsafe void ManagedReferenceReceiversPreserveCallerOwnedStorage()
    {
        var handler = new ExactValueReceiverHandler();
        Assert.IsTrue(
            Method<ExactValueReceiverHandler>(
                nameof(ExactValueReceiverHandler.Read))
            .GetParameters()[0]
            .IsIn);
        using var mutable = Create(
            Method<ExactMutableReceiver>(
                nameof(ExactMutableReceiver.Add)),
            typeof(ExactMutableReceiver),
            handler,
            nameof(ExactValueReceiverHandler.Add));
        using var readOnly =
            InterceptionHandlerTrampolineFactory.Create(
                InterceptionCallShape
                    .ForReadOnlyManagedReferenceReceiver(
                        Method<ExactReadonlyReceiver>(
                            nameof(ExactReadonlyReceiver.Read)),
                        typeof(ExactReadonlyReceiver)),
                handler,
                Method<ExactValueReceiverHandler>(
                    nameof(ExactValueReceiverHandler.Read)));
        using var record = Create(
            Method<ExactRecordReceiver>(
                nameof(ExactRecordReceiver.Read)),
            typeof(ExactRecordReceiver),
            handler,
            nameof(ExactValueReceiverHandler.ReadRecord));

        var mutableValue = new ExactMutableReceiver(3);
        var readOnlyValue = new ExactReadonlyReceiver(5);
        var recordValue = new ExactRecordReceiver(7);
        Assert.IsTrue(mutable.TryAcquire(out var mutableEntry));
        Assert.IsTrue(readOnly.TryAcquire(out var readOnlyEntry));
        Assert.IsTrue(record.TryAcquire(out var recordEntry));

        Assert.AreEqual(
            5,
            ((delegate* managed<
                ref ExactMutableReceiver,
                int,
                int>)mutableEntry)(ref mutableValue, 2));
        Assert.AreEqual(5, mutableValue.Value);
        Assert.AreEqual(
            8,
            ((delegate* managed<
                in ExactReadonlyReceiver,
                int,
                int>)readOnlyEntry)(in readOnlyValue, 3));
        Assert.AreEqual(
            11,
            ((delegate* managed<
                ref ExactRecordReceiver,
                int,
                int>)recordEntry)(ref recordValue, 4));
    }

    /// <summary>A constrained interface operation routes through its concrete ref receiver.</summary>
    [TestMethod]
    public unsafe void ConstrainedInterfaceUsesConcreteManagedReference()
    {
        var shape =
            InterceptionCallShape.ForManagedReferenceReceiver(
                Method<IExactValueMetric>(
                    nameof(IExactValueMetric.Measure)),
                typeof(ExactConstrainedReceiver));
        var handler = new ExactValueReceiverHandler();
        using var trampoline =
            InterceptionHandlerTrampolineFactory.Create(
                shape,
                handler,
                Method<ExactValueReceiverHandler>(
                    nameof(ExactValueReceiverHandler.Measure)));

        var receiver = new ExactConstrainedReceiver(17);
        Assert.IsTrue(trampoline.TryAcquire(out var entryPoint));
        int result = ((delegate* managed<
            ref ExactConstrainedReceiver,
            int,
            int>)entryPoint)(ref receiver, 7);

        Assert.AreEqual(24, result);
        Assert.AreEqual(24, receiver.Value);
        Assert.AreEqual(
            InterceptionReceiverOwnership.ManagedReference,
            shape.ReceiverOwnership);
        Assert.AreEqual(typeof(ExactConstrainedReceiver), shape.ReceiverType);
    }

    /// <summary>Declared arguments, returns, and custom modifiers remain exact.</summary>
    [TestMethod]
    public void DeclaredOperationSignatureRemainsExact()
    {
        var shape =
            InterceptionCallShape.ForManagedReferenceReceiver(
                Method<ExactMutableReceiver>(
                    nameof(ExactMutableReceiver.ReadIn)),
                typeof(ExactMutableReceiver));
        var handler = new ExactValueReceiverHandler();
        using var exact =
            InterceptionHandlerTrampolineFactory.Create(
                shape,
                handler,
                Method<ExactValueReceiverHandler>(
                    nameof(ExactValueReceiverHandler.ReadIn)));

        Assert.IsTrue(exact.TryAcquire(out var entryPoint));
        Assert.AreNotEqual(0, entryPoint);
        Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                shape,
                handler,
                Method<ExactValueReceiverHandler>(
                    nameof(ExactValueReceiverHandler.ReadWritable))));
        Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                InterceptionCallShape.ForManagedReferenceReceiver(
                    Method<ExactMutableReceiver>(
                        nameof(ExactMutableReceiver.Add)),
                    typeof(ExactMutableReceiver)),
                handler,
                Method<ExactValueReceiverHandler>(
                    nameof(ExactValueReceiverHandler.BadReturn))));
    }

    /// <summary>Ambiguous, mismatched, open, and byref-like receivers are rejected.</summary>
    [TestMethod]
    public void UnsafeOrMismatchedReceiverShapesAreRejected()
    {
        MethodInfo add = Method<ExactMutableReceiver>(
            nameof(ExactMutableReceiver.Add));
        Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionCallShape.ForManagedReferenceReceiver(
                add,
                typeof(ExactReadonlyReceiver)));
        Assert.ThrowsExactly<ArgumentException>(() =>
            InterceptionCallShape.ForManagedReferenceReceiver(
                add,
                typeof(ExactMutableReceiver).MakeByRefType()));
        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionCallShape.ForManagedReferenceReceiver(
                Method(
                    typeof(ExactOpenReceiver<>),
                    nameof(ExactOpenReceiver<>.Read)),
                typeof(ExactOpenReceiver<>)));
        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionCallShape.ForManagedReferenceReceiver(
                Method<IExactValueMetric>(
                    nameof(IExactValueMetric.Measure)),
                typeof(Span<int>)));
        Type nested = typeof(ExactNestedReceiver<>)
            .MakeGenericType(typeof(Span<int>));
        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionCallShape.ForManagedReferenceReceiver(
                Method(nested, nameof(ExactNestedReceiver<>.Read)),
                nested));
    }

    /// <summary>Legacy and explicit shape paths reject varargs consistently.</summary>
    [TestMethod]
    public void VarArgsCallShapesRemainRejected()
    {
        MethodInfo operation = Method(
            typeof(ExactVarArgsTarget),
            nameof(ExactVarArgsTarget.Observe));
        MethodInfo handler = Method(
            typeof(ExactVarArgsTarget),
            nameof(ExactVarArgsTarget.Handle));

        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                operation,
                null,
                handler));
        Assert.ThrowsExactly<NotSupportedException>(() =>
            InterceptionHandlerTrampolineFactory.Create(
                InterceptionCallShape.FromMethod(operation),
                null,
                handler));
    }

    private static InterceptionHandlerTrampoline Create(
        MethodInfo operation,
        Type receiverType,
        ExactValueReceiverHandler handler,
        string handlerName) =>
        InterceptionHandlerTrampolineFactory.Create(
            InterceptionCallShape.ForManagedReferenceReceiver(
                operation,
                receiverType),
            handler,
            Method<ExactValueReceiverHandler>(handlerName));

    private static MethodInfo Method<T>(string name) =>
        Method(typeof(T), name);

    private static MethodInfo Method(Type type, string name) =>
        type.GetMethod(name) ??
        throw new InvalidOperationException(
            $"Method '{type.FullName}.{name}' was not found.");
}
