namespace AlvorKit.Script.AlvorEye;

internal sealed partial class WindowsAlvorEyePlatform
{
    /// <summary>Snapshot flag for thread enumeration.</summary>
    private const uint SnapshotThreads = 0x00000004;

    /// <summary>Thread access needed for suspend and resume.</summary>
    private const uint ThreadSuspendResume = 0x0002;

    /// <summary>Invalid native handle marker.</summary>
    private static readonly nint InvalidHandle = new(-1);

    /// <inheritdoc/>
    public void FreezeProcess(int processId)
    {
        foreach (var threadId in EnumerateThreadIds(processId))
            WithThread(threadId, thread => { WindowsThreadNative.SuspendThread(thread); });
    }

    /// <inheritdoc/>
    public void ResumeProcess(int processId)
    {
        foreach (var threadId in EnumerateThreadIds(processId))
            WithThread(threadId, thread => { while (WindowsThreadNative.ResumeThread(thread) > 0) { } });
    }

    /// <summary>Enumerates thread ids for a process.</summary>
    private static IEnumerable<uint> EnumerateThreadIds(int processId)
    {
        var snapshot = WindowsThreadNative.CreateToolhelp32Snapshot(SnapshotThreads, 0);
        if (snapshot == InvalidHandle)
            throw new InvalidOperationException("Could not create thread snapshot.");

        try
        {
            var entry = new WindowsThreadNative.ThreadEntry32();
            entry.Size = (uint)Marshal.SizeOf(entry);
            if (!WindowsThreadNative.Thread32First(snapshot, ref entry))
                yield break;
            do
            {
                if (entry.OwnerProcessId == (uint)processId)
                    yield return entry.ThreadId;
            } while (WindowsThreadNative.Thread32Next(snapshot, ref entry));
        }
        finally
        {
            WindowsThreadNative.CloseHandle(snapshot);
        }
    }

    /// <summary>Opens one thread, applies an operation, and closes the handle.</summary>
    private static void WithThread(uint threadId, Action<nint> operation)
    {
        var thread = WindowsThreadNative.OpenThread(ThreadSuspendResume, false, threadId);
        if (thread == 0)
            return;
        try
        {
            operation(thread);
        }
        finally
        {
            WindowsThreadNative.CloseHandle(thread);
        }
    }
}
