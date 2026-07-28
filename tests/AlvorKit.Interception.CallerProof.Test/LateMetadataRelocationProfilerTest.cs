namespace AlvorKit.Interception.CallerProof.Test;

[TestClass]
public sealed class LateMetadataRelocationProfilerTest
{
    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(10);

    /// <summary>Creates and reuses executable late TypeSpec, MemberRef, and MethodSpec tokens.</summary>
    [TestMethod]
    public void GenerationRelocatesLateTypeMemberAndMethodTokensIdempotently()
    {
        RequireProfiledHost();
        var caller = Method(nameof(LateMetadataRelocationTarget.Caller));
        RuntimeHelpers.PrepareMethod(caller.MethodHandle);
        Assert.AreEqual(3, LateMetadataRelocationTarget.Caller());

        var privateValue = Method("PrivateValue");
        var internalTransform = InternalMethod();
        var privateIdentity = Method("PrivateIdentity");
        var typeSpec = GenericBoxOfInt32Signature();
        var privateMemberRef = caller.Module.ResolveSignature(
            privateValue.MetadataToken);
        Assert.IsTrue(
            privateMemberRef.Contains((byte)0x1F) ||
                privateMemberRef.Contains((byte)0x20),
            "The private ref-readonly member must carry its custom modifier.");
        byte[] internalMemberRef = [0x00, 0x01, 0x08, 0x08];
        byte[] methodSpec = [0x0A, 0x01, 0x08];
        var (TypeSpecs, MemberRefs, MethodSpecs) = ReadOriginalRowsAndAssertAbsent(
            caller.Module,
            typeSpec,
            privateValue,
            privateMemberRef,
            internalTransform,
            internalMemberRef,
            privateIdentity,
            methodSpec);
        var profiler = InterceptionProfiler.Connect();
        var baseline = profiler.GetLoadedMethodBody(
            InterceptionTarget.FromMethod(caller));
        var body = CreateBody(
            out var firstTypeOffset,
            out var secondTypeOffset,
            out var privateMemberOffset,
            out var internalMemberOffset,
            out var methodOffset);

        using var patch = profiler.Install(
            Plan(
                caller,
                baseline.Identity,
                body,
                firstTypeOffset,
                secondTypeOffset,
                privateMemberOffset,
                internalMemberOffset,
                methodOffset,
                typeSpec,
                privateValue,
                privateMemberRef,
                internalTransform,
                internalMemberRef,
                privateIdentity,
                methodSpec,
                101,
                0));
        var firstRequest = patch.LastRequestId;
        _ = WaitFor(profiler, firstRequest);
        AssertGeneration(profiler, firstRequest, 101, 0);
        Assert.AreEqual(12, LateMetadataRelocationTarget.Caller());
        var firstTokens = ReadTokens(profiler, firstRequest);
        Assert.AreEqual(firstTokens[0], firstTokens[1]);
        Assert.AreEqual(
            0x1B000000 | TypeSpecs + 1,
            firstTokens[0],
            "The absent generic TypeSpec must append exactly one metadata row.");
        Assert.AreEqual(
            0x0A000000 | MemberRefs + 1,
            firstTokens[2],
            "The absent private MemberRef must append exactly one metadata row.");
        Assert.AreEqual(
            0x0A000000 | MemberRefs + 2,
            firstTokens[3],
            "The absent internal MemberRef must append exactly one metadata row.");
        Assert.AreEqual(
            0x2B000000 | MethodSpecs + 1,
            firstTokens[4],
            "The absent MethodSpec must append exactly one metadata row.");
        AssertResolvedTokens(
            caller.Module,
            firstTokens,
            privateValue,
            internalTransform,
            privateIdentity);

        var replaceRequest = patch.Replace(
            Plan(
                caller,
                baseline.Identity,
                body,
                firstTypeOffset,
                secondTypeOffset,
                privateMemberOffset,
                internalMemberOffset,
                methodOffset,
                typeSpec,
                privateValue,
                privateMemberRef,
                internalTransform,
                internalMemberRef,
                privateIdentity,
                methodSpec,
                102,
                101));
        _ = WaitFor(profiler, replaceRequest);
        AssertGeneration(profiler, replaceRequest, 102, 101);
        CollectionAssert.AreEqual(
            firstTokens,
            ReadTokens(profiler, replaceRequest),
            "Every identical late relocation must reuse its metadata token.");
        Assert.AreEqual(12, LateMetadataRelocationTarget.Caller());

        var removeRequest = patch.Remove();
        _ = WaitFor(profiler, removeRequest);
        Assert.AreEqual(3, LateMetadataRelocationTarget.Caller());
    }

