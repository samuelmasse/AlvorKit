namespace AlvorKit;

/// <summary>Selects Mocking operation interception capabilities.</summary>
public static class MockInterception
{
    /// <summary>
    /// Binds one selected owned caller site to an exact Mocking instance-operation
    /// wrapper while the caller route is being prepared.
    /// </summary>
    /// <typeparam name="TDelegate">
    /// The exact delegate type whose first parameter is the operation receiver
    /// and whose remaining parameters and return metadata match the operation.
    /// </typeparam>
    /// <param name="caller">The selected caller that owns the operation site.</param>
    /// <param name="originalIlOffset">
    /// The operation instruction offset in the caller's authoritative original
    /// method body.
    /// </param>
    /// <param name="operation">The closed instance method executed by the site.</param>
    /// <param name="original">
    /// The exact preserved-original operation delegate used for passthrough and
    /// unmatched behavior.
    /// </param>
    /// <returns>
    /// An exact typed wrapper suitable for the prepared route's managed handler.
    /// </returns>
    /// <remarks>
    /// This cold-path operation only binds Mocking behavior. The consumer remains
    /// responsible for preparing and activating the corresponding caller rewrite
    /// through <see cref="MockInterceptionPreparationCoordinator"/>.
    /// </remarks>
    public static TDelegate BindOwnedInstanceCaller<TDelegate>(
        MethodInfo caller,
        int originalIlOffset,
        MethodInfo operation,
        TDelegate original)
        where TDelegate : Delegate =>
        MockInterceptionRuntime.BindOwnedInstanceCaller(
            caller,
            originalIlOffset,
            operation,
            original);

    /// <summary>
    /// Enables the Interception operation backend. Repeated calls are
    /// idempotent.
    /// </summary>
    public static void Enable() =>
        MockRuntimeBackendRegistry.RegisterOperation(
            MockInterceptionOperationBackend.Instance);
}
