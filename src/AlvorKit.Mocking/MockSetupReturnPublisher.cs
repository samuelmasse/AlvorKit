namespace AlvorKit.Mocking;

/// <summary>Publishes ordinary and managed-reference constant return setups.</summary>
internal static class MockSetupReturnPublisher
{
    /// <summary>Publishes a constant through the receiver-free or instance setup store.</summary>
    internal static void Publish<TValue>(
        Mocked? mocked,
        MethodInfo method,
        object?[] arguments,
        MockReceiverFreeSetupPublisher? receiverFree,
        TValue value,
        MockSnapshotProjector[] projectors)
    {
        if (receiverFree is not null)
        {
            receiverFree.Publish(
                MockReceiverFreeBehavior.OrdinaryReturn(
                    value,
                    projectors));
            return;
        }

        if (!MockManagedReferenceAbi.IsSupported(method.ReturnType))
        {
            mocked!.AddConstant(
                method,
                arguments,
                value,
                [],
                projectors);
            return;
        }

        var storage = new MockRefStorage<TValue>(value);
        MockReturnKind kind =
            MockCanonicalSignature.Create(method).Return.Kind;
        if (kind == MockReturnKind.ReadOnlyManagedReference)
        {
            mocked!.AddRefReadonlyReturnFactory(
                method,
                arguments,
                storage.ReadOnly,
                projectors);
            return;
        }

        mocked!.AddRefReturnFactory(
            method,
            arguments,
            storage.Mutable,
            projectors);
    }
}
