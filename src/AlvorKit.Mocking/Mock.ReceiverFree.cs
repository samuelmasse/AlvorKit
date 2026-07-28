namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Captures immutable metadata for one interception void operation site.</summary>
    public static MockCallSite Site(Action operation)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return MockReceiverFreeApiBoundary.CaptureSite(operation);
    }

    /// <summary>Captures immutable metadata for one interception value-returning operation site.</summary>
    public static MockCallSite Site<T>(Func<T> operation)
        where T : allows ref struct
    {
        ArgumentNullException.ThrowIfNull(operation);
        return MockReceiverFreeApiBoundary.CaptureSite(operation);
    }

    /// <summary>Captures one object-allocation site for substitution or passthrough.</summary>
    public static MockConstructionSetupClause<T> WhenNew<T>(
        Func<T> construction)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(construction);
        return new(
            MockReceiverFreeApiBoundary.CaptureSetup(
                construction,
                MockInvocationOperationKind.Construction));
    }

    /// <summary>Captures one object-allocation site for count verification.</summary>
    public static MockVerification VerifyNew<T>(
        Func<T> construction)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(construction);
        return new(
            MockReceiverFreeApiBoundary.CaptureVerification(
                construction,
                MockInvocationOperationKind.Construction));
    }

    /// <summary>
    /// Captures a constructor body after its mandatory initializer while
    /// preserving allocation identity.
    /// </summary>
    public static MockConstructorBodySetupClause<T> WhenConstructorBody<T>(
        Func<T> construction)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(construction);
        return new(
            MockReceiverFreeApiBoundary.CaptureSetup(
                construction,
                MockInvocationOperationKind.ConstructorBody));
    }

    /// <summary>Captures one constructor body for count verification.</summary>
    public static MockVerification VerifyConstructorBody<T>(
        Func<T> construction)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(construction);
        return new(
            MockReceiverFreeApiBoundary.CaptureVerification(
                construction,
                MockInvocationOperationKind.ConstructorBody));
    }

    /// <summary>Creates a typed field handle from exact reflection metadata.</summary>
    public static MockField<TValue> Field<TValue>(FieldInfo field)
        where TValue : allows ref struct =>
        new(field);

    /// <summary>
    /// Creates a typed field handle for one field declared directly by
    /// <typeparamref name="TDeclaring"/>.
    /// </summary>
    public static MockField<TValue> Field<TDeclaring, TValue>(
        string name)
        where TValue : allows ref struct
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly;
        FieldInfo field = typeof(TDeclaring).GetField(name, flags) ??
            throw new MockException(
                $"Type '{typeof(TDeclaring)}' does not declare field '{name}'.");
        return new(field);
    }

    /// <summary>Configures one field read on an exact reference receiver.</summary>
    public static MockFieldReadSetupClause<TValue>
        WhenFieldRead<TTarget, TValue>(
            TTarget target,
            MockField<TValue> field)
        where TTarget : class
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateInstance(target, field);
        return new(
            MockReceiverFreeApiBoundary.FieldSetup<TValue>(
                field.Metadata,
                MockInvocationOperationKind.FieldRead,
                target,
                null));
    }

    /// <summary>Configures one static field read.</summary>
    public static MockFieldReadSetupClause<TValue>
        WhenFieldRead<TValue>(MockField<TValue> field)
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateStatic(field);
        return new(
            MockReceiverFreeApiBoundary.FieldSetup<TValue>(
                field.Metadata,
                MockInvocationOperationKind.FieldRead,
                null,
                null));
    }

    /// <summary>
    /// Configures one field write on an exact reference receiver. The value
    /// lambda executes inside matcher capture.
    /// </summary>
    public static MockFieldWriteSetupClause<TValue>
        WhenFieldWrite<TTarget, TValue>(
            TTarget target,
            MockField<TValue> field,
            Func<TValue> value)
        where TTarget : class
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateInstance(target, field);
        ArgumentNullException.ThrowIfNull(value);
        return new(
            MockReceiverFreeApiBoundary.FieldSetup(
                field.Metadata,
                MockInvocationOperationKind.FieldWrite,
                target,
                value));
    }

    /// <summary>
    /// Configures one static field write. The value lambda executes inside
    /// matcher capture.
    /// </summary>
    public static MockFieldWriteSetupClause<TValue>
        WhenFieldWrite<TValue>(
            MockField<TValue> field,
            Func<TValue> value)
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateStatic(field);
        ArgumentNullException.ThrowIfNull(value);
        return new(
            MockReceiverFreeApiBoundary.FieldSetup(
                field.Metadata,
                MockInvocationOperationKind.FieldWrite,
                null,
                value));
    }

    /// <summary>Captures one instance field read for count verification.</summary>
    public static MockVerification VerifyFieldRead<TTarget, TValue>(
        TTarget target,
        MockField<TValue> field)
        where TTarget : class
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateInstance(target, field);
        return new(
            MockReceiverFreeApiBoundary.FieldVerification<TValue>(
                field.Metadata,
                MockInvocationOperationKind.FieldRead,
                target,
                null));
    }

    /// <summary>Captures one static field read for count verification.</summary>
    public static MockVerification VerifyFieldRead<TValue>(
        MockField<TValue> field)
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateStatic(field);
        return new(
            MockReceiverFreeApiBoundary.FieldVerification<TValue>(
                field.Metadata,
                MockInvocationOperationKind.FieldRead,
                null,
                null));
    }

    /// <summary>Captures one instance field write for count verification.</summary>
    public static MockVerification VerifyFieldWrite<TTarget, TValue>(
        TTarget target,
        MockField<TValue> field,
        Func<TValue> value)
        where TTarget : class
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateInstance(target, field);
        ArgumentNullException.ThrowIfNull(value);
        return new(
            MockReceiverFreeApiBoundary.FieldVerification(
                field.Metadata,
                MockInvocationOperationKind.FieldWrite,
                target,
                value));
    }

    /// <summary>Captures one static field write for count verification.</summary>
    public static MockVerification VerifyFieldWrite<TValue>(
        MockField<TValue> field,
        Func<TValue> value)
        where TValue : allows ref struct
    {
        MockFieldContract.ValidateStatic(field);
        ArgumentNullException.ThrowIfNull(value);
        return new(
            MockReceiverFreeApiBoundary.FieldVerification(
                field.Metadata,
                MockInvocationOperationKind.FieldWrite,
                null,
                value));
    }

}
