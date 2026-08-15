namespace AlvorKit;

[TestClass]
public sealed class MockingRefSafeApiContractTest
{
    private static readonly MethodInfo TransformExactMethod =
        typeof(IRefSafeContractTarget).GetMethod(
            nameof(IRefSafeContractTarget.TransformExact))!;
    private static readonly MethodInfo TransformAnswerMethod =
        typeof(IRefSafeContractTarget).GetMethod(
            nameof(IRefSafeContractTarget.TransformAnswer))!;
    private static readonly MethodInfo WideMethod =
        typeof(IRefSafeContractTarget).GetMethod(
            nameof(IRefSafeContractTarget.Wide))!;

    /// <summary>Standard Action and Func delegates directly invoke one, several, and mixed borrowed parameters.</summary>
    [TestMethod]
    public void StandardDelegates_InvokeBorrowedParametersDirectly()
    {
        var observed = new int[2];
        ReadOnlySpan<int> source = [2, 3, 5];
        Span<int> destination = stackalloc int[3];
        void one(ReadOnlySpan<int> values) => observed[0] = values.Length;
        void several(ReadOnlySpan<int> values, Span<int> output)
        {
            values.CopyTo(output);
            observed[1] = output[2];
        }
        int mixed(int offset, ReadOnlySpan<int> values, Span<int> output)
        {
            values.CopyTo(output);
            return offset + values.Length;
        }

        one(source);
        several(source, destination);
        var result = mixed(10, source, destination);

        CollectionAssert.AreEqual(
            new[] { 2, 3, 5 },
            destination.ToArray());
        CollectionAssert.AreEqual(
            new[] { 3, 5 },
            observed);
        Assert.AreEqual(13, result);
    }

    /// <summary>Natural delegates preserve exact in, ref, and out shapes and remain directly invocable.</summary>
    [TestMethod]
    public void NaturalDelegate_InvokesExactReferenceShapeDirectly()
    {
        var callback =
            (
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                source.CopyTo(destination);
                written = new(destination[..source.Length]);
            };
        ReadOnlySpan<int> source = [8, 13];
        Span<int> destination = stackalloc int[2];

        callback(
            in source,
            ref destination,
            out var written);

        CollectionAssert.AreEqual(
            new[] { 8, 13 },
            written.Values.ToArray());
        var invoke = callback.GetType().GetMethod(
            nameof(Action.Invoke))!;
        Assert.IsTrue(
            invoke.GetParameters()[0].IsIn);
        Assert.IsTrue(
            invoke.GetParameters()[1].ParameterType.IsByRef);
        Assert.IsTrue(
            invoke.GetParameters()[2].IsOut);
    }

    /// <summary>A natural calculated-answer delegate preserves mixed values and exact reference modifiers.</summary>
    [TestMethod]
    public void NaturalAnswer_InvokesMixedReferenceShapeDirectly()
    {
        var answer =
            (
                int offset,
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                source.CopyTo(destination);
                written = new(destination[..source.Length]);
                return offset + source.Length;
            };
        ReadOnlySpan<int> source = [1, 1, 2, 3];
        Span<int> destination = stackalloc int[4];

        var result = answer(
            20,
            in source,
            ref destination,
            out var written);

        Assert.AreEqual(24, result);
        CollectionAssert.AreEqual(
            new[] { 1, 1, 2, 3 },
            written.Values.ToArray());
        Assert.AreEqual(
            typeof(int),
            answer.GetType()
                .GetMethod(nameof(Action.Invoke))!
                .ReturnType);

        Delegate normalized =
            RefSafeCallbackContract.Normalize(
                answer,
                TransformAnswerMethod);
        var direct =
            RefSafeStableDelegateCache
                .CreateDirectInvoker<ExactTransformAnswer>(
                    normalized,
                    TransformAnswerMethod);
        Span<int> secondDestination =
            stackalloc int[4];
        var normalizedResult = direct(
            30,
            in source,
            ref secondDestination,
            out var normalizedWritten);

        Assert.AreEqual(34, normalizedResult);
        CollectionAssert.AreEqual(
            new[] { 1, 1, 2, 3 },
            normalizedWritten.Values.ToArray());
    }

