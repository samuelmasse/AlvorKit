namespace AlvorKit;

/// <summary>Reads the native profiler ABI without assuming platform module enumeration.</summary>
internal static class ProfilerActivationProbe
{
    /// <summary>Requires the loaded library to expose a ready profiler runtime.</summary>
    internal static void AssertActive(string profilerPath)
    {
        var library = NativeLibrary.Load(profilerPath);
        try
        {
            var address = NativeLibrary.GetExport(
                library,
                "alvorkit_interception_get_profiler_state");
            var getState = Marshal.GetDelegateForFunctionPointer<GetProfilerState>(
                address);
            var state = new NativeProfilerState
            {
                Size = (uint)Marshal.SizeOf<NativeProfilerState>()
            };
            Assert.AreEqual(
                0,
                getState(ref state),
                "The library was loadable but CoreCLR had not activated its profiler runtime.");
            Assert.AreEqual(3u, state.AbiVersion);
            Assert.AreEqual(1u, state.Ready);
        }
        finally
        {
            NativeLibrary.Free(library);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GetProfilerState(ref NativeProfilerState state);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeProfilerState
    {
        public uint Size;
        public uint AbiVersion;
        public uint Ready;
        public uint Stopping;
        public uint PendingRequests;
        public uint ActivePatches;
        public uint RetainedCompletions;
        public uint Reserved;
        public ulong LastRequestId;
    }
}
