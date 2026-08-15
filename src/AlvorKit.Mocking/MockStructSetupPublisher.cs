namespace AlvorKit;

/// <summary>Publishes one immutable struct setup into a caller-owned target.</summary>
internal sealed class MockStructSetupPublisher
{
    private readonly Action<
        MockStructSetupDescriptor,
        MockStructBehavior> publish;

    /// <summary>Creates a publisher around immutable setup metadata.</summary>
    internal MockStructSetupPublisher(
        MockStructSetupDescriptor descriptor,
        Action<MockStructSetupDescriptor, MockStructBehavior> publish)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(publish);
        Descriptor = descriptor;
        this.publish = publish;
    }

    /// <summary>Gets the immutable setup metadata awaiting publication.</summary>
    internal MockStructSetupDescriptor Descriptor { get; }

    /// <summary>Returns a publisher with one additional typed projection.</summary>
    internal MockStructSetupPublisher WithProjection<T, TResult>(
        MockSnapshotPhase phase,
        SnapshotProjector<T, TResult> projector)
        where T : struct =>
        new(
            Descriptor.WithProjection(
                phase,
                projector),
            publish);

    /// <summary>Returns a publisher with one additional receiver mutation.</summary>
    internal MockStructSetupPublisher WithMutation(
        MockSnapshotPhase phase,
        Delegate mutation) =>
        new(
            Descriptor.WithMutation(phase, mutation),
            publish);

    /// <summary>Publishes the final behavior with the accumulated metadata.</summary>
    internal void Publish(MockStructBehavior behavior)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        publish(Descriptor, behavior);
    }
}
