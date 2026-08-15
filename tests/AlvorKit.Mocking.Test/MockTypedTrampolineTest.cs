namespace AlvorKit;

[TestClass]
public sealed class MockTypedTrampolineTest
{
    private static readonly MockBackendIdentity InterceptionBackend =
        new(MockBackendKind.Interception, 1);

    private delegate bool MixedPrefix(
        MethodInfo method,
        object instance,
        ref int result,
        int ordinary,
        in int input,
        ref int mutable,
        out int output,
        Span<int> values,
        out MockDispatchContinuation? state);

    private delegate bool OrdinaryPrefix(
        MethodInfo method,
        object instance,
        ref int result,
        int value,
        out MockDispatchContinuation? state);

    private delegate Exception? OrdinaryFinalizer(
        Exception? exception,
        MockDispatchContinuation? state,
        ref int result,
        int value);

    private delegate Exception? OriginalFinalizer(
        Exception? exception,
        MockDispatchContinuation? state,
        ref int result,
        ref int mutable,
        ref int output,
        bool shouldThrow);

    private delegate bool OriginalPrefix(
        MethodInfo method,
        object instance,
        ref int result,
        ref int mutable,
        out int output,
        bool shouldThrow,
        out MockDispatchContinuation? state);

    /// <summary>Configured ordinary return and writeback execute through the exact prefix in declared call order.</summary>
    [TestMethod]
    public void Prefix_ConfiguredReturnAndWritebackUseExistingControlPlane()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Mixed));
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = artifact.Prefix.CreateDelegate<MixedPrefix>();
        var target = new DispatchTarget();
        Mocked mocked = Attach(target, MockFallbackBehavior.Strict);

        try
        {
            mocked.AddConstant(
                method,
                CarrierArguments(artifact, 3, 4, 5),
                42,
                [4, 8, 9]);
            int result = -1;
            int input = 4;
            int mutable = 5;
            int[] storage = [10, 20];

            bool runOriginal = prefix(
                method,
                target,
                ref result,
                3,
                in input,
                ref mutable,
                out int output,
                storage,
                out MockDispatchContinuation? state);

            Assert.IsFalse(runOriginal);
            Assert.IsNull(state);
            Assert.AreEqual(42, result);
            Assert.AreEqual(4, input);
            Assert.AreEqual(8, mutable);
            Assert.AreEqual(9, output);
            CollectionAssert.AreEqual(new[] { 10, 20 }, storage);

            var invocations = mocked.Invocations.Snapshot().Invocations;
            Assert.AreEqual(1, invocations.Length);
            MockInvocation invocation = invocations[0];
            Assert.AreEqual(MockInvocationCompletionKind.Returned, invocation.Completion.Kind);
            Assert.AreEqual(3, invocation.Arguments[0].Entry.Value);
            Assert.AreEqual(4, invocation.Arguments[1].Entry.Value);
            Assert.AreEqual(5, invocation.Arguments[2].Entry.Value);
            Assert.AreEqual(MockUnavailableReason.OutHasNoEntryValue, invocation.Arguments[3].Entry.Unavailable!.Reason);
            Assert.AreEqual(
                MockUnavailableReason.ByRefLikeProjectionNotConfigured,
                invocation.Arguments[4].Entry.Unavailable!.Reason);
        }
        finally
        {
            Mock.Sealed.Remove(target);
        }
    }

    /// <summary>Ordinary callbacks run outside mock locking and receive no boxed live ref-struct value.</summary>
    [TestMethod]
    public void Prefix_OrdinaryCallbackKeepsLiveRefStructOutOfControlPlane()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Mixed));
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = artifact.Prefix.CreateDelegate<MixedPrefix>();
        var target = new DispatchTarget();
        Mocked mocked = Attach(target, MockFallbackBehavior.Strict);
        object?[]? observedCarrier = null;
        bool callbackHeldMockLock = true;

        try
        {
            mocked.AddCallback(
                method,
                CarrierArguments(artifact, 7, 11, 13),
                call =>
                {
                    callbackHeldMockLock = Monitor.IsEntered(mocked);
                    observedCarrier = (object?[])typeof(MockCall)
                        .GetField("arguments", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(call)!;
                    call.SetReference(2, 17);
                    call.SetReference(3, 19);
                    return 23;
                });
            int result = -1;
            int input = 11;
            int mutable = 13;
            int[] storage = [29];

            bool runOriginal = prefix(
                method,
                target,
                ref result,
                7,
                in input,
                ref mutable,
                out int output,
                storage,
                out MockDispatchContinuation? state);

            Assert.IsFalse(runOriginal);
            Assert.IsNull(state);
            Assert.IsFalse(callbackHeldMockLock);
            Assert.AreEqual(23, result);
            Assert.AreEqual(17, mutable);
            Assert.AreEqual(19, output);
            Assert.IsNotNull(observedCarrier);
            Assert.IsNull(observedCarrier[artifact.CarrierIndices[4]]);
            Assert.IsFalse(observedCarrier.Any(static value => value?.GetType().IsByRefLike == true));
            Assert.AreEqual(29, storage[0]);
        }
        finally
        {
            Mock.Sealed.Remove(target);
        }
    }

    /// <summary>Strict, loose, partial, and unmocked receivers preserve their existing fallback decisions.</summary>
    [TestMethod]
    public void Prefix_FallbackAndUnmockedDecisionsRemainStable()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Ordinary));
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = artifact.Prefix.CreateDelegate<OrdinaryPrefix>();
        var strictTarget = new DispatchTarget();
        var looseTarget = new DispatchTarget();
        var partialTarget = new DispatchTarget();
        var unmockedTarget = new DispatchTarget();
        Mocked strict = Attach(strictTarget, MockFallbackBehavior.Strict);
        Mocked loose = Attach(looseTarget, MockFallbackBehavior.Loose);
        Mocked partial = Attach(partialTarget, MockFallbackBehavior.Partial);

        try
        {
            int strictResult = -1;
            MockDispatchContinuation? strictState = null;
            MockException exception = Assert.Throws<MockException>(
                () => prefix(
                    method,
                    strictTarget,
                    ref strictResult,
                    3,
                    out strictState));
            StringAssert.Contains(exception.Message, nameof(DispatchTarget.Ordinary));
            Assert.IsNull(strictState);

            int looseResult = -1;
            Assert.IsFalse(prefix(
                method,
                looseTarget,
                ref looseResult,
                3,
                out MockDispatchContinuation? looseState));
            Assert.AreEqual(0, looseResult);
            Assert.IsNull(looseState);
            Assert.AreEqual(
                MockInvocationExecutionSource.LooseFallback,
                loose.Invocations.Snapshot().Invocations[0].Completion.Source);

            int partialResult = 31;
            Assert.IsTrue(prefix(
                method,
                partialTarget,
                ref partialResult,
                3,
                out MockDispatchContinuation? partialState));
            Assert.IsNotNull(partialState);
            partialResult = partialTarget.Ordinary(3);
            var finalizer = artifact.Finalizer.CreateDelegate<OrdinaryFinalizer>();
            Assert.IsNull(finalizer(null, partialState, ref partialResult, 3));
            Assert.AreEqual(6, partialResult);
            Assert.AreEqual(
                MockInvocationCompletionKind.Returned,
                partial.Invocations.Snapshot().Invocations[0].Completion.Kind);

            int unmockedResult = 37;
            Assert.IsTrue(prefix(
                method,
                unmockedTarget,
                ref unmockedResult,
                3,
                out MockDispatchContinuation? unmockedState));
            Assert.AreEqual(37, unmockedResult);
            Assert.IsNull(unmockedState);
            Assert.AreEqual(
                MockInvocationCompletionKind.Threw,
                strict.Invocations.Snapshot().Invocations[0].Completion.Kind);
        }
        finally
        {
            Mock.Sealed.Remove(strictTarget);
            Mock.Sealed.Remove(looseTarget);
            Mock.Sealed.Remove(partialTarget);
        }
    }

    /// <summary>One immutable generated artifact is reused while different mocks retain isolated behavior and history.</summary>
    [TestMethod]
    public void Cache_EquivalentSignatureReusesCodeWithoutSharingMockState()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Ordinary));
        MockTypedTrampolineArtifact firstArtifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        MockTypedTrampolineArtifact secondArtifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = firstArtifact.Prefix.CreateDelegate<OrdinaryPrefix>();
        var firstTarget = new DispatchTarget();
        var secondTarget = new DispatchTarget();
        Mocked first = Attach(firstTarget, MockFallbackBehavior.Strict);
        Mocked second = Attach(secondTarget, MockFallbackBehavior.Strict);

        try
        {
            first.AddConstant(method, [5], 11, []);
            second.AddConstant(method, [5], 22, []);
            int firstResult = 0;
            int secondResult = 0;

            Assert.IsFalse(prefix(
                method,
                firstTarget,
                ref firstResult,
                5,
                out MockDispatchContinuation? firstState));
            Assert.IsFalse(prefix(
                method,
                secondTarget,
                ref secondResult,
                5,
                out MockDispatchContinuation? secondState));

            Assert.AreSame(firstArtifact, secondArtifact);
            Assert.IsNull(firstState);
            Assert.IsNull(secondState);
            Assert.AreEqual(11, firstResult);
            Assert.AreEqual(22, secondResult);
            Assert.AreEqual(1, first.Invocations.Snapshot().Invocations.Length);
            Assert.AreEqual(1, second.Invocations.Snapshot().Invocations.Length);
            Assert.IsTrue(firstArtifact.Prefix.Module.Assembly.IsCollectible);
            AssertNoPerMockState(firstArtifact);
        }
        finally
        {
            Mock.Sealed.Remove(firstTarget);
            Mock.Sealed.Remove(secondTarget);
        }
    }

    /// <summary>Capture initializes skipped results and outputs while retaining only ordinary values.</summary>
    [TestMethod]
    public void Prefix_CaptureKeepsRefStructTransientAndInitializesOutputs()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Mixed));
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = artifact.Prefix.CreateDelegate<MixedPrefix>();
        var target = new DispatchTarget();
        _ = Attach(target, MockFallbackBehavior.Strict);

        try
        {
            Capture.Start(CaptureOperation.Setup);
            int result = -1;
            int input = 2;
            int mutable = 3;
            int[] storage = [5, 7];

            bool runOriginal = prefix(
                method,
                target,
                ref result,
                1,
                in input,
                ref mutable,
                out int output,
                storage,
                out MockDispatchContinuation? state);

            Assert.IsFalse(runOriginal);
            Assert.IsNull(state);
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, output);
            Assert.IsNotNull(Capture.Context.Args);
            Assert.IsNull(Capture.Context.Args[artifact.CarrierIndices[4]]);
        }
        finally
        {
            Capture.End();
            Mock.Sealed.Remove(target);
        }
    }

    /// <summary>The production prefix preserves original parameter types, direction, modifiers, names, and scoped metadata.</summary>
    [TestMethod]
    public void Prefix_EmittedMetadataPreservesExactOriginalParameters()
    {
        MethodInfo target = GetMethod(nameof(DispatchTarget.Mixed));
        MethodInfo prefix = MockTypedTrampolineCache.GetOrCreate(
            target,
            InterceptionBackend).Prefix;
        ParameterInfo[] expected = target.GetParameters();
        ParameterInfo[] actual = prefix.GetParameters()[3..^1];

        Assert.AreEqual(expected.Length, actual.Length);
        for (int index = 0; index < expected.Length; index++)
        {
            Assert.AreEqual(expected[index].ParameterType, actual[index].ParameterType);
            Assert.AreEqual(expected[index].Attributes, actual[index].Attributes);
            Assert.AreEqual(expected[index].Name, actual[index].Name);
            CollectionAssert.AreEqual(
                expected[index].GetRequiredCustomModifiers(),
                actual[index].GetRequiredCustomModifiers());
            CollectionAssert.AreEqual(
                expected[index].GetOptionalCustomModifiers(),
                actual[index].GetOptionalCustomModifiers());
        }

        string? expectedScoped = expected[4].GetCustomAttributesData()
            .Single(attribute => attribute.AttributeType.Name == "ScopedRefAttribute")
            .AttributeType.FullName;
        string? actualScoped = actual[4].GetCustomAttributesData()
            .Single(attribute => attribute.AttributeType.Name == "ScopedRefAttribute")
            .AttributeType.FullName;
        Assert.AreEqual(expectedScoped, actualScoped);
    }

    /// <summary>Partial finalization completes the original token once with ref-out exits or the exact thrown exception.</summary>
    [TestMethod]
    public void Finalizer_PartialReturnAndThrowCompleteExistingInvocation()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Original));
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(method, InterceptionBackend);
        var prefix = artifact.Prefix.CreateDelegate<OriginalPrefix>();
        var finalizer = artifact.Finalizer.CreateDelegate<OriginalFinalizer>();
        var returnedTarget = new DispatchTarget();
        var thrownTarget = new DispatchTarget();
        Mocked returned = Attach(returnedTarget, MockFallbackBehavior.Partial);
        Mocked thrown = Attach(thrownTarget, MockFallbackBehavior.Partial);

        try
        {
            int returnedResult = 0;
            int returnedMutable = 3;
            Assert.IsTrue(prefix(
                method,
                returnedTarget,
                ref returnedResult,
                ref returnedMutable,
                out int returnedOutput,
                false,
                out MockDispatchContinuation? returnedState));
            returnedResult = returnedTarget.Original(
                ref returnedMutable,
                out returnedOutput,
                false);

            Assert.IsNull(finalizer(
                null,
                returnedState,
                ref returnedResult,
                ref returnedMutable,
                ref returnedOutput,
                false));

            var returnedInvocations = returned.Invocations.Snapshot().Invocations;
            Assert.AreEqual(1, returnedInvocations.Length);
            Assert.AreEqual(MockInvocationCompletionKind.Returned, returnedInvocations[0].Completion.Kind);
            Assert.AreEqual(MockInvocationExecutionSource.PartialPassthrough, returnedInvocations[0].Completion.Source);
            Assert.AreEqual(returnedMutable, returnedInvocations[0].Arguments[0].Exit.Value);
            Assert.AreEqual(returnedOutput, returnedInvocations[0].Arguments[1].Exit.Value);
            Assert.AreEqual(returnedResult, returnedInvocations[0].Completion.Return!.Value);

            int thrownResult = 0;
            int thrownMutable = 5;
            Assert.IsTrue(prefix(
                method,
                thrownTarget,
                ref thrownResult,
                ref thrownMutable,
                out int thrownOutput,
                true,
                out MockDispatchContinuation? thrownState));
            Exception caught;
            try
            {
                thrownResult = thrownTarget.Original(
                    ref thrownMutable,
                    out thrownOutput,
                    true);
                throw new AssertFailedException("The original method was expected to throw.");
            }
            catch (ExpectedOriginalException exception)
            {
                caught = exception;
            }

            Assert.AreSame(
                caught,
                finalizer(
                    caught,
                    thrownState,
                    ref thrownResult,
                    ref thrownMutable,
                    ref thrownOutput,
                    true));
            var thrownInvocations = thrown.Invocations.Snapshot().Invocations;
            Assert.AreEqual(1, thrownInvocations.Length);
            Assert.AreEqual(MockInvocationCompletionKind.Threw, thrownInvocations[0].Completion.Kind);
            Assert.AreSame(caught, thrownInvocations[0].Completion.Exception);
            Assert.AreEqual(
                MockInvocationFailureStage.OriginalImplementation,
                thrownInvocations[0].Completion.FailureStage);
        }
        finally
        {
            Mock.Sealed.Remove(returnedTarget);
            Mock.Sealed.Remove(thrownTarget);
        }
    }

    /// <summary>Partial continuation records a managed-reference return only as unavailable borrowed metadata.</summary>
    [TestMethod]
    public void Continuation_BorrowedReturnIsNeverRetained()
    {
        MethodInfo method = GetMethod(nameof(DispatchTarget.Borrowed));
        var target = new DispatchTarget();
        Mocked mocked = Attach(target, MockFallbackBehavior.Partial);

        try
        {
            MockInvocationToken token = MockInvocationCapture.Open(
                mocked,
                method,
                [],
                MockBackendLabel.InterceptionInstance);
            var continuation = new MockDispatchContinuation(
                mocked,
                token,
                method);

            continuation.CompleteReturned([], 41);

            MockInvocation invocation = mocked.Invocations.Snapshot().Invocations[0];
            Assert.AreEqual(MockInvocationReturnKind.Unavailable, invocation.Completion.Return!.Kind);
            Assert.AreEqual(typeof(int).MakeByRefType(), invocation.Completion.Return.DeclaredType);
            Assert.AreEqual(
                MockUnavailableReason.BorrowedReturnNotRetained,
                invocation.Completion.Return.UnavailableReason);
            Assert.IsNull(invocation.Completion.Return.Value);
        }
        finally
        {
            Mock.Sealed.Remove(target);
        }
    }

    /// <summary>Unsupported open signatures are rejected before a weak source-module cache is created.</summary>
    [TestMethod]
    public void Cache_UnsupportedSignatureIsRejectedBeforeCacheMutation()
    {
        MethodInfo openMethod = CreateOpenGenericMethod();
        var caches = (System.Runtime.CompilerServices.ConditionalWeakTable<Module, MockTypedTrampolineCache>)
            typeof(MockTypedTrampolineCache)
                .GetField("caches", BindingFlags.Static | BindingFlags.NonPublic)!
                .GetValue(null)!;

        MockException exception = Assert.Throws<MockException>(
            () => MockTypedTrampolineCache.GetOrCreate(
                openMethod,
                InterceptionBackend));

        StringAssert.Contains(exception.Message, "open generic parameters");
        Assert.IsFalse(caches.TryGetValue(openMethod.Module, out _));
    }

    private static MethodInfo GetMethod(string name) =>
        typeof(DispatchTarget).GetMethod(name)!;

    private static Mocked Attach(
        DispatchTarget target,
        MockFallbackBehavior fallback)
    {
        var mocked = new Mocked(fallback, new TypeCache(typeof(DispatchTarget)));
        Mock.Sealed.Add(target, mocked);
        return mocked;
    }

    private static object?[] CarrierArguments(
        MockTypedTrampolineArtifact artifact,
        int ordinary,
        int input,
        int mutable)
    {
        var arguments = new object?[5];
        arguments[artifact.CarrierIndices[0]] = ordinary;
        arguments[artifact.CarrierIndices[1]] = input;
        arguments[artifact.CarrierIndices[2]] = mutable;
        return arguments;
    }

    private static void AssertNoPerMockState(MockTypedTrampolineArtifact artifact)
    {
        string[] forbidden = ["Mocked", "Setup", "Behavior", "Invocation", "History", "Session", "Delegate"];
        foreach (FieldInfo field in artifact.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            Assert.IsFalse(
                forbidden.Any(field.FieldType.Name.Contains),
                $"Generated artifact field '{field.Name}' retains '{field.FieldType}'.");
        }
    }

    private static MethodInfo CreateOpenGenericMethod()
    {
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"AlvorKit.Mocking.OpenSignature.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module = assembly.DefineDynamicModule("OpenSignature");
        TypeBuilder type = module.DefineType("OpenSignature.Target", TypeAttributes.Public);
        MethodBuilder method = type.DefineMethod(
            "Identity",
            MethodAttributes.Public,
            CallingConventions.HasThis);
        GenericTypeParameterBuilder parameter = method.DefineGenericParameters("T")[0];
        method.SetReturnType(parameter);
        method.SetParameters(parameter);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ret);
        return type.CreateType()!.GetMethod("Identity")!;
    }

    private sealed class DispatchTarget
    {
        private int borrowed = 41;

        public int Mixed(
            int ordinary,
            in int input,
            ref int mutable,
            out int output,
            scoped Span<int> values)
        {
            output = ordinary + input + mutable + values.Length;
            return output;
        }

        public int Ordinary(int value) => value * 2;

        public int Original(
            ref int mutable,
            out int output,
            bool shouldThrow)
        {
            mutable += 2;
            output = mutable * 3;
            if (shouldThrow)
                throw new ExpectedOriginalException();
            return output + 1;
        }

        public ref int Borrowed() => ref borrowed;
    }

    private sealed class ExpectedOriginalException : Exception;

}
