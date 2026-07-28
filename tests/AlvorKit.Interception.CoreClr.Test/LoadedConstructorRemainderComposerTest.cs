using System.Buffers.Binary;
using System.Reflection;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Verifies executable constructor-remainder extraction and ABI-v3 composition.</summary>
[TestClass]
public sealed class LoadedConstructorRemainderComposerTest
{
    /// <summary>Extracts the exact suffix and emits a post-initializer route generation.</summary>
    [TestMethod]
    public void Compose_PreservesInitializerAndExtractsOriginalRemainder()
    {
        ConstructorInfo constructor = Constructor(typeof(DirectTarget));
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        var planning = LoadedConstructorRemainderPlanner.Plan(
            body,
            new ReflectionLoadedConstructorMetadataResolver(constructor));
        Assert.IsTrue(
            planning.IsSuccessful,
            string.Join(
                Environment.NewLine,
                planning.Rejections.Select(rejection =>
                    rejection.Detail)));
        LoadedConstructorRemainderPlan plan = planning.Plan!;

        var artifact = LoadedConstructorRemainderComposer.Compose(
            constructor,
            body,
            plan,
            Route(nameof(RouteDirect)),
            typeof(DirectRemainder),
            41);

        Assert.AreEqual(
            constructor.MetadataToken,
            artifact.Plan.Target.MethodToken);
        Assert.AreSame(body.Identity, artifact.Plan.BaselineBodyIdentity);
        Assert.AreEqual(41ul, artifact.Plan.GenerationId);
        Assert.AreEqual(0ul, artifact.Plan.PriorGenerationId);
        Assert.IsTrue(artifact.Plan.Relocations.IsEmpty);
        Assert.IsFalse(artifact.Plan.IlMap[^1].Accurate);

        var generated = LoadedMethodBodyDecoder.Decode(
            artifact.Plan.MethodBody.Bytes.Span);
        string[] generatedNames =
        [
            .. generated.Instructions
                .Select(instruction => instruction.OpCode.Name!)
        ];
        CollectionAssert.AreEqual(
            (string[])
            [
                .. plan.PreservedPrefix.Instructions
                    .Select(instruction => instruction.OpCode.Name!)
            ],
            generatedNames[
                ..plan.PreservedPrefix.Instructions.Length]);
        CollectionAssert.AreEqual(
            new[] { "ldarg.0", "ldarg.1", "call", "ret" },
            generatedNames[^4..]);
        Assert.AreEqual(
            typeof(DirectTarget).BaseType!
                .GetConstructor(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic,
                    null,
                    [typeof(int)],
                    null)!
                .MetadataToken,
            generated.Instructions.Single(instruction =>
                instruction.BaselineOffset ==
                plan.InitializerCallOffset)
                .Operand.IntegerValue);
        Assert.AreEqual(
            Route(nameof(RouteDirect)).MetadataToken,
            generated.Instructions[^2].Operand.IntegerValue);

        DirectTarget.Reset();
        var target = new DirectTarget(3);
        DirectTarget.ResetBody();
        ((DirectRemainder)artifact.OriginalRemainder)(target, 7);
        Assert.AreEqual(7, target.Value);
        Assert.AreEqual(1, DirectTarget.BodyCalls);
        CollectionAssert.AreEqual(
            new[] { "body:7" },
            DirectTarget.Events.ToArray());
    }

    /// <summary>Relocates branches, metadata, locals, and a complete remainder exception region.</summary>
    [TestMethod]
    public void Compose_RelocatesRemainderControlFlowAndExceptionRegion()
    {
        ConstructorInfo constructor = Constructor(typeof(ExceptionTarget));
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        var planning = LoadedConstructorRemainderPlanner.Plan(
            body,
            new ReflectionLoadedConstructorMetadataResolver(constructor));
        Assert.IsTrue(
            planning.IsSuccessful,
            string.Join(
                Environment.NewLine,
                planning.Rejections.Select(rejection =>
                    rejection.Detail)));
        Assert.IsFalse(planning.Plan!.MovedExceptionRegions.IsEmpty);

        var artifact = LoadedConstructorRemainderComposer.Compose(
            constructor,
            body,
            planning.Plan,
            Route(nameof(RouteException)),
            typeof(ExceptionRemainder),
            42,
            41);

        var target = new ExceptionTarget(1);
        ((ExceptionRemainder)artifact.OriginalRemainder)(target, -9);
        Assert.AreEqual(9, target.Value);
        Assert.AreEqual(2, target.FinallyCalls);
        ((ExceptionRemainder)artifact.OriginalRemainder)(target, 5);
        Assert.AreEqual(5, target.Value);
        Assert.AreEqual(3, target.FinallyCalls);
        Assert.AreEqual(42ul, artifact.Plan.GenerationId);
        Assert.AreEqual(41ul, artifact.Plan.PriorGenerationId);
    }