    private static InterceptionGenerationPlan Plan(
        MethodInfo caller,
        LoadedMethodBodyIdentity baselineIdentity,
        byte[] body,
        int firstTypeOffset,
        int secondTypeOffset,
        int privateMemberOffset,
        int internalMemberOffset,
        int methodOffset,
        byte[] typeSpec,
        MethodInfo privateValue,
        byte[] privateMemberRef,
        MethodInfo internalTransform,
        byte[] internalMemberRef,
        MethodInfo privateIdentity,
        byte[] methodSpec,
        ulong generation,
        ulong prior) =>
        new(
            InterceptionTarget.FromMethod(caller),
            InterceptionMethodBody.FromRaw(body),
            baselineIdentity,
            generation,
            prior,
            [
                new(
                    InterceptionGenerationRelocationKind.TypeSpec,
                    checked((uint)firstTypeOffset),
                    typeSpec),
                new(
                    InterceptionGenerationRelocationKind.TypeSpec,
                    checked((uint)secondTypeOffset),
                    typeSpec),
                new(
                    InterceptionGenerationRelocationKind.MemberRef,
                    checked((uint)privateMemberOffset),
                    privateMemberRef,
                    privateValue.DeclaringType!.MetadataToken,
                    privateValue.Name),
                new(
                    InterceptionGenerationRelocationKind.MemberRef,
                    checked((uint)internalMemberOffset),
                    internalMemberRef,
                    internalTransform.DeclaringType!.MetadataToken,
                    internalTransform.Name),
                new(
                    InterceptionGenerationRelocationKind.MethodSpec,
                    checked((uint)methodOffset),
                    methodSpec,
                    privateIdentity.MetadataToken)
            ],
            [new(0, 0)]);

    private static byte[] CreateBody(
        out int firstTypeOffset,
        out int secondTypeOffset,
        out int privateMemberOffset,
        out int internalMemberOffset,
        out int methodOffset)
    {
        const int headerSize = 12;
        const int codeSize = 29;
        var body = new byte[headerSize + codeSize];
        BinaryPrimitives.WriteUInt16LittleEndian(body, 0x3003);
        BinaryPrimitives.WriteUInt16LittleEndian(body.AsSpan(2), 1);
        BinaryPrimitives.WriteInt32LittleEndian(body.AsSpan(4), codeSize);

        var offset = headerSize;
        body[offset++] = 0xD0;
        firstTypeOffset = offset;
        offset += sizeof(int);
        body[offset++] = 0x26;
        body[offset++] = 0xD0;
        secondTypeOffset = offset;
        offset += sizeof(int);
        body[offset++] = 0x26;
        body[offset++] = 0x28;
        privateMemberOffset = offset;
        offset += sizeof(int);
        body[offset++] = 0x4A;
        body[offset++] = 0x28;
        internalMemberOffset = offset;
        offset += sizeof(int);
        body[offset++] = 0x28;
        methodOffset = offset;
        offset += sizeof(int);
        body[offset++] = 0x2A;
        Assert.AreEqual(body.Length, offset);
        return body;
    }

