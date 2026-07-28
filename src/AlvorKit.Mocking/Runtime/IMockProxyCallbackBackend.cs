namespace AlvorKit.Mocking;

/// <summary>
/// Adapts mock construction, generic capture, and exact callback normalization
/// to one selected executable backend.
/// </summary>
internal interface IMockProxyCallbackBackend
{
    /// <summary>Gets the backend name used in capability diagnostics.</summary>
    string Name { get; }

    /// <summary>Resolves the concrete runtime type used for one full mock.</summary>
    [return: DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicConstructors |
        DynamicallyAccessedMemberTypes.NonPublicConstructors)]
    Type ResolveMockType(Type mockedType);

    /// <summary>Prepares backend-specific generic capture metadata.</summary>
    void PrepareCapture(Delegate capture);

    /// <summary>Normalizes one validated callback for exact backend dispatch.</summary>
    Delegate NormalizeCallback(
        Delegate callback,
        MethodInfo capturedMethod);

    /// <summary>Normalizes one constructor callback for exact backend dispatch.</summary>
    Delegate NormalizeConstructorCallback(
        Delegate callback,
        MethodInfo logicalMethod);
}