    /// <summary>Generic route MethodSpecs remain unsupported in constructor generations.</summary>
    [TestMethod]
    public void ComposeRejectsGenericRoutes()
    {
        ConstructorInfo constructor = Constructor(typeof(DirectTarget));
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        LoadedConstructorRemainderPlan plan =
            LoadedConstructorRemainderPlanner.Plan(
                body,
                new ReflectionLoadedConstructorMetadataResolver(constructor))
            .Plan!;
        MethodInfo route = Route(nameof(RouteGeneric))
            .MakeGenericMethod(typeof(int));

        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructorRemainderComposer.Compose(
                constructor,
                body,
                plan,
                route,
                typeof(DirectRemainder),
                1));
        MethodInfo genericOwnerRoute = typeof(GenericRouteOwner<int>)
            .GetMethod(
                nameof(GenericRouteOwner<>.Route),
                BindingFlags.NonPublic | BindingFlags.Static)!;
        Assert.ThrowsExactly<ArgumentException>(() =>
            LoadedConstructorRemainderComposer.Compose(
                constructor,
                body,
                plan,
                genericOwnerRoute,
                typeof(DirectRemainder),
                1));
    }

    /// <summary>Dynamic call-site signatures fail closed until they can be relocated.</summary>
    [TestMethod]
    public void ComposeRejectsInlineSignatureInRemainder()
    {
        ConstructorInfo constructor = Constructor(typeof(InlineSigTarget));
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        LoadedConstructorRemainderPlan plan =
            LoadedConstructorRemainderPlanner.Plan(
                body,
                new ReflectionLoadedConstructorMetadataResolver(constructor))
            .Plan!;

        NotSupportedException exception =
            Assert.ThrowsExactly<NotSupportedException>(() =>
                LoadedConstructorRemainderComposer.Compose(
                    constructor,
                    body,
                    plan,
                    Route(nameof(RouteInlineSig)),
                    typeof(InlineSigRemainder),
                    1));
        StringAssert.Contains(exception.Message, "InlineSig");
    }

    /// <summary>TypeSpec-bearing local signatures fail closed in dynamic scope.</summary>
    [TestMethod]
    public void ComposeRejectsTypeSpecLocalSignature()
    {
        ConstructorInfo constructor = Constructor(typeof(GenericLocalTarget));
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        LoadedConstructorRemainderPlan plan =
            LoadedConstructorRemainderPlanner.Plan(
                body,
                new ReflectionLoadedConstructorMetadataResolver(constructor))
            .Plan!;

        NotSupportedException exception =
            Assert.ThrowsExactly<NotSupportedException>(() =>
                LoadedConstructorRemainderComposer.Compose(
                    constructor,
                    body,
                    plan,
                    Route(nameof(RouteGenericLocal)),
                    typeof(GenericLocalRemainder),
                    1));
        StringAssert.Contains(exception.Message, "TypeSpec");
    }

    /// <summary>An explicit empty dynamic local signature supports local-free remainders.</summary>
    [TestMethod]
    public void ComposeExecutesRemainderWithoutSourceLocalSignature()
    {
        ConstructorInfo constructor = Constructor(typeof(NoLocalsTarget));
        Assert.AreEqual(
            0,
            constructor.GetMethodBody()!.LocalSignatureMetadataToken);
        var body = LoadedMethodBodyDecoder.Decode(
            ReflectionBody(constructor));
        LoadedConstructorRemainderPlan plan =
            LoadedConstructorRemainderPlanner.Plan(
                body,
                new ReflectionLoadedConstructorMetadataResolver(constructor))
            .Plan!;

        LoadedConstructorRemainderGeneration artifact =
            LoadedConstructorRemainderComposer.Compose(
                constructor,
                body,
                plan,
                Route(nameof(RouteNoLocals)),
                typeof(NoLocalsRemainder),
                1);
        var target = new NoLocalsTarget(3);

        ((NoLocalsRemainder)artifact.OriginalRemainder)(target, 29);

        Assert.AreEqual(29, target.Value);
    }

    private static ConstructorInfo Constructor(Type type) =>
        type.GetConstructor(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic,
            null,
            [typeof(int)],
            null)!;

    private static MethodInfo Route(string name) =>
        typeof(LoadedConstructorRemainderComposerTest).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static byte[] ReflectionBody(ConstructorInfo constructor)
    {
        MethodBody body = constructor.GetMethodBody()!;
        byte[] il = body.GetILAsByteArray()!;
        var clauses = body.ExceptionHandlingClauses;
        var sectionSize = clauses.Count == 0
            ? 0
            : 4 + clauses.Count * 24;
        var sectionStart = clauses.Count == 0
            ? 12 + il.Length
            : (12 + il.Length + 3) & ~3;
        var bytes = new byte[sectionStart + sectionSize];
        ushort flags = (ushort)(
            0x0003 |
            (body.InitLocals ? 0x0010 : 0) |
            (clauses.Count == 0 ? 0 : 0x0008) |
            (3 << 12));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, flags);
        BinaryPrimitives.WriteUInt16LittleEndian(
            bytes.AsSpan(2),
            checked((ushort)body.MaxStackSize));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), il.Length);
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(8),
            body.LocalSignatureMetadataToken);
        il.CopyTo(bytes, 12);
        if (clauses.Count == 0)
            return bytes;

        Span<byte> section = bytes.AsSpan(sectionStart);
        section[0] = 0x41;
        section[1] = checked((byte)sectionSize);
        section[2] = checked((byte)(sectionSize >> 8));
        section[3] = checked((byte)(sectionSize >> 16));
        for (var index = 0; index < clauses.Count; ++index)
        {
            ExceptionHandlingClause clause = clauses[index];
            Span<byte> encoded = section.Slice(4 + index * 24, 24);
            BinaryPrimitives.WriteUInt32LittleEndian(
                encoded,
                (uint)clause.Flags);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[4..],
                clause.TryOffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[8..],
                clause.TryLength);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[12..],
                clause.HandlerOffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[16..],
                clause.HandlerLength);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[20..],
                clause.Flags == ExceptionHandlingClauseOptions.Filter
                    ? clause.FilterOffset
                    : clause.Flags ==
                        ExceptionHandlingClauseOptions.Clause
                        ? clause.CatchType!.MetadataToken
                        : 0);
        }

        return bytes;
    }

    private static void RouteDirect(DirectTarget target, int value)
    {
        _ = target;
        _ = value;
    }

    private static void RouteException(ExceptionTarget target, int value)
    {
        _ = target;
        _ = value;
    }

    private static void RouteGeneric<T>(DirectTarget target, int value)
    {
        _ = target;
        _ = value;
        _ = typeof(T);
    }

    private static void RouteInlineSig(InlineSigTarget target, int value)
    {
        _ = target;
        _ = value;
    }

    private static void RouteGenericLocal(
        GenericLocalTarget target,
        int value)
    {
        _ = target;
        _ = value;
    }

    private static void RouteNoLocals(NoLocalsTarget target, int value)
    {
        _ = target;
        _ = value;
    }

    private delegate void DirectRemainder(DirectTarget target, int value);

    private delegate void ExceptionRemainder(
        ExceptionTarget target,
        int value);

    private delegate void InlineSigRemainder(
        InlineSigTarget target,
        int value);

    private delegate void GenericLocalRemainder(
        GenericLocalTarget target,
        int value);

    private delegate void NoLocalsRemainder(
        NoLocalsTarget target,
        int value);

    private class DirectBase
    {
        protected DirectBase(int value)
        {
            BaseValue = value;
        }

        internal int BaseValue { get; }
    }

    private sealed class DirectTarget : DirectBase
    {
        internal static readonly List<string> Events = [];

        internal DirectTarget(int value)
            : base(value + 100)
        {
            BodyCalls++;
            Value = value;
            Events.Add($"body:{value}");
        }

        internal static int BodyCalls { get; private set; }

        internal int Value { get; private set; }

        internal static void Reset()
        {
            ResetBody();
        }

        internal static void ResetBody()
        {
            BodyCalls = 0;
            Events.Clear();
        }

    }

    private class ExceptionBase
    {
        protected ExceptionBase(int value)
        {
            _ = value;
        }
    }

    private sealed class ExceptionTarget : ExceptionBase
    {
        internal ExceptionTarget(int value)
            : base(value)
        {
            try
            {
                if (value < 0)
                    Value = -value;
                else
                    Value = value;
            }
            finally
            {
                FinallyCalls++;
            }
        }

        internal int FinallyCalls { get; private set; }

        internal int Value { get; private set; }
    }

    private sealed class InlineSigTarget : DirectBase
    {
        internal unsafe InlineSigTarget(int value)
            : base(value)
        {
            delegate* managed<int, int> transform = &Transform;
            Value = transform(value);
        }

        internal int Value { get; }

        private static int Transform(int value) => value + 1;
    }

    private sealed class GenericLocalTarget : DirectBase
    {
        internal GenericLocalTarget(int value)
            : base(value)
        {
            var values = new List<int> { value };
            Value = values[0];
        }

        internal int Value { get; }
    }

    private sealed class NoLocalsTarget : DirectBase
    {
        internal NoLocalsTarget(int value)
            : base(value)
        {
            Value = value;
        }

        internal int Value { get; private set; }
    }

    private static class GenericRouteOwner<T>
    {
        internal static void Route(DirectTarget target, int value)
        {
            _ = target;
            _ = value;
            _ = typeof(T);
        }
    }
}