    /// <summary>A natural delegate normalizes to one generated exact delegate and remains directly invocable.</summary>
    [TestMethod]
    public void NaturalDelegate_NormalizesForGeneratedDirectInvocation()
    {
        var invocationCount = 0;
        var callback =
            (
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                invocationCount++;
                source.CopyTo(destination);
                written = new(destination[..source.Length]);
            };
        Delegate normalized =
            RefSafeCallbackContract.Normalize(
                callback,
                TransformExactMethod);
        var direct =
            RefSafeStableDelegateCache
                .CreateDirectInvoker<ExactTransformCallback>(
                    normalized,
                    TransformExactMethod);
        ReadOnlySpan<int> source = [5, 8];
        Span<int> destination = stackalloc int[2];

        direct(
            in source,
            ref destination,
            out var written);

        Assert.AreEqual(1, invocationCount);
        CollectionAssert.AreEqual(
            new[] { 5, 8 },
            written.Values.ToArray());
    }

    /// <summary>Fluent overloads select standard delegates for values and a natural delegate for exact references.</summary>
    [TestMethod]
    public void FluentOverloads_AreUnambiguous()
    {
        var voidClause = new RefSafeProofSetupClause();
        voidClause.Do(
            (ReadOnlySpan<int> _) => { });
        Assert.AreEqual(
            RefSafeCallbackKind.Action,
            voidClause.Kind);

        var exactVoidClause = new RefSafeProofSetupClause(
            TransformExactMethod);
        exactVoidClause.Do(
            (
                scoped in ReadOnlySpan<int> _,
                scoped ref Span<int> destination,
                scoped out BorrowedWindow written) =>
            {
                written = new(destination);
            });
        Assert.AreEqual(
            RefSafeCallbackKind.NaturalDelegate,
            exactVoidClause.Kind);

        var resultClause =
            new RefSafeProofSetupClause<int>();
        resultClause.Answer(
            (
                int offset,
                ReadOnlySpan<int> values,
                Span<int> _) =>
                offset + values.Length);
        Assert.AreEqual(
            RefSafeCallbackKind.Func,
            resultClause.Kind);

        var exactResultClause = new RefSafeProofSetupClause<int>(
            TransformAnswerMethod);
        MockingRefSafeCompilerProof.ExactReferenceCallbacks(
            exactVoidClause,
            exactResultClause);
        Assert.AreEqual(
            RefSafeCallbackKind.NaturalDelegate,
            exactVoidClause.Kind);
        Assert.AreEqual(
            RefSafeCallbackKind.NaturalDelegate,
            exactResultClause.Kind);
        Assert.AreEqual(
            RefSafeStableDelegateCache.GetOrCreate(
                TransformExactMethod),
            exactVoidClause.Callback!.GetType());
        Assert.AreEqual(
            RefSafeStableDelegateCache.GetOrCreate(
                TransformAnswerMethod),
            exactResultClause.Callback!.GetType());
        RefSafeCallbackContract.ValidateInvoke(
            exactVoidClause.Callback.GetType().GetMethod(
                nameof(Action.Invoke))!,
            TransformExactMethod);
        RefSafeCallbackContract.ValidateInvoke(
            exactResultClause.Callback.GetType().GetMethod(
                nameof(Action.Invoke))!,
            TransformAnswerMethod);
    }

    /// <summary>A seventeen-input lambda selects the natural fallback and stores one stable exact delegate.</summary>
    [TestMethod]
    public void NaturalDelegate_MoreThanSixteenInputs_NormalizesOnce()
    {
        var clause = new RefSafeProofSetupClause(
            WideMethod);
        int[] observed = [0];

        MockingRefSafeCompilerProof.WideCallback(
            clause,
            observed);

        Delegate stored = clause.Callback!;
        Assert.AreEqual(
            RefSafeCallbackKind.NaturalDelegate,
            clause.Kind);
        Assert.AreEqual(1, clause.NormalizationCount);
        Assert.AreEqual(
            RefSafeStableDelegateCache.GetOrCreate(
                WideMethod),
            stored.GetType());
        Assert.AreSame(stored, clause.Callback);
        RefSafeCallbackContract.ValidateInvoke(
            stored.GetType().GetMethod(nameof(Action.Invoke))!,
            WideMethod);

        var direct =
            RefSafeStableDelegateCache
                .CreateDirectInvoker<WideDirectCallback>(
                    stored,
                    WideMethod);
        direct(
            1, 2, 3, 4, 5, 6,
            7, 8, 9, 10, 11, 12,
            13, 14, 15, 16, 17);

        Assert.AreEqual(153, observed[0]);
    }

