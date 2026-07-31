using AlvorKit.Interception.Profiler;

namespace AlvorKit.Interception;

public partial class InterceptionProfiler
{
    internal ulong Replace(InterceptionPatchHandle handle, InterceptionPlan plan)
    {
        if (plan.Target != handle.Target)
            throw new ArgumentException("A patch handle cannot move to another exact method.", nameof(plan));

        return EnqueueInstall(handle.PatchId, plan);
    }

    internal ulong Replace(
        InterceptionPatchHandle handle,
        InterceptionDispatchPlan plan)
    {
        if (plan.Target != handle.Target)
            throw new ArgumentException("A patch handle cannot move to another exact method.", nameof(plan));

        return EnqueueInstall(handle.PatchId, plan);
    }

    internal ulong Remove(InterceptionPatchHandle handle)
    {
        var requestId = NextRequestId();
        var request = new InterceptionProfilerRemove
        {
            Size = ((uint)Marshal.SizeOf<InterceptionProfilerRemove>()),
            AbiVersion = NativeAbiVersion,
            RequestId = requestId,
            PatchId = handle.PatchId,
            Target = ToNative(handle.Target)
        };
        Marshal.ThrowExceptionForHR(api.EnqueueRemove(in request));
        return requestId;
    }

    private ulong EnqueueInstall(ulong patchId, InterceptionPlan plan)
    {
        var body = plan.MethodBody.Bytes.Span;
        if ((uint)body.Length > Capabilities.MaximumIlBodyBytes)
        {
            throw new ArgumentException(
                $"Replacement body has {body.Length} bytes; the profiler limit is {Capabilities.MaximumIlBodyBytes}.",
                nameof(plan));
        }

        var requestId = NextRequestId();
        var request = new InterceptionProfilerInstall
        {
            Size = ((uint)Marshal.SizeOf<InterceptionProfilerInstall>()),
            AbiVersion = NativeAbiVersion,
            RequestId = requestId,
            PatchId = patchId,
            Target = ToNative(plan.Target),
            PatchFlags = (uint)plan.Flags,
            IlBodySize = ((uint)body.Length)
        };
        Marshal.ThrowExceptionForHR(api.EnqueueInstall(
            in request,
            body,
            ((uint)body.Length)));
        return requestId;
    }

    private ulong EnqueueInstall(
        ulong patchId,
        InterceptionDispatchPlan plan)
    {
        var requestId = NextRequestId();
        var request = new InterceptionProfilerInstallDispatch
        {
            Size = ((uint)Marshal.SizeOf<InterceptionProfilerInstallDispatch>()),
            AbiVersion = NativeAbiVersion,
            RequestId = requestId,
            PatchId = patchId,
            Target = ToNative(plan.Target),
            PatchFlags = (uint)plan.Flags,
            SlotId = plan.SlotId,
            ResolverPointer = ((ulong)plan.ResolverPointer)
        };
        Marshal.ThrowExceptionForHR(api.EnqueueInstallDispatch(in request));
        return requestId;
    }

    private static ulong NextRequestId() =>
        ((ulong)Interlocked.Increment(ref nextRequestId));

    private static ulong NextPatchId() =>
        ((ulong)Interlocked.Increment(ref nextPatchId));

    private static void ConfigureNativeResolver(string fullPath)
    {
        lock (NativeGate)
        {
            if (resolverInstalled)
            {
                if (!string.Equals(nativePath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"The profiler binding is already connected to '{nativePath}'.");
                }

                return;
            }

            nativeHandle = NativeLibrary.Load(fullPath);
            nativePath = fullPath;
            NativeLibrary.SetDllImportResolver(
                typeof(InterceptionProfilerBackend).Assembly,
                ResolveNativeLibrary);
            resolverInstalled = true;
        }
    }

    private static nint ResolveNativeLibrary(
        string libraryName,
        Assembly assembly,
        DllImportSearchPath? searchPath)
    {
        return libraryName == NativeLibraryName ? nativeHandle : 0;
    }

    private static InterceptionProfilerTarget ToNative(InterceptionTarget target) =>
        new()
        {
            ModuleMvid = ToNative(target.ModuleMvid),
            MethodToken = target.MethodToken,
            SignatureHash = target.SignatureHash
        };

    private static InterceptionTarget FromNative(InterceptionProfilerTarget target)
    {
        var mvid = FromNative(target.ModuleMvid);
        return InterceptionTarget.FromIdentity(
            mvid,
            target.MethodToken,
            target.SignatureHash,
            $"{mvid:D}:0x{target.MethodToken:X8}");
    }

    private static InterceptionProfilerGuid ToNative(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        value.TryWriteBytes(bytes);
        var result = new InterceptionProfilerGuid
        {
            Data1 = BitConverter.ToUInt32(bytes),
            Data2 = BitConverter.ToUInt16(bytes[4..]),
            Data3 = BitConverter.ToUInt16(bytes[6..])
        };
        for (var index = 0; index < 8; ++index)
            result.Data4[index] = bytes[index + 8];
        return result;
    }

    /// <summary>Converts one generated native GUID record to its managed value.</summary>
    internal static Guid FromNative(InterceptionProfilerGuid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        BitConverter.TryWriteBytes(bytes, value.Data1);
        BitConverter.TryWriteBytes(bytes[4..], value.Data2);
        BitConverter.TryWriteBytes(bytes[6..], value.Data3);
        for (var index = 0; index < 8; ++index)
            bytes[index + 8] = value.Data4[index];
        return new(bytes);
    }
}
