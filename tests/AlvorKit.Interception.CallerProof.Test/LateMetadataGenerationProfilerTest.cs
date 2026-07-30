namespace AlvorKit.Interception.CallerProof.Test;

[TestClass]
public sealed class LateMetadataGenerationProfilerTest
{
    private static readonly TimeSpan RequestTimeout =
        TimeSpan.FromSeconds(10);

    /// <summary>Creates a late StandAloneSig after JIT and reuses its token on replacement.</summary>
    [TestMethod]
    public unsafe void GenerationRelocatesLateCalliSignatureIdempotently()
    {
        RequireProfiledHost();
        var caller = Method(nameof(LateMetadataTarget.Caller));
        var template = Method(nameof(LateMetadataTarget.CalliTemplate));
        var profiler = InterceptionProfiler.Connect();
        var source = LoadedSourceMethodResolver.Resolve(caller, profiler);
        Assert.IsTrue(
            source.IsSuccessful,
            string.Join(
                Environment.NewLine,
                source.Rejections.Select(rejection => rejection.Detail)));
        var baseline = source.Target!.Body;
        var templateBody = ReflectionMethodBodyEncoder.Read(template)
            .Bytes
            .ToArray();
        var symbolic = AddInt32Argument(templateBody);
        var calliOffset = FindCalliOperand(symbolic);
        var existingToken = BinaryPrimitives.ReadInt32LittleEndian(
            templateBody.AsSpan(FindCalliOperand(templateBody), 4));
        var signature = AddInt32Parameter(
            template.Module.ResolveSignature(existingToken));
        var signaturesBefore = ReadStandaloneSignatures(template.Module);
        Assert.IsFalse(
            signaturesBefore.Any(candidate =>
                candidate.Value.AsSpan().SequenceEqual(signature)),
            "The late signature must not already exist in loaded metadata.");
        symbolic.AsSpan(calliOffset, 4).Clear();

        RuntimeHelpers.PrepareMethod(
            Method(nameof(LateMetadataTarget.Replacement)).MethodHandle);
        LateMetadataTarget.ReplacementPointer =
            (nint)(delegate* unmanaged[Cdecl]<int, int>)&
                    LateMetadataTarget.Replacement;
        Assert.AreEqual(2, LateMetadataTarget.Caller());

        using var patch = profiler.Install(
            Plan(caller, baseline.Identity, symbolic, calliOffset, signature, 1, 0));
        var first = WaitFor(profiler, patch.LastRequestId);
        Assert.AreEqual(InterceptionState.Active, first.State);
        Assert.AreEqual(73, LateMetadataTarget.Caller());
        var firstGeneration = profiler.GetGenerationCompletion(
            patch.LastRequestId);
        Assert.AreEqual(1u, firstGeneration.AppliedRelocations);
        Assert.AreEqual(1u, firstGeneration.AppliedIlMapEntries);
        Assert.AreEqual(InterceptionGenerationFailureStage.None, firstGeneration.FailureStage);
        var firstRelocation = profiler.GetRelocationResult(
            patch.LastRequestId,
            0);
        Assert.AreEqual(
            0x11000000 | signaturesBefore.Count + 1,
            firstRelocation.MetadataToken,
            "A previously absent StandAloneSig must append one metadata row.");
        CollectionAssert.AreEqual(
            signature,
            template.Module.ResolveSignature(
                firstRelocation.MetadataToken));

        var replaceRequest = patch.Replace(
            Plan(caller, baseline.Identity, symbolic, calliOffset, signature, 2, 1));
        _ = WaitFor(profiler, replaceRequest);
        var replacementRelocation = profiler.GetRelocationResult(
            replaceRequest,
            0);
        Assert.AreEqual(
            firstRelocation.MetadataToken,
            replacementRelocation.MetadataToken,
            "Identical late signatures must reuse the same metadata token.");
        Assert.AreEqual(73, LateMetadataTarget.Caller());

        var removeRequest = patch.Remove();
        _ = WaitFor(profiler, removeRequest);
        Assert.AreEqual(2, LateMetadataTarget.Caller());
    }