    /// <summary>Closed generic constructions validate and normalize to construction-specific stable delegate identities.</summary>
    [TestMethod]
    public void ClosedGenericSignatures_NormalizePerConstruction()
    {
        MethodInfo definition =
            typeof(IRefSafeContractTarget).GetMethod(
                nameof(IRefSafeContractTarget.Echo))!;
        MethodInfo integerMethod =
            definition.MakeGenericMethod(typeof(int));
        MethodInfo stringMethod =
            definition.MakeGenericMethod(typeof(string));
        Func<int, int> integerCallback =
            value => value + 1;
        Func<string, string> stringCallback =
            value => value + "!";

        Delegate integerNormalized =
            RefSafeCallbackContract.Normalize(
                integerCallback,
                integerMethod);
        Delegate stringNormalized =
            RefSafeCallbackContract.Normalize(
                stringCallback,
                stringMethod);
        var integerDirect =
            RefSafeStableDelegateCache
                .CreateDirectInvoker<Func<int, int>>(
                    integerNormalized,
                    integerMethod);
        var stringDirect =
            RefSafeStableDelegateCache
                .CreateDirectInvoker<Func<string, string>>(
                    stringNormalized,
                    stringMethod);

        Assert.AreEqual(8, integerDirect(7));
        Assert.AreEqual("ok!", stringDirect("ok"));
        Assert.AreNotEqual(
            integerNormalized.GetType(),
            stringNormalized.GetType());
        Assert.AreSame(
            integerNormalized.GetType(),
            RefSafeStableDelegateCache.GetOrCreate(
                integerMethod));

        var openError = Assert.Throws<ArgumentException>(
            () => RefSafeCallbackContract.Validate(
                integerCallback,
                definition));
        StringAssert.Contains(
            openError.Message,
            "must be closed");
    }

    /// <summary>Every canonical Invoke mismatch reports its exact deterministic signature facet.</summary>
    [TestMethod]
    public void ClosedCanonicalValidation_MismatchFacetsAreDeterministic()
    {
        RefSafeCallbackContract.ValidateInvoke(
            typeof(ExactTransformCallback).GetMethod(
                nameof(Action.Invoke))!,
            TransformExactMethod);

        (string Variant, string Facet)[] cases =
        [
            ("return", "return type"),
            ("count", "parameter count"),
            ("type", "parameter 0 type"),
            ("byref", "parameter 0 type"),
            ("in", "parameter 0 IsIn metadata"),
            ("out", "parameter 2 IsOut metadata"),
            ("required", "parameter 0 required custom modifiers"),
            ("optional", "parameter 0 optional custom modifiers"),
            ("scoped", "parameter 0 scoped metadata"),
        ];

        foreach ((string variant, string facet) in cases)
        {
            MethodInfo invoke = CreateInvokeVariant(variant);
            var error = Assert.Throws<ArgumentException>(
                () => RefSafeCallbackContract.ValidateInvoke(
                    invoke,
                    TransformExactMethod));

            StringAssert.Contains(error.Message, facet);
            Assert.AreEqual("callback", error.ParamName);
        }
    }

    /// <summary>Arbitrary ref structs work with standard callbacks, predicates, and heap-safe projections.</summary>
    [TestMethod]
    public void ArbitraryRefStruct_UsesLiveDelegatesAndHeapSafeProjection()
    {
        var clause = new RefSafeProofSetupClause();
        clause.Do(
            (BorrowedWindow _) => { });
        static int callback(BorrowedWindow window) =>
                window.Values.Length;
        static bool predicate(scoped in BorrowedWindow window) =>
                window.Values[0] == 21;
        static int[] projector(scoped in BorrowedWindow window) =>
                    window.Values.ToArray();
        ReadOnlySpan<int> source = [21, 34];
        var window = new BorrowedWindow(source);

        var length = callback(window);
        var matches = predicate(in window);
        var snapshot = projector(in window);
        source = [55, 89];

        Assert.AreEqual(2, length);
        Assert.IsTrue(matches);
        Assert.AreEqual(
            RefSafeCallbackKind.Action,
            clause.Kind);
        CollectionAssert.AreEqual(
            new[] { 21, 34 },
            snapshot);
    }

