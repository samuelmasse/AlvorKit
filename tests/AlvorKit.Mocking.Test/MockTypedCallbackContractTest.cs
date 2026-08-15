namespace AlvorKit;

[TestClass]
public sealed class MockTypedCallbackContractTest
{
    /// <summary>Standard borrowed callbacks normalize to one reusable exact stable delegate.</summary>
    [TestMethod]
    public void StandardCallback_NormalizesToStableExactDelegate()
    {
        MethodInfo method = typeof(ITypedCallbackContractTarget).GetMethod(
            nameof(ITypedCallbackContractTarget.Observe))!;
        Action<ReadOnlySpan<int>> callback = _ => { };

        Delegate first =
            MockTypedCallbackContract.Normalize(callback, method);
        Delegate second =
            MockTypedCallbackContract.Normalize(callback, method);
        Type stable =
            MockTypedCallbackDelegateCache.GetOrCreate(method);

        Assert.AreEqual(stable, first.GetType());
        Assert.AreEqual(stable, second.GetType());
        Assert.AreSame(stable, second.GetType());
        MockTypedCallbackContract.ValidateInvoke(
            stable.GetMethod(nameof(Action.Invoke))!,
            method);
    }

    /// <summary>Natural in, ref, and out delegates preserve every exact callback signature facet.</summary>
    [TestMethod]
    public void NaturalCallback_NormalizesExactReferenceMetadata()
    {
        MethodInfo method = typeof(ITypedCallbackContractTarget).GetMethod(
            nameof(ITypedCallbackContractTarget.Exact))!;
        var callback =
            (
                scoped in ReadOnlySpan<int> source,
                scoped ref Span<int> destination,
                scoped out TypedCallbackWindow written) =>
            {
                source.CopyTo(destination);
                written = new(destination);
            };

        Delegate normalized =
            MockTypedCallbackContract.Normalize(callback, method);
        MethodInfo invoke = normalized.GetType().GetMethod(
            nameof(Action.Invoke))!;

        MockTypedCallbackContract.ValidateInvoke(invoke, method);
        Assert.AreEqual(typeof(void), invoke.ReturnType);
        Assert.IsTrue(invoke.GetParameters()[0].IsIn);
        Assert.IsTrue(invoke.GetParameters()[1].ParameterType.IsByRef);
        Assert.IsTrue(invoke.GetParameters()[2].IsOut);
    }

    /// <summary>Async-void and return-shape mismatches reject before a callback can be retained.</summary>
    [TestMethod]
    public void InvalidCallbacks_RejectDeterministically()
    {
        MethodInfo ordinary = typeof(ITypedCallbackContractTarget).GetMethod(
            nameof(ITypedCallbackContractTarget.Ordinary))!;
        MethodInfo answer = typeof(ITypedCallbackContractTarget).GetMethod(
            nameof(ITypedCallbackContractTarget.Answer))!;
        Action<int> asyncVoid =
            async _ => await Task.Yield();
        Action<ReadOnlySpan<int>> wrongReturn = _ => { };

        MockException asyncError = Assert.Throws<MockException>(
            () => MockTypedCallbackContract.Normalize(
                asyncVoid,
                ordinary));
        MockException returnError = Assert.Throws<MockException>(
            () => MockTypedCallbackContract.Normalize(
                wrongReturn,
                answer));

        StringAssert.Contains(asyncError.Message, "Async-void");
        StringAssert.Contains(returnError.Message, "return type");
    }

