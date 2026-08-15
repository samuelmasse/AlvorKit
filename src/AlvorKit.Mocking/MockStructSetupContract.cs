namespace AlvorKit;

/// <summary>Validates and materializes live-struct setup contracts.</summary>
internal static class MockStructSetupContract
{
    /// <summary>Validates that capture preserved the exact live receiver signature.</summary>
    internal static MockReceiverFreeIdentity ValidateCapture<T>(
        MockStructSetupDescriptor descriptor,
        MockCapturedInvocation captured)
        where T : struct
    {
        MockReceiverFreeIdentity identity =
            captured.Mocked.ReceiverFree ??
            throw new MockException(
                "Struct operation capture requires a interception live-ref call site.");
        if (identity.Site.OperationKind !=
                MockInvocationOperationKind.StructMethod ||
            identity.Operation is not MethodInfo
            {
                IsStatic: false,
                DeclaringType: { } declaringType
            } ||
            !(declaringType == typeof(T) ||
              declaringType.IsInterface &&
              declaringType.IsAssignableFrom(typeof(T))))
        {
            throw new MockException(
                $"Captured operation is not an instance method on " +
                $"'{typeof(T)}'.");
        }

        ParameterInfo[] parameters = captured.Method.GetParameters();
        if (parameters.Length == 0 ||
            parameters[0].ParameterType !=
                typeof(T).MakeByRefType() ||
            captured.Method.ReturnType != descriptor.ResultType)
        {
            throw new MockException(
                "The interception struct call does not preserve the frozen exact " +
                "live-this signature.");
        }

        return identity;
    }

    /// <summary>Creates the receiver pattern selected by a struct scope.</summary>
    internal static MockArgumentPattern ReceiverPattern<T>(
        MockStructScopeDescriptor scope)
        where T : struct =>
        scope.Mode == MockStructMode.ValueMatched
            ? new(
                new Matcher(
                    MatcherType.TypedPredicate,
                    new MockTypedMatcher<T>(
                        (RefPredicate<T>)scope.Predicate!,
                        "struct receiver predicate")))
            : new(new Matcher(MatcherType.Any, null));

    /// <summary>Copies immutable live-receiver projections into setup projectors.</summary>
    internal static MockSnapshotProjector[] Projectors(
        MockStructSetupDescriptor descriptor)
    {
        ReadOnlySpan<MockStructThisProjection> source =
            descriptor.Projections;
        var projectors = new MockSnapshotProjector[source.Length];
        for (var index = 0; index < source.Length; index++)
            projectors[index] = source[index].Projector;
        return projectors;
    }

    /// <summary>Converts one public struct behavior into its runtime behavior.</summary>
    internal static MockConfiguredBehavior ConfigureBehavior(
        MockStructBehavior behavior,
        MethodInfo method,
        Type resultType) =>
        behavior.Kind switch
        {
            MockStructBehaviorKind.Callback =>
                new MockTypedCallbackBehavior(
                    MockTypedCallbackContract.Normalize(
                        behavior.Callback!,
                        method)),
            MockStructBehaviorKind.Return =>
                new MockConstantBehavior(
                    ValidateReturn(
                        behavior.Value,
                        resultType),
                    []),
            MockStructBehaviorKind.ReturnFactory =>
                ValidateFactory(
                    behavior.Callback!,
                    resultType),
            MockStructBehaviorKind.Throw =>
                new MockThrowBehavior(behavior.Exception!),
            MockStructBehaviorKind.Passthrough =>
                new MockPassthroughBehavior(),
            MockStructBehaviorKind.Strict =>
                new MockStrictBehavior(),
            _ => throw new UnreachableException()
        };

    /// <summary>Validates a heap-safe constant result.</summary>
    private static object? ValidateReturn(
        object? value,
        Type resultType)
    {
        if (resultType == typeof(void) ||
            resultType.IsByRefLike ||
            resultType.IsByRef ||
            resultType.IsPointer ||
            value is not null &&
            !resultType.IsInstanceOfType(value))
        {
            throw new MockException(
                $"Struct return value does not match '{resultType}'.");
        }

        return value;
    }

    /// <summary>Validates an exact zero-argument struct return factory.</summary>
    private static MockConfiguredBehavior ValidateFactory(
        Delegate factory,
        Type resultType)
    {
        MethodInfo invoke =
            factory.GetType().GetMethod(nameof(Action.Invoke))!;
        if (invoke.GetParameters().Length != 0 ||
            invoke.ReturnType != resultType)
        {
            throw new MockException(
                $"Struct return factory must be an exact zero-argument " +
                $"'{resultType}' factory.");
        }

        return new MockTypedReturnFactoryBehavior(factory);
    }

    /// <summary>Rejects receiver mutations for readonly live-this storage.</summary>
    internal static void ValidateMutableThis(
        MockStructSetupDescriptor descriptor,
        MethodInfo logicalMethod)
    {
        if (descriptor.Mutations.Length == 0)
            return;

        ParameterInfo receiver = logicalMethod.GetParameters()[0];
        bool readOnly = receiver.IsIn ||
            receiver.GetRequiredCustomModifiers().Any(
                static type =>
                    type.FullName ==
                    "System.Runtime.CompilerServices.IsReadOnlyAttribute");
        if (readOnly)
        {
            throw new MockException(
                $"Readonly struct receiver '{descriptor.Scope.StructType}' " +
                "cannot use entry or exit mutation.");
        }
    }
}