    /// <summary>Snapshot registration retains projector delegates while projected history retains only ordinary results.</summary>
    [TestMethod]
    public void SnapshotProjectors_RetainDelegatesAndReturnHeapSafeValues()
    {
        var clause = new RefSafeProofSetupClause();
        MockingRefSafeCompilerProof.Snapshots(clause);
        int[] source = [3, 8, 13];
        var window = new BorrowedWindow(source);
        SnapshotProjector<BorrowedWindow, int[]> projector =
            (SnapshotProjector<BorrowedWindow, int[]>)
                clause.Projectors[2];

        var snapshot = projector(in window);
        source[0] = 21;

        Assert.AreEqual(3, clause.Projectors.Count);
        CollectionAssert.AreEqual(
            new[] { 3, 8, 13 },
            snapshot);
        Assert.IsFalse(snapshot.GetType().IsByRefLike);
    }

    /// <summary>A synchronous borrowed callback can copy input and return asynchronous work safely.</summary>
    [TestMethod]
    public async Task TaskReturningCallback_CopiesBeforeSuspension()
    {
        var clause =
            new RefSafeProofSetupClause<Task<int>>();
        MockingRefSafeCompilerProof.TaskReturning(clause);
        static Task<int> callback(ReadOnlySpan<byte> bytes) =>
                CountAsync(bytes.ToArray());
        byte[] bytes = [1, 2, 3, 4];

        var pending = callback(bytes);
        bytes[0] = 9;
        var count = await pending;

        Assert.AreEqual(4, count);
        Assert.AreEqual(
            RefSafeCallbackKind.Func,
            clause.Kind);
    }

    /// <summary>A synchronous borrowed callback can safely return a ValueTask created from copied input.</summary>
    [TestMethod]
    public async Task ValueTaskReturningCallback_CopiesBeforeSuspension()
    {
        var clause =
            new RefSafeProofSetupClause<ValueTask<int>>();
        MockingRefSafeCompilerProof.ValueTaskReturning(
            clause);
        static ValueTask<int> callback(ReadOnlySpan<byte> bytes) =>
                CountValueTaskAsync(bytes.ToArray());
        byte[] bytes = [2, 4, 6];

        var pending = callback(bytes);
        bytes[0] = 8;
        var count = await pending;

        Assert.AreEqual(3, count);
        Assert.AreEqual(
            RefSafeCallbackKind.Func,
            clause.Kind);
    }

    /// <summary>The callback contract rejects async-void delegates before they can be stored.</summary>
    [TestMethod]
    public void AsyncVoidCallback_IsRejected()
    {
        var clause = new RefSafeProofSetupClause();
        static async void callback(int _) => await Task.Yield();

        var exception =
            Assert.Throws<ArgumentException>(
                () => clause.Do((Action<int>)callback));

        StringAssert.Contains(
            exception.Message,
            "Async-void");
        Assert.IsNull(clause.Callback);
    }

