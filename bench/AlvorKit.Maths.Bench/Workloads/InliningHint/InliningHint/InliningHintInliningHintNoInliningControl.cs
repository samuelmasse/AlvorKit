namespace AlvorKit;

internal static class InliningHintInliningHintNoInliningControl
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Run(Vec3[] left, Vec3[] right, Vec3[] output, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < 4096; i++)
                output[i] = InliningHintApproaches.NoInlineHotPath(left[i], right[i]);
        }
    }
}
