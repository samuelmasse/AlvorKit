namespace AlvorKit;

/// <summary>Identifies one configured receiver-free behavior contract.</summary>
internal enum MockReceiverFreeBehaviorKind
{
    Return,
    ReturnSequence,
    Callback,
    ReturnFactory,
    Throw,
    Passthrough,
    Strict,
    Substitute,
    SubstituteFactory,
    Observe,
    Transform,
    Replace
}

/// <summary>
/// Carries one behavior without invoking a typed delegate or retaining a
/// byref-like value.
/// </summary>
internal sealed class MockReceiverFreeBehavior
{
    private readonly MockSnapshotProjector[] projectors;

    private MockReceiverFreeBehavior(
        MockReceiverFreeBehaviorKind kind,
        object? value = null,
        Delegate? callback = null,
        Exception? exception = null,
        MockSnapshotProjector[]? projectors = null)
    {
        Kind = kind;
        Value = value;
        Callback = callback;
        Exception = exception;
        this.projectors = projectors?.ToArray() ?? [];
    }

    internal MockReceiverFreeBehaviorKind Kind { get; }

    internal object? Value { get; }

    internal Delegate? Callback { get; }

    internal Exception? Exception { get; }

    internal ReadOnlySpan<MockSnapshotProjector> Projectors =>
        projectors;

    internal static MockReceiverFreeBehavior OrdinaryReturn(
        object? value,
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.Return, value: value, projectors: projectors);

    internal static MockReceiverFreeBehavior ReturnSequence(
        object?[] values,
        MockSnapshotProjector[] projectors) =>
        new(
            MockReceiverFreeBehaviorKind.ReturnSequence,
            value: values.ToArray(),
            projectors: projectors);

    internal static MockReceiverFreeBehavior CallbackBehavior(
        Delegate callback,
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.Callback, callback: callback, projectors: projectors);

    internal static MockReceiverFreeBehavior ReturnFactory(
        Delegate factory,
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.ReturnFactory, callback: factory, projectors: projectors);

    internal static MockReceiverFreeBehavior Throw(
        Exception exception,
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.Throw, exception: exception, projectors: projectors);

    internal static MockReceiverFreeBehavior Passthrough(
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.Passthrough, projectors: projectors);

    internal static MockReceiverFreeBehavior Strict(
        MockSnapshotProjector[] projectors) =>
        new(MockReceiverFreeBehaviorKind.Strict, projectors: projectors);

    internal static MockReceiverFreeBehavior Substitute(object value) =>
        new(MockReceiverFreeBehaviorKind.Substitute, value: value);

    internal static MockReceiverFreeBehavior SubstituteFactory(Delegate factory) =>
        new(MockReceiverFreeBehaviorKind.SubstituteFactory, callback: factory);

    internal static MockReceiverFreeBehavior Observe(Delegate observer) =>
        new(MockReceiverFreeBehaviorKind.Observe, callback: observer);

    internal static MockReceiverFreeBehavior Transform(Delegate transform) =>
        new(MockReceiverFreeBehaviorKind.Transform, callback: transform);

    internal static MockReceiverFreeBehavior Replace(Delegate replacement) =>
        new(MockReceiverFreeBehaviorKind.Replace, callback: replacement);
}

/// <summary>
/// Immutable member, receiver, pattern, and optional site scope for one setup.
/// </summary>
internal sealed class MockReceiverFreeSetupDescriptor
{
    private readonly MockArgumentPattern[] patterns;

    internal MockReceiverFreeSetupDescriptor(
        MemberInfo operation,
        MockInvocationOperationKind operationKind,
        object? receiver,
        ReadOnlySpan<MockArgumentPattern> patterns,
        MockCallSite? site = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ValidateReceiver(operation, operationKind, receiver);
        site?.Validate(operation, operationKind);

        Operation = operation;
        OperationKind = operationKind;
        Receiver = receiver;
        this.patterns = patterns.ToArray();
        Site = site;
    }

    internal MemberInfo Operation { get; }

    internal MockInvocationOperationKind OperationKind { get; }

    internal object? Receiver { get; }

    internal ReadOnlySpan<MockArgumentPattern> Patterns => patterns;

    internal MockCallSite? Site { get; }

    internal MockReceiverFreeSetupDescriptor AtSite(MockCallSite site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new(
            Operation,
            OperationKind,
            Receiver,
            patterns,
            site);
    }

    private static void ValidateReceiver(
        MemberInfo operation,
        MockInvocationOperationKind operationKind,
        object? receiver)
    {
        if (operationKind is MockInvocationOperationKind.FieldRead or
            MockInvocationOperationKind.FieldWrite)
        {
            if (operation is not FieldInfo field)
                throw new MockException("A field setup requires exact FieldInfo metadata.");

            if (field.IsStatic == (receiver is not null))
            {
                string scope = field.IsStatic ? "static" : "instance";
                throw new MockException(
                    $"Field '{field.DeclaringType?.FullName}.{field.Name}' is " +
                    $"{scope}, but the setup receiver shape does not match.");
            }

            return;
        }

        if (receiver is not null)
        {
            throw new MockException(
                $"Receiver-free operation '{operation.Name}' cannot retain an " +
                "instance receiver.");
        }
    }
}

/// <summary>Publishes terminal clause choices into a caller-owned setup target.</summary>
internal sealed class MockReceiverFreeSetupPublisher
{
    private readonly Action<
        MockReceiverFreeSetupDescriptor,
        MockReceiverFreeBehavior> publish;

    internal MockReceiverFreeSetupPublisher(
        MockReceiverFreeSetupDescriptor descriptor,
        Action<MockReceiverFreeSetupDescriptor, MockReceiverFreeBehavior> publish)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(publish);

        Descriptor = descriptor;
        this.publish = publish;
    }

    internal MockReceiverFreeSetupDescriptor Descriptor { get; }

    internal MockReceiverFreeSetupPublisher AtSite(MockCallSite site) =>
        new(Descriptor.AtSite(site), publish);

    internal void Publish(MockReceiverFreeBehavior behavior)
    {
        ArgumentNullException.ThrowIfNull(behavior);
        publish(Descriptor, behavior);
    }
}