    /// <summary>The weak delegate cache stores generated types but no callback delegate instance.</summary>
    [TestMethod]
    public void StableDelegateCache_HasNoStaticCallbackStorage()
    {
        foreach (FieldInfo field in typeof(MockTypedCallbackDelegateCache)
            .GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic))
        {
            Assert.IsFalse(
                typeof(Delegate).IsAssignableFrom(field.FieldType),
                field.Name);
        }
    }

    /// <summary>Managed-reference and pointer-shaped callback returns reject before setup publication.</summary>
    [TestMethod]
    public void UnsupportedReturns_RejectBeforePublication()
    {
        var mocked = new Mocked(
            MockFallbackBehavior.Strict,
            new TypeCache(typeof(UnsupportedTypedCallbackReturnTarget)));
        Action callback = () => { };
        MethodInfo managedReference =
            GetUnsupportedReturn(nameof(
                UnsupportedTypedCallbackReturnTarget.ManagedReference));
        MethodInfo pointer =
            GetUnsupportedReturn(nameof(
                UnsupportedTypedCallbackReturnTarget.Pointer));
        MethodInfo functionPointer =
            GetUnsupportedReturn(nameof(
                UnsupportedTypedCallbackReturnTarget.FunctionPointer));

        MockException managedReferenceError =
            Assert.Throws<MockException>(
                () => mocked.AddTypedCallback(
                    managedReference,
                    [],
                    callback,
                    []));
        MockException pointerError =
            Assert.Throws<MockException>(
                () => mocked.AddTypedCallback(
                    pointer,
                    [],
                    callback,
                    []));
        MockException functionPointerError =
            Assert.Throws<MockException>(
                () => mocked.AddTypedCallback(
                    functionPointer,
                    [],
                    callback,
                    []));

        StringAssert.Contains(
            managedReferenceError.Message,
            "Mock.WhenRef");
        StringAssert.Contains(pointerError.Message, "Pointer-shaped");
        StringAssert.Contains(
            functionPointerError.Message,
            "Pointer-shaped");
        Assert.AreEqual(0, mocked.SnapshotSetups().Length);
    }

    /// <summary>Proxy generic callbacks map every standard Action and Func input arity.</summary>
    [TestMethod]
    public void StandardDelegateShape_CoversSupportedArities()
    {
        for (var count = 0; count <= 16; count++)
        {
            var parameters = new Type[count];
            Array.Fill(parameters, typeof(int));
            Type expectedAction = count == 0
                ? typeof(Action)
                : typeof(Action).Assembly.GetType(
                    $"System.Action`{count}")!
                    .MakeGenericType(parameters);
            Type expectedFunc = typeof(Func<>).Assembly.GetType(
                $"System.Func`{count + 1}")!
                .MakeGenericType([.. parameters, typeof(int)]);

            Assert.AreEqual(
                expectedAction,
                MockTypedCallbackDelegateShape.Create(
                    typeof(void),
                    parameters));
            Assert.AreEqual(
                expectedFunc,
                MockTypedCallbackDelegateShape.Create(
                    typeof(int),
                    parameters));
        }

        var tooWide = new Type[17];
        Array.Fill(tooWide, typeof(int));
        Assert.IsNull(
            MockTypedCallbackDelegateShape.Create(
                typeof(void),
                tooWide));
        Assert.IsNull(
            MockTypedCallbackDelegateShape.Create(
                typeof(int),
                tooWide));
        Assert.IsNull(
            MockTypedCallbackDelegateShape.Create(
                typeof(void),
                [typeof(int).MakeByRefType()]));
    }

    private static MethodInfo GetUnsupportedReturn(string name) =>
        typeof(UnsupportedTypedCallbackReturnTarget).GetMethod(
            name,
            BindingFlags.Static |
            BindingFlags.NonPublic)!;
}

internal interface ITypedCallbackContractTarget
{
    void Ordinary(int value);

    void Observe(ReadOnlySpan<int> values);

    void Exact(
        scoped in ReadOnlySpan<int> source,
        scoped ref Span<int> destination,
        scoped out TypedCallbackWindow written);

    int Answer(ReadOnlySpan<int> values);
}

internal readonly ref struct TypedCallbackWindow(
    ReadOnlySpan<int> values)
{
    internal ReadOnlySpan<int> Values { get; } = values;
}

internal static unsafe class UnsupportedTypedCallbackReturnTarget
{
    private static int value;

    internal static ref int ManagedReference() => ref value;

    internal static int* Pointer() => null;

    internal static delegate* managed<void> FunctionPointer() => null;
}
