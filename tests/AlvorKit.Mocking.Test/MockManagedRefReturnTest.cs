namespace AlvorKit;

[TestClass]
public sealed class MockManagedRefReturnTest
{
    /// <summary>A mutable interface setup returns the same user-owned alias and observes caller mutation.</summary>
    [TestMethod]
    public void MutableInterface_ReturnsRepeatedUserOwnedAlias()
    {
        var target = Mock.Create<IManagedRefTarget>();
        var owner = new ManagedRefOwner(13);
        Mock.WhenRef(target.Mutable).ReturnRef(owner.Mutable);

        ref int first = ref target.Mutable();
        ref int second = ref target.Mutable();
        first = 21;

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref first,
                ref second));
        Assert.AreEqual(21, owner.Value);
        Assert.AreEqual(21, second);
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(2, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Source ==
            MockInvocationExecutionSource.Configured
            && invocation.Completion.Return!.Kind ==
            MockInvocationReturnKind.Unavailable
            && invocation.Completion.Return.UnavailableReason ==
            MockUnavailableReason.BorrowedReturnNotRetained));
    }

    /// <summary>A readonly setup preserves alias identity while the mocked member remains readonly to callers.</summary>
    [TestMethod]
    public void ReadonlyInterface_ReturnsRepeatedUserOwnedAlias()
    {
        var target = Mock.Create<IManagedRefTarget>();
        var owner = new ManagedRefOwner(34);
        Mock.WhenRefReadonly(target.ReadOnly).ReturnRef(owner.ReadOnly);

        ref readonly int first = ref target.ReadOnly();
        ref readonly int second = ref target.ReadOnly();

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref System.Runtime.CompilerServices.Unsafe.AsRef(in first),
                ref System.Runtime.CompilerServices.Unsafe.AsRef(in second)));
        Assert.AreEqual(34, first);
    }

    /// <summary>Value-based mutable setup owns one stable cell whose edits persist across calls.</summary>
    [TestMethod]
    public void ValueOverload_OwnsStableMutableCell()
    {
        var target = Mock.Create<IManagedRefTarget>();
        Mock.WhenRef(target.Mutable).ReturnRef(55);

        ref int first = ref target.Mutable();
        first = 89;
        ref int second = ref target.Mutable();

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref first,
                ref second));
        Assert.AreEqual(89, second);
    }

    /// <summary>Loose fallback owns one stable per-mock cell without executing an original implementation.</summary>
    [TestMethod]
    public void LooseFallback_ReturnsStablePerMockCell()
    {
        var firstTarget = Mock.CreateLoose<ManagedRefVirtualTarget>();
        var secondTarget = Mock.CreateLoose<ManagedRefVirtualTarget>();

        ref int first = ref firstTarget.Mutable();
        first = 144;
        ref int repeated = ref firstTarget.Mutable();
        ref int other = ref secondTarget.Mutable();

        Assert.AreEqual(144, repeated);
        Assert.AreEqual(0, other);
        Assert.AreEqual(0, firstTarget.Calls);
        Assert.AreEqual(0, secondTarget.Calls);
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref first,
                ref repeated));
        Assert.IsFalse(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref first,
                ref other));
    }

    /// <summary>Strict fallback throws without exposing a placeholder reference.</summary>
    [TestMethod]
    public void StrictFallback_ThrowsBeforeOriginalImplementation()
    {
        var target = Mock.Create<ManagedRefVirtualTarget>();

        Assert.Throws<MockException>(() => target.Mutable());
        Assert.AreEqual(0, target.Calls);
    }

    /// <summary>Virtual proxy dispatch publishes the configured alias without running its body.</summary>
    [TestMethod]
    public void VirtualClassBackend_ReturnsConfiguredAlias()
    {
        var virtualTarget = Mock.Create<ManagedRefVirtualTarget>();
        var owner = new ManagedRefOwner(233);
        Mock.WhenRef(virtualTarget.Mutable).ReturnRef(owner.Mutable);

        ref int virtualResult = ref virtualTarget.Mutable();

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref owner.Mutable(),
                ref virtualResult));
        Assert.AreEqual(0, virtualTarget.Calls);
    }

    /// <summary>Array elements and object fields remain exact stable user-owned backing locations.</summary>
    [TestMethod]
    public void UserOwnedFactories_PreserveArrayAndFieldBacking()
    {
        var target = Mock.Create<IManagedRefTarget>();
        int[] values = [3, 5, 8];
        var owner = new ManagedRefOwner(610);
        Mock.WhenRef(target.Mutable).ReturnRef(() => ref values[1]);
        Mock.WhenRefReadonly(target.ReadOnly).ReturnRef(owner.ReadOnly);

        ref int mutable = ref target.Mutable();
        ref readonly int readOnly = ref target.ReadOnly();
        mutable = 13;
        owner.Value = 987;

        Assert.AreEqual(13, values[1]);
        Assert.AreEqual(987, readOnly);
    }

    /// <summary>A ref-result factory throw preserves exception identity and completes the invocation once at the factory stage.</summary>
    [TestMethod]
    public void FactoryThrow_RecordsExactConfiguredFailure()
    {
        var target = Mock.Create<IManagedRefTarget>();
        var expected = new InvalidOperationException("managed ref");
        var calls = 0;
        Mock.WhenRef(target.Mutable).ReturnRef(Throw);

        Exception actual = Assert.Throws<InvalidOperationException>(
            () => target.Mutable());
        MockInvocation invocation =
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations[0];

        Assert.AreSame(expected, actual);
        Assert.AreEqual(MockInvocationCompletionKind.Threw, invocation.Completion.Kind);
        Assert.AreEqual(MockInvocationExecutionSource.Configured, invocation.Completion.Source);
        Assert.AreEqual(MockInvocationFailureStage.ReturnFactory, invocation.Completion.FailureStage);
        Assert.AreSame(expected, invocation.Completion.Exception);
        Assert.AreEqual(1, calls);
        return;

        ref int Throw()
        {
            calls++;
            throw expected;
        }
    }

    /// <summary>A readonly virtual member preserves its configured alias.</summary>
    [TestMethod]
    public void ReadonlyVirtualClass_PreservesConfiguredAlias()
    {
        var virtualTarget = Mock.Create<ManagedRefVirtualTarget>();
        var owner = new ManagedRefOwner(1597);
        Mock.WhenRefReadonly(virtualTarget.ReadOnly).ReturnRef(owner.ReadOnly);

        ref readonly int virtualResult = ref virtualTarget.ReadOnly();

        Assert.AreEqual(1597, virtualResult);
        Assert.AreEqual(0, virtualTarget.ReadOnlyCalls);
    }

    /// <summary>A closed generic proxy type preserves exact mutable and readonly element aliases.</summary>
    [TestMethod]
    public void ClosedGenericProxy_ReturnsExactAliases()
    {
        var target = Mock.Create<IManagedRefGenericTarget<string>>();
        var mutable = new ManagedRefObjectOwner("mutable");
        var readOnly = new ManagedRefObjectOwner("readonly");
        Mock.WhenRef(target.Mutable).ReturnRef(mutable.Mutable);
        Mock.WhenRefReadonly(target.ReadOnly).ReturnRef(readOnly.ReadOnly);

        ref string mutableResult = ref target.Mutable();
        ref readonly string readOnlyResult = ref target.ReadOnly();
        mutableResult = "changed";

        Assert.AreEqual("changed", mutable.Value);
        Assert.AreEqual("readonly", readOnlyResult);
    }

    /// <summary>Newest exact and wildcard matcher setups select their own stable aliases.</summary>
    [TestMethod]
    public void MatcherSelection_ReturnsSelectedSetupAlias()
    {
        var target = Mock.Create<IIndexedManagedRefTarget>();
        var wildcard = new ManagedRefOwner(2584);
        var exact = new ManagedRefOwner(4181);
        Mock.WhenRef(() => ref target.At(Arg.Any<int>()))
            .ReturnRef(wildcard.Mutable);
        Mock.WhenRef(() => ref target.At(5))
            .ReturnRef(exact.Mutable);

        ref int exactResult = ref target.At(5);
        ref int wildcardResult = ref target.At(6);

        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref exactResult,
                ref exact.Mutable()));
        Assert.IsTrue(
            System.Runtime.CompilerServices.Unsafe.AreSame(
                ref wildcardResult,
                ref wildcard.Mutable()));
    }

    /// <summary>A ref factory may reenter the same mock without corrupting either invocation token.</summary>
    [TestMethod]
    public void FactoryReentry_CompletesOuterAndInnerInvocationsOnce()
    {
        var target = Mock.Create<IReentrantManagedRefTarget>();
        var owner = new ReentrantManagedRefOwner(target);
        Mock.When(target.Value).Return(6765);
        Mock.WhenRef(target.Mutable).ReturnRef(owner.Mutable);

        ref int result = ref target.Mutable();
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];

        Assert.AreEqual(6765, result);
        Assert.AreEqual(6765, owner.Observed);
        Assert.AreEqual(1, owner.Calls);
        Assert.AreEqual(2, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Kind ==
            MockInvocationCompletionKind.Returned));
    }

    /// <summary>Concurrent calls publish the same stable alias and complete every ledger reservation.</summary>
    [TestMethod]
    public void ConcurrentCalls_ReturnOneAliasAndCompleteLedger()
    {
        const int Count = 128;
        var target = Mock.Create<IManagedRefTarget>();
        var owner = new ManagedRefOwner(10946);
        Mock.WhenRef(target.Mutable).ReturnRef(owner.Mutable);
        var mismatch = 0;

        Parallel.For(
            0,
            Count,
            _ =>
            {
                ref int actual = ref target.Mutable();
                ref int expected = ref owner.Mutable();
                if (!System.Runtime.CompilerServices.Unsafe.AreSame(
                    ref actual,
                    ref expected))
                {
                    Interlocked.Exchange(ref mismatch, 1);
                }
            });

        Assert.AreEqual(0, mismatch);
        MockInvocation[] invocations = [..
            Mock.GetMocked(target)!.Invocations.Snapshot().Invocations];
        Assert.AreEqual(Count, invocations.Length);
        Assert.IsTrue(invocations.All(invocation =>
            invocation.Completion.Kind ==
            MockInvocationCompletionKind.Returned));
    }

    /// <summary>Generated prefix metadata uses the exact alias factory while finalizer IL never dereferences it.</summary>
    [TestMethod]
    public void GeneratedAbi_UsesExactAliasFactoryWithoutByrefBoxing()
    {
        var target = Mock.Create<IManagedRefTarget>();
        MethodInfo mutable = target.GetType().GetMethod(
            nameof(IManagedRefTarget.Mutable))!;
        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(
                mutable,
                new(MockBackendKind.Proxy, 2));
        ParameterInfo resultRef = artifact.Prefix.GetParameters()[2];
        Type expected =
            typeof(MockManagedReferenceFactory<int>).MakeByRefType();

        Assert.AreEqual("__resultRef", resultRef.Name);
        Assert.AreEqual(expected, resultRef.ParameterType);
        Assert.AreEqual(0, resultRef.GetRequiredCustomModifiers().Length);
        Assert.AreEqual(0, resultRef.GetOptionalCustomModifiers().Length);
        Assert.IsFalse(ContainsOpcode(artifact.Finalizer, OpCodes.Box));
        Assert.IsFalse(ContainsOpcode(artifact.Finalizer, OpCodes.Unbox_Any));
        Assert.IsFalse(ContainsOpcode(artifact.Finalizer, OpCodes.Ldobj));
        OpCode[] indirectLoads =
        [
            OpCodes.Ldind_I,
            OpCodes.Ldind_I1,
            OpCodes.Ldind_I2,
            OpCodes.Ldind_I4,
            OpCodes.Ldind_I8,
            OpCodes.Ldind_R4,
            OpCodes.Ldind_R8,
            OpCodes.Ldind_Ref,
            OpCodes.Ldind_U1,
            OpCodes.Ldind_U2,
            OpCodes.Ldind_U4,
        ];
        Assert.IsFalse(indirectLoads.Any(opcode =>
            ContainsOpcode(artifact.Finalizer, opcode)));
        Assert.IsFalse(
            artifact.Prefix.DeclaringType!.GetFields(
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic)
            .Any(field =>
                typeof(Delegate).IsAssignableFrom(field.FieldType)));
    }

    /// <summary>Proxy overrides preserve readonly return metadata while transport modifiers stay off the alias factory.</summary>
    [TestMethod]
    public void ProxyMetadata_PreservesReadonlyReturnModifiers()
    {
        var target = Mock.Create<IManagedRefTarget>();
        MethodInfo source = typeof(IManagedRefTarget).GetMethod(
            nameof(IManagedRefTarget.ReadOnly))!;
        MethodInfo proxy = target.GetType().GetMethod(
            nameof(IManagedRefTarget.ReadOnly))!;

        Assert.AreEqual(
            MockCanonicalSignature.Create(source).Return.Kind,
            MockCanonicalSignature.Create(proxy).Return.Kind);
        Assert.AreEqual(
            MockReturnKind.ReadOnlyManagedReference,
            MockCanonicalSignature.Create(proxy).Return.Kind);

        MockTypedTrampolineArtifact artifact =
            MockTypedTrampolineCache.GetOrCreate(
                proxy,
                new(MockBackendKind.Proxy, 2));
        ParameterInfo resultRef = artifact.Prefix.GetParameters()[2];
        Assert.AreEqual(0, resultRef.GetRequiredCustomModifiers().Length);
        Assert.AreEqual(0, resultRef.GetOptionalCustomModifiers().Length);
    }

    /// <summary>Invalid readonly, pointer, ref-struct, and open generic setup shapes reject before publication.</summary>
    [TestMethod]
    public unsafe void InvalidFactories_RejectBeforeSetupPublication()
    {
        var target = Mock.Create<IManagedRefTarget>();
        Mocked mocked = Mock.GetMocked(target)!;
        MethodInfo mutable = typeof(IManagedRefTarget).GetMethod(
            nameof(IManagedRefTarget.Mutable))!;
        var owner = new ManagedRefOwner(17711);

        MockException modifierError = Assert.Throws<MockException>(
            () => mocked.AddRefReadonlyReturnFactory(
                mutable,
                [],
                owner.ReadOnly));
        StringAssert.Contains(modifierError.Message, "return is mutable");

        MethodInfo pointer = typeof(ManagedRefUnsupportedTarget).GetMethod(
            nameof(ManagedRefUnsupportedTarget.Pointer))!;
        MockException pointerError = Assert.Throws<MockException>(
            () => mocked.AddRefReturnFactory(
                pointer,
                [],
                owner.Mutable));
        StringAssert.Contains(pointerError.Message, "pointer-shaped");

        MethodInfo byRefLike = typeof(ManagedRefUnsupportedTarget).GetMethod(
            nameof(ManagedRefUnsupportedTarget.RefStruct))!;
        MockException byRefLikeError = Assert.Throws<MockException>(
            () => mocked.AddRefReturnFactory(
                byRefLike,
                [],
                owner.Mutable));
        StringAssert.Contains(byRefLikeError.Message, "ref struct");

        Assert.AreEqual(0, mocked.SnapshotSetups().Length);
        Assert.Throws<MockException>(
            () => Mock.Create<IManagedRefGenericMethodTarget>());
    }

    /// <summary>Mock, setup, bridge, and user owner remain collectible as one unreachable cycle.</summary>
    [TestMethod]
    public void RefFactoryState_DoesNotEscapeMockLifetime()
    {
        WeakReference[] references = CreateCollectibleRefSetup();

        ForceCollection();

        Assert.IsFalse(references[0].IsAlive, "mock stayed alive");
        Assert.IsFalse(references[1].IsAlive, "owner stayed alive");
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference[] CreateCollectibleRefSetup()
    {
        var target = Mock.Create<IManagedRefTarget>();
        var owner = new ManagedRefOwner(28657);
        Mock.WhenRef(target.Mutable).ReturnRef(owner.Mutable);
        _ = target.Mutable();
        return [new(target), new(owner)];
    }

    [System.Runtime.CompilerServices.MethodImpl(
        System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void ForceCollection()
    {
        for (var attempt = 0; attempt < 8; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private static bool ContainsOpcode(
        MethodInfo method,
        OpCode expected)
    {
        byte[] bytes = method.GetMethodBody()!.GetILAsByteArray()!;
        for (var index = 0; index < bytes.Length;)
        {
            OpCode opcode = ReadOpcode(bytes, ref index);
            if (opcode == expected)
                return true;
            index += OperandSize(opcode, bytes, index);
        }

        return false;
    }

    private static OpCode ReadOpcode(
        byte[] bytes,
        ref int index)
    {
        ushort value = bytes[index++];
        if (value == 0xfe)
            value = (ushort)(0xfe00 | bytes[index++]);

        foreach (FieldInfo field in typeof(OpCodes).GetFields(
            BindingFlags.Public | BindingFlags.Static))
        {
            var opcode = (OpCode)field.GetValue(null)!;
            if ((ushort)opcode.Value == value)
                return opcode;
        }

        throw new InvalidOperationException($"Unknown IL opcode 0x{value:x4}.");
    }

    private static int OperandSize(
        OpCode opcode,
        byte[] bytes,
        int index) =>
        opcode.OperandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
            OperandType.ShortInlineI or
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineI or
            OperandType.InlineBrTarget or
            OperandType.InlineField or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or
            OperandType.InlineR => 8,
            OperandType.InlineSwitch =>
                4 + BitConverter.ToInt32(bytes, index) * 4,
            _ => throw new InvalidOperationException(
                $"Unknown operand type {opcode.OperandType}.")
        };
}

internal interface IManagedRefTarget
{
    ref int Mutable();

    ref readonly int ReadOnly();
}

internal interface IManagedRefGenericTarget<T>
{
    ref T Mutable();

    ref readonly T ReadOnly();
}

internal interface IIndexedManagedRefTarget
{
    ref int At(int index);
}

internal interface IReentrantManagedRefTarget
{
    int Value();

    ref int Mutable();
}

internal interface IManagedRefGenericMethodTarget
{
    ref T Mutable<T>();
}

internal sealed class ManagedRefOwner(int value)
{
    private int value = value;

    internal int Value
    {
        get => value;
        set => this.value = value;
    }

    internal ref int Mutable() => ref value;

    internal ref readonly int ReadOnly() => ref value;
}

internal sealed class ManagedRefObjectOwner(string value)
{
    private string value = value;

    internal string Value => value;

    internal ref string Mutable() => ref value;

    internal ref readonly string ReadOnly() => ref value;
}

internal sealed class ReentrantManagedRefOwner(
    IReentrantManagedRefTarget target)
{
    private int value;

    internal int Observed { get; private set; }
    internal int Calls { get; private set; }

    internal ref int Mutable()
    {
        Calls++;
        Observed = target.Value();
        value = Observed;
        return ref value;
    }
}

internal class ManagedRefVirtualTarget
{
    private int value = 1;
    private readonly int readOnly = 2;

    internal int Calls;
    internal int ReadOnlyCalls;

    public virtual ref int Mutable()
    {
        Calls++;
        return ref value;
    }

    public virtual ref readonly int ReadOnly()
    {
        ReadOnlyCalls++;
        return ref readOnly;
    }
}

internal sealed class ManagedRefSealedTarget
{
    private int value = 1;
    private readonly int readOnly = 2;

    internal int Calls;
    internal int ReadOnlyCalls;

    public ref int Mutable()
    {
        Calls++;
        return ref value;
    }

    public ref readonly int ReadOnly()
    {
        ReadOnlyCalls++;
        return ref readOnly;
    }
}

internal sealed class ManagedRefPartialTarget
{
    private int configured = 1;
    private int neighbor = 2;
    private readonly int configuredReadOnly = 3;
    private readonly int neighborReadOnly = 4;

    internal int ConfiguredCalls;
    internal int NeighborCalls;
    internal int ConfiguredReadOnlyCalls;
    internal int NeighborReadOnlyCalls;

    public ref int Configured()
    {
        ConfiguredCalls++;
        return ref configured;
    }

    public ref int Neighbor()
    {
        NeighborCalls++;
        return ref neighbor;
    }

    public ref readonly int ConfiguredReadOnly()
    {
        ConfiguredReadOnlyCalls++;
        return ref configuredReadOnly;
    }

    public ref readonly int NeighborReadOnly()
    {
        NeighborReadOnlyCalls++;
        return ref neighborReadOnly;
    }
}

internal sealed unsafe class ManagedRefUnsupportedTarget
{
    private int* pointer;

    public ref int* Pointer() => ref pointer;

    public ref Span<int> RefStruct() =>
        throw new NotImplementedException();
}

internal sealed class ManagedRefSealedGenericMethodTarget
{
    public ref T Mutable<T>() =>
        throw new NotImplementedException();
}
