namespace AlvorKit;

public partial class InterceptionProfiler
{
    internal ulong Replace(
        InterceptionPatchHandle handle,
        InterceptionGenerationPlan plan)
    {
        if (plan.Target != handle.Target)
            throw new ArgumentException("A patch handle cannot move to another exact method.", nameof(plan));

        return EnqueueInstall(handle.PatchId, plan);
    }

    private unsafe ulong EnqueueInstall(
        ulong patchId,
        InterceptionGenerationPlan plan)
    {
        var body = plan.MethodBody.Bytes.ToArray();
        if ((uint)body.Length > Capabilities.MaximumIlBodyBytes)
            throw new ArgumentException("The generation body exceeds the profiler limit.", nameof(plan));
        if ((uint)plan.Relocations.Length > Capabilities.MaximumRelocations)
            throw new ArgumentException("The generation has too many metadata relocations.", nameof(plan));
        if ((uint)plan.IlMap.Length > Capabilities.MaximumIlMapEntries)
            throw new ArgumentException("The generation has too many IL map entries.", nameof(plan));

        var relocations = new InterceptionProfilerRelocation[
            plan.Relocations.Length];
        List<byte> metadata = [];
        for (var index = 0; index < plan.Relocations.Length; ++index)
        {
            var relocation = plan.Relocations[index];
            var signatureOffset = ((uint)metadata.Count);
            metadata.AddRange(relocation.Signature.ToArray());
            var nameOffset = ((uint)metadata.Count);
            byte[] name = relocation.MemberName is null
                ? []
                : Encoding.UTF8.GetBytes(relocation.MemberName);
            metadata.AddRange(name);
            relocations[index] = new()
            {
                Kind = (uint)relocation.Kind,
                BodyOffset = relocation.BodyOffset,
                ParentToken = relocation.ParentToken,
                SignatureOffset = signatureOffset,
                SignatureSize = ((uint)relocation.Signature.Length),
                NameOffset = nameOffset,
                NameSize = ((uint)name.Length)
            };
        }
        var metadataBytes = metadata.ToArray();
        if ((uint)metadataBytes.Length > Capabilities.MaximumMetadataBytes)
            throw new ArgumentException("The generation metadata exceeds the profiler limit.", nameof(plan));

        var maps = plan.IlMap.Select(entry => new InterceptionProfilerIlMap
        {
            OldOffset = entry.OldOffset,
            NewOffset = entry.NewOffset,
            Accurate = entry.Accurate ? 1u : 0u
        }).ToArray();
        var identityBytes = Convert.FromHexString(
            plan.BaselineBodyIdentity.Value);
        var nativeIdentity = new InterceptionProfilerBodyIdentity();
        identityBytes.AsSpan().CopyTo(nativeIdentity.Sha256);

        var requestId = NextRequestId();
        var request = new InterceptionProfilerGeneration
        {
            Size = ((uint)Marshal.SizeOf<InterceptionProfilerGeneration>()),
            AbiVersion = NativeAbiVersion,
            RequestId = requestId,
            PatchId = patchId,
            Target = ToNative(plan.Target),
            PatchFlags = (uint)plan.Flags,
            IlBodySize = ((uint)body.Length),
            GenerationId = plan.GenerationId,
            PriorGenerationId = plan.PriorGenerationId,
            BaselineBodyIdentity = nativeIdentity,
            RelocationCount = ((uint)relocations.Length),
            MetadataSize = ((uint)metadataBytes.Length),
            IlMapCount = ((uint)maps.Length)
        };
        fixed (byte* bodyPointer = body)
        fixed (InterceptionProfilerRelocation* relocationPointer = relocations)
        fixed (byte* metadataPointer = metadataBytes)
        fixed (InterceptionProfilerIlMap* mapPointer = maps)
        {
            Marshal.ThrowExceptionForHR(api.EnqueueGeneration(
                in request,
                (nint)bodyPointer,
                ((uint)body.Length),
                relocationPointer,
                ((uint)relocations.Length),
                (nint)metadataPointer,
                ((uint)metadataBytes.Length),
                mapPointer,
                ((uint)maps.Length)));
        }
        return requestId;
    }

    /// <summary>Reads the exact authoritative loaded body after target and allowlist validation.</summary>
    public unsafe LoadedMethodBodySnapshot GetLoadedMethodBody(
        InterceptionTarget target)
    {
        var nativeTarget = ToNative(target);
        Marshal.ThrowExceptionForHR(api.GetLoadedMethodBody(
            in nativeTarget,
            0,
            0,
            out var requiredSize,
            out _));
        if (requiredSize == 0 ||
            requiredSize > Capabilities.MaximumIlBodyBytes)
        {
            throw new InvalidOperationException(
                "The profiler returned an invalid loaded method-body size.");
        }

        var bytes = new byte[requiredSize];
        fixed (byte* body = bytes)
        {
            Marshal.ThrowExceptionForHR(api.GetLoadedMethodBody(
                in nativeTarget,
                (nint)body,
                ((uint)bytes.Length),
                out var actualSize,
                out var identity));
            if (actualSize != requiredSize)
            {
                throw new InvalidOperationException(
                    "The loaded method body changed size while it was acquired.");
            }

            var snapshot = LoadedMethodBodyDecoder.Decode(bytes);
            var computed = Convert.FromHexString(snapshot.Identity.Value);
            for (var index = 0; index < computed.Length; ++index)
            {
                if (computed[index] != identity.Sha256[index])
                {
                    throw new InvalidOperationException(
                        "The profiler body bytes and native SHA-256 identity disagree.");
                }
            }
            return snapshot;
        }
    }

    /// <inheritdoc />
    bool ILoadedMethodBodySnapshotResolver.TryResolveLoadedBody(
        InterceptionTarget method,
        [NotNullWhen(true)] out LoadedMethodBodySnapshot? body)
    {
        try
        {
            body = GetLoadedMethodBody(method);
            return true;
        }
        catch (Exception exception) when (
            exception is ExternalException or
                UnauthorizedAccessException or
                FileNotFoundException)
        {
            body = null;
            return false;
        }
    }
}
