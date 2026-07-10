namespace AlvorKit.ECS;

internal static class EntArchDiagnostics<A>
{
    // All alloc owners for A must be quiescent while this snapshot scans alloc-local rows and columns.
    // The graph lock keeps the shared catalog stable; it does not synchronize alloc-local mutation.
    internal static EntArchMetrics Capture()
    {
        lock (EntArchGraph<A>.Sync)
        {
            var metrics = new EntArchMetrics();
            EntArchGraph<A>.AccumulateMetrics(ref metrics);
            EntArchRows<A>.AccumulateMetrics(ref metrics);
            return metrics;
        }
    }
}
