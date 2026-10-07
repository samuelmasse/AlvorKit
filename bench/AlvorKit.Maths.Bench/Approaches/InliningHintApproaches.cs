namespace AlvorKit;

internal static class InliningHintApproaches
{
    internal static Vec3 DefaultHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vec3 InlineHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static Vec3 OptimizeHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    internal static Vec3 BothHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static Vec3 NoInlineHotPath(Vec3 left, Vec3 right) => Vec3.Normalize(left - right) + left * 0.5f;
}