    private static byte[] GenericBoxOfInt32Signature()
    {
        var typeDefOrRef = checked(
            (uint)(typeof(LateMetadataBox<>).MetadataToken & 0x00FFFFFF) << 2);
        return
        [
            0x15,
            0x12,
            .. EncodeCompressedInteger(typeDefOrRef),
            0x01,
            0x08
        ];
    }

    private static byte[] EncodeCompressedInteger(uint value)
    {
        if (value <= 0x7F)
            return [checked((byte)value)];
        if (value <= 0x3FFF)
        {
            return
            [
                checked((byte)(0x80 | value >> 8)),
                checked((byte)value)
            ];
        }
        if (value <= 0x1FFFFFFF)
        {
            return
            [
                checked((byte)(0xC0 | value >> 24)),
                checked((byte)(value >> 16)),
                checked((byte)(value >> 8)),
                checked((byte)value)
            ];
        }
        throw new ArgumentOutOfRangeException(nameof(value));
    }

    private static (int TypeSpecs, int MemberRefs, int MethodSpecs)
        ReadOriginalRowsAndAssertAbsent(
            Module module,
            byte[] typeSpecSignature,
            MethodInfo privateMember,
            byte[] privateMemberSignature,
            MethodInfo internalMember,
            byte[] internalMemberSignature,
            MethodInfo genericMethod,
            byte[] methodSpecSignature)
    {
        using var stream = File.OpenRead(module.FullyQualifiedName);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var typeSpecs = metadata.GetTableRowCount(TableIndex.TypeSpec);
        var memberRefs = metadata.GetTableRowCount(TableIndex.MemberRef);
        var methodSpecs = metadata.GetTableRowCount(TableIndex.MethodSpec);

        Assert.IsFalse(
            Enumerable.Range(1, typeSpecs).Any(row =>
                metadata.GetBlobBytes(
                    metadata.GetTypeSpecification(
                        MetadataTokens.TypeSpecificationHandle(row)).Signature)
                .AsSpan()
                .SequenceEqual(typeSpecSignature)),
            "The generic TypeSpec must be absent after the target has JITted.");
        Assert.IsFalse(
            HasMemberRef(
                metadata,
                memberRefs,
                privateMember,
                privateMemberSignature),
            "The private MemberRef must be absent after the target has JITted.");
        Assert.IsFalse(
            HasMemberRef(
                metadata,
                memberRefs,
                internalMember,
                internalMemberSignature),
            "The internal MemberRef must be absent after the target has JITted.");
        Assert.IsFalse(
            Enumerable.Range(1, methodSpecs).Any(row =>
            {
                var candidate = metadata.GetMethodSpecification(
                    MetadataTokens.MethodSpecificationHandle(row));
                return MetadataTokens.GetToken(candidate.Method) ==
                        genericMethod.MetadataToken &&
                    metadata.GetBlobBytes(candidate.Signature)
                        .AsSpan()
                        .SequenceEqual(methodSpecSignature);
            }),
            "The MethodSpec must be absent after the target has JITted.");
        return (typeSpecs, memberRefs, methodSpecs);
    }

    private static bool HasMemberRef(
        MetadataReader metadata,
        int rowCount,
        MethodInfo member,
        byte[] signature) =>
        Enumerable.Range(1, rowCount).Any(row =>
        {
            var candidate = metadata.GetMemberReference(
                MetadataTokens.MemberReferenceHandle(row));
            return MetadataTokens.GetToken(candidate.Parent) ==
                    member.DeclaringType!.MetadataToken &&
                metadata.GetString(candidate.Name) == member.Name &&
                metadata.GetBlobBytes(candidate.Signature)
                    .AsSpan()
                    .SequenceEqual(signature);
        });

