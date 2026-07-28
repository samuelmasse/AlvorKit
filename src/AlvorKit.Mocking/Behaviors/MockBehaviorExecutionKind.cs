namespace AlvorKit.Mocking;

/// <summary>Identifies how a selected configured behavior is executed.</summary>
internal enum MockBehaviorExecutionKind
{
    /// <summary>Returns a configured value and optional reference writebacks.</summary>
    Return,

    /// <summary>Throws the configured exception stored in the execution value.</summary>
    Throw,

    /// <summary>Invokes an ordinary heap-safe callback.</summary>
    Callback,

    /// <summary>Invokes an exact-signature typed callback from generated code.</summary>
    TypedCallback,

    /// <summary>Invokes an exact-signature typed return factory.</summary>
    TypedReturnFactory,

    /// <summary>Publishes an exact stable managed-reference return factory.</summary>
    TypedRefReturnFactory,

    /// <summary>Observes or transforms one exact field value around its original opcode.</summary>
    ReceiverFreeFieldBehavior,

    /// <summary>Observes or replaces one constructor remainder after initialization.</summary>
    ReceiverFreeConstructorBehavior,

    /// <summary>Executes the preserved original for a selected setup.</summary>
    Passthrough,

    /// <summary>Rejects a selected setup with a strict diagnostic.</summary>
    Strict
}