    /// <summary>Matcher placeholders are default or null and use no static generic value storage.</summary>
    [TestMethod]
    public void MatcherPlaceholders_AreDefaultNullAndStorageFree()
    {
        var predicateCalls = 0;
        var value = RefSafeProofArg.Match<ReadOnlySpan<int>>(
            0,
            _ =>
            {
                predicateCalls++;
                return true;
            });
        ref var any =
            ref RefSafeProofArg.AnyRef<Span<int>>(0);
        ref var matched =
            ref RefSafeProofArg.Match<Span<int>>(
                0,
                (
                    scoped in values) =>
                    values.Length != 0);

        Assert.IsTrue(value.IsEmpty);
        Assert.AreEqual(0, predicateCalls);
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe
                .IsNullRef(ref any));
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe
                .IsNullRef(ref matched));
        Assert.IsFalse(
            typeof(RefSafeProofArg)
                .GetFields(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .Any());
    }

    private static async Task<int> CountAsync(
        byte[] bytes)
    {
        await Task.Yield();
        return bytes.Length;
    }

    private static async ValueTask<int> CountValueTaskAsync(
        byte[] bytes)
    {
        await Task.Yield();
        return bytes.Length;
    }

    private static MethodInfo CreateInvokeVariant(
        string variant)
    {
        ParameterInfo[] expected =
            TransformExactMethod.GetParameters();
        Type returnType = TransformExactMethod.ReturnType;
        Type[] parameterTypes =
            [.. expected.Select(static parameter => parameter.ParameterType)];
        Type[][] required =
            [.. expected.Select(static parameter => parameter.GetRequiredCustomModifiers())];
        Type[][] optional =
            [.. expected.Select(static parameter => parameter.GetOptionalCustomModifiers())];
        ParameterAttributes[] attributes =
            [.. expected.Select(static parameter => parameter.Attributes)];
        bool[] copyScoped =
            [.. expected.Select(HasScopedRef)];

        switch (variant)
        {
            case "return":
                returnType = typeof(int);
                break;
            case "count":
                parameterTypes = parameterTypes[..^1];
                required = required[..^1];
                optional = optional[..^1];
                attributes = attributes[..^1];
                copyScoped = copyScoped[..^1];
                break;
            case "type":
                parameterTypes[0] =
                    typeof(ReadOnlySpan<byte>).MakeByRefType();
                break;
            case "byref":
                parameterTypes[0] = typeof(ReadOnlySpan<int>);
                break;
            case "in":
                attributes[0] &= ~ParameterAttributes.In;
                break;
            case "out":
                attributes[2] &= ~ParameterAttributes.Out;
                break;
            case "required":
                required[0] =
                    [.. required[0], typeof(System.Runtime.CompilerServices.IsExternalInit)];
                break;
            case "optional":
                optional[0] =
                    [.. optional[0], typeof(ObsoleteAttribute)];
                break;
            case "scoped":
                copyScoped[0] = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(variant),
                    variant,
                    "Unknown signature variant.");
        }

        var assembly =
            AssemblyBuilder.DefineDynamicAssembly(
                new($"RefSafeSignature_{Guid.NewGuid():N}"),
                AssemblyBuilderAccess.RunAndCollect);
        ModuleBuilder module =
            assembly.DefineDynamicModule("Signatures");
        TypeBuilder type = module.DefineType(
            $"Invoke_{variant}",
            TypeAttributes.Public |
            TypeAttributes.Abstract |
            TypeAttributes.Class);
        MethodBuilder method = type.DefineMethod(
            nameof(Action.Invoke),
            MethodAttributes.Public |
            MethodAttributes.Abstract |
            MethodAttributes.Virtual |
            MethodAttributes.NewSlot);
        method.SetSignature(
            returnType,
            TransformExactMethod.ReturnParameter
                .GetRequiredCustomModifiers(),
            TransformExactMethod.ReturnParameter
                .GetOptionalCustomModifiers(),
            parameterTypes,
            required,
            optional);

        for (int index = 0; index < parameterTypes.Length; index++)
        {
            ParameterBuilder parameter = method.DefineParameter(
                index + 1,
                attributes[index],
                expected[index].Name);
            if (copyScoped[index])
                CopyScoped(expected[index], parameter);
        }

        return type.CreateType()!.GetMethod(
            nameof(Action.Invoke))!;
    }

    private static bool HasScopedRef(ParameterInfo parameter) =>
        parameter.GetCustomAttributesData().Any(
            static attribute =>
                attribute.AttributeType.FullName ==
                "System.Runtime.CompilerServices.ScopedRefAttribute");

    private static void CopyScoped(
        ParameterInfo source,
        ParameterBuilder destination)
    {
        CustomAttributeData attribute =
            source.GetCustomAttributesData().Single(
                static candidate =>
                    candidate.AttributeType.FullName ==
                    "System.Runtime.CompilerServices.ScopedRefAttribute");
        destination.SetCustomAttribute(
            new(attribute.Constructor, []));
    }
}