    /// <summary>Rejects a generation whose authoritative baseline SHA-256 identity is stale.</summary>
    [TestMethod]
    public void GenerationRejectsStaleLoadedBodyIdentity()
    {
        RequireProfiledHost();
        var caller = Method(nameof(LateMetadataTarget.Caller));
        var template = Method(nameof(LateMetadataTarget.CalliTemplate));
        var profiler = InterceptionProfiler.Connect();
        var staleIdentity = profiler.GetLoadedMethodBody(
                InterceptionTarget.FromMethod(template))
            .Identity;
        var symbolic = ReflectionMethodBodyEncoder.Read(template)
            .Bytes
            .ToArray();
        var calliOffset = FindCalliOperand(symbolic);
        var token = BinaryPrimitives.ReadInt32LittleEndian(
            symbolic.AsSpan(calliOffset, 4));
        var signature = template.Module.ResolveSignature(token);
        symbolic.AsSpan(calliOffset, 4).Clear();

        var plan = new InterceptionGenerationPlan(
            InterceptionTarget.FromMethod(caller),
            InterceptionMethodBody.FromRaw(symbolic),
            staleIdentity,
            91,
            0,
            [new(
                InterceptionGenerationRelocationKind.StandaloneSignature,
                ((uint)calliOffset),
                signature)],
            [new(0, 0)]);
        var patch = profiler.Install(plan);
        var completion = WaitForFailure(profiler, patch.LastRequestId);
        Assert.AreEqual(InterceptionState.Failed, completion.State);
        var generation = profiler.GetGenerationCompletion(
            patch.LastRequestId);
        Assert.AreEqual(
            InterceptionGenerationFailureStage.Baseline,
            generation.FailureStage);
        Assert.AreEqual(0u, generation.AppliedRelocations);
        Assert.AreEqual(2, LateMetadataTarget.Caller());
    }

    private static InterceptionGenerationPlan Plan(
        MethodInfo caller,
        LoadedMethodBodyIdentity baselineIdentity,
        byte[] symbolic,
        int calliOffset,
        byte[] signature,
        ulong generation,
        ulong prior) =>
        new(
            InterceptionTarget.FromMethod(caller),
            InterceptionMethodBody.FromRaw(symbolic),
            baselineIdentity,
            generation,
            prior,
            [new(
                InterceptionGenerationRelocationKind.StandaloneSignature,
                ((uint)calliOffset),
                signature)],
            [new(0, 0)]);

    private static int FindCalliOperand(byte[] body)
    {
        var matches = Enumerable.Range(12, body.Length - 16)
            .Where(index => body[index] == 0x29)
            .ToArray();
        Assert.AreEqual(1, matches.Length);
        return matches[0] + 1;
    }

    private static byte[] AddInt32Argument(byte[] body)
    {
        const int fatHeaderSize = 12;
        var codeSize = BinaryPrimitives.ReadInt32LittleEndian(
            body.AsSpan(4));
        Assert.AreEqual(body.Length - fatHeaderSize, codeSize);
        var result = new byte[body.Length + 1];
        body.AsSpan(0, fatHeaderSize).CopyTo(result);
        result[fatHeaderSize] = 0x1B;
        body.AsSpan(fatHeaderSize).CopyTo(
            result.AsSpan(fatHeaderSize + 1));
        BinaryPrimitives.WriteInt32LittleEndian(
            result.AsSpan(4),
            codeSize + 1);
        BinaryPrimitives.WriteUInt16LittleEndian(
            result.AsSpan(2),
            Math.Max(
                (ushort)2,
                BinaryPrimitives.ReadUInt16LittleEndian(
                    result.AsSpan(2))));
        return result;
    }

    private static byte[] AddInt32Parameter(byte[] signature)
    {
        CollectionAssert.AreEqual(
            new byte[] { 0x01, 0x00, 0x08 },
            signature,
            "The template must remain an unmanaged Cdecl int() call site.");
        return [0x01, 0x01, 0x08, 0x08];
    }

    private static IReadOnlyList<KeyValuePair<int, byte[]>>
        ReadStandaloneSignatures(Module module)
    {
        List<KeyValuePair<int, byte[]>> signatures = [];
        for (var row = 1; row <= 0x00FFFFFF; ++row)
        {
            var token = 0x11000000 | row;
            try
            {
                signatures.Add(new(
                    token,
                    module.ResolveSignature(token)));
            }
            catch (ArgumentException)
            {
                break;
            }
        }
        return signatures;
    }

    private static MethodInfo Method(string name) =>
        typeof(LateMetadataTarget).GetMethod(
            name,
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
            _ = LateMetadataTarget.Caller();
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
            {
                completion.ThrowIfFailed();
                return completion;
            }
        }
        throw new TimeoutException();
    }

    private static InterceptionCompletion WaitForFailure(
        InterceptionProfiler profiler,
        ulong requestId)
    {
        var stopwatch = Stopwatch.StartNew();
        while (stopwatch.Elapsed < RequestTimeout)
        {
            var completion = profiler.GetCompletion(requestId);
            if (completion.IsTerminal)
                return completion;
        }
        throw new TimeoutException();
    }
}