    private static int[] ReadTokens(
        InterceptionProfiler profiler,
        ulong requestId)
    {
        InterceptionGenerationRelocationKind[] expected =
        [
            InterceptionGenerationRelocationKind.TypeSpec,
            InterceptionGenerationRelocationKind.TypeSpec,
            InterceptionGenerationRelocationKind.MemberRef,
            InterceptionGenerationRelocationKind.MemberRef,
            InterceptionGenerationRelocationKind.MethodSpec
        ];
        var tokens = new int[expected.Length];
        for (var index = 0; index < expected.Length; ++index)
        {
            var result = profiler.GetRelocationResult(
                requestId,
                checked((uint)index));
            Assert.AreEqual(expected[index], result.Kind);
            Assert.AreEqual(0, result.HResult);
            tokens[index] = result.MetadataToken;
        }
        return tokens;
    }

    private static void AssertGeneration(
        InterceptionProfiler profiler,
        ulong requestId,
        ulong generation,
        ulong prior)
    {
        var completion = profiler.GetGenerationCompletion(requestId);
        Assert.AreEqual(InterceptionState.Active, completion.State);
        Assert.AreEqual(generation, completion.GenerationId);
        Assert.AreEqual(prior, completion.PriorGenerationId);
        Assert.AreEqual(
            InterceptionGenerationFailureStage.None,
            completion.FailureStage);
        Assert.AreEqual(5u, completion.RequestedRelocations);
        Assert.AreEqual(5u, completion.AppliedRelocations);
        Assert.AreEqual(1u, completion.RequestedIlMapEntries);
        Assert.AreEqual(1u, completion.AppliedIlMapEntries);
        Assert.AreNotEqual(0ul, completion.TargetRejitId);
    }

    private static void AssertResolvedTokens(
        Module module,
        int[] tokens,
        MethodInfo privateValue,
        MethodInfo internalTransform,
        MethodInfo privateIdentity)
    {
        var relocatedType = module.ResolveType(tokens[0]);
        Assert.IsTrue(relocatedType.IsConstructedGenericType);
        Assert.AreEqual(
            typeof(LateMetadataBox<>),
            relocatedType.GetGenericTypeDefinition());
        Assert.AreEqual(typeof(int), relocatedType.GetGenericArguments()[0]);

        var relocatedMember = module.ResolveMethod(tokens[2]);
        Assert.AreEqual(privateValue.Name, relocatedMember!.Name);
        Assert.AreEqual(privateValue.DeclaringType, relocatedMember.DeclaringType);
        Assert.IsTrue(relocatedMember.IsPrivate);

        var relocatedInternal = module.ResolveMethod(tokens[3]);
        Assert.AreEqual(internalTransform.Name, relocatedInternal!.Name);
        Assert.AreEqual(
            internalTransform.DeclaringType,
            relocatedInternal.DeclaringType);
        Assert.IsTrue(relocatedInternal.IsAssembly);

        var relocatedMethod = (MethodInfo)module.ResolveMethod(tokens[4])!;
        Assert.AreEqual(
            privateIdentity.MetadataToken,
            relocatedMethod.GetGenericMethodDefinition().MetadataToken);
        Assert.AreEqual(typeof(int), relocatedMethod.GetGenericArguments()[0]);
    }

    private static MethodInfo Method(string name) =>
        typeof(LateMetadataRelocationTarget).GetMethod(
            name,
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static MethodInfo InternalMethod() =>
        typeof(LateMetadataInternalTarget).GetMethod(
            nameof(LateMetadataInternalTarget.InternalTransform),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void RequireProfiledHost()
    {
        if (Environment.GetEnvironmentVariable(
                InterceptionProfiler.PathEnvironmentVariable) is null)
        {
            Assert.Inconclusive(
                "Run through AlvorKit.Script.TestInterception.");
        }
    }

    private static InterceptionCompletion WaitFor(
        InterceptionProfiler profiler,
        ulong requestId)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < RequestTimeout)
        {
            _ = LateMetadataRelocationTarget.Caller();
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }
        }
        throw new TimeoutException();
    }
}
