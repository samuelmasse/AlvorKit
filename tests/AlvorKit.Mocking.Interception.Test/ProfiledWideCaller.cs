namespace AlvorKit;

/// <summary>Owns the wide selected caller and its exact gated trampoline lease.</summary>
internal static class ProfiledWideCaller
{
    private static ProfiledRouteBinding? binding;
    private static nint routePointer;

    /// <summary>Calls the wide operation with matcher spans or live span slices.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Selected(
        ProfiledWideTarget target,
        int[] values,
        int[] references,
        int[]? spans)
    {
        if (spans is null)
        {
            return target.Wide(
                values[0], ref references[0], Arg.Any<Span<int>>(2),
                values[1], ref references[1], Arg.Any<Span<int>>(5),
                values[2], ref references[2], Arg.Any<Span<int>>(8),
                values[3], ref references[3], Arg.Any<Span<int>>(11),
                values[4], ref references[4], Arg.Any<Span<int>>(14),
                values[5], ref references[5], Arg.Any<Span<int>>(17),
                values[6], ref references[6], Arg.Any<Span<int>>(20),
                values[7], ref references[7], Arg.Any<Span<int>>(23),
                values[8], ref references[8], Arg.Any<Span<int>>(26),
                values[9], ref references[9], Arg.Any<Span<int>>(29),
                values[10], ref references[10], Arg.Any<Span<int>>(32),
                values[11], ref references[11], Arg.Any<Span<int>>(35),
                values[12], ref references[12], Arg.Any<Span<int>>(38),
                values[13], ref references[13], Arg.Any<Span<int>>(41),
                values[14], ref references[14], Arg.Any<Span<int>>(44),
                values[15], ref references[15], Arg.Any<Span<int>>(47));
        }

        return target.Wide(
            values[0], ref references[0], spans.AsSpan(0, 1),
            values[1], ref references[1], spans.AsSpan(1, 1),
            values[2], ref references[2], spans.AsSpan(2, 1),
            values[3], ref references[3], spans.AsSpan(3, 1),
            values[4], ref references[4], spans.AsSpan(4, 1),
            values[5], ref references[5], spans.AsSpan(5, 1),
            values[6], ref references[6], spans.AsSpan(6, 1),
            values[7], ref references[7], spans.AsSpan(7, 1),
            values[8], ref references[8], spans.AsSpan(8, 1),
            values[9], ref references[9], spans.AsSpan(9, 1),
            values[10], ref references[10], spans.AsSpan(10, 1),
            values[11], ref references[11], spans.AsSpan(11, 1),
            values[12], ref references[12], spans.AsSpan(12, 1),
            values[13], ref references[13], spans.AsSpan(13, 1),
            values[14], ref references[14], spans.AsSpan(14, 1),
            values[15], ref references[15], spans.AsSpan(15, 1));
    }

    /// <summary>Runs original behavior while inert or the exact wide route.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static unsafe int RoutedTemplate(
        ProfiledWideTarget target,
        int[] values,
        int[] references,
        int[]? spans)
    {
        var route = Volatile.Read(ref routePointer);
        if (route == 0)
            return CallOriginal(target, values, references, spans);
        return ((delegate* managed<
            ProfiledWideTarget,
            int[],
            int[],
            int[]?,
            int>)route)(target, values, references, spans);
    }

    /// <summary>Binds the exact route while its published pointer remains inert.</summary>
    internal static void Bind(
        MockInterceptionRoute route,
        IInterceptionHandlerTrampoline trampoline) =>
        Volatile.Write(ref binding, new(route, trampoline));

    /// <summary>Clears the retired exact route lease.</summary>
    internal static void Clear() => Volatile.Write(ref binding, null);

    /// <summary>Publishes the exact route pointer or zero for original behavior.</summary>
    internal static void Publish(nint pointer) =>
        Volatile.Write(ref routePointer, pointer);

    /// <summary>Gets the prepared managed route entry point.</summary>
    internal static nint FunctionPointer()
    {
        var method = typeof(ProfiledWideCaller).GetMethod(
            nameof(Invoke),
            BindingFlags.NonPublic | BindingFlags.Static)!;
        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        return method.MethodHandle.GetFunctionPointer();
    }

    /// <summary>Invokes the leased exact trampoline or preserves original behavior.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int Invoke(
        ProfiledWideTarget target,
        int[] values,
        int[] references,
        int[]? spans)
    {
        var current = Volatile.Read(ref binding);
        if (current is null ||
            !current.Route.IsActivated ||
            !current.Trampoline.TryAcquire(out var entryPoint))
        {
            return CallOriginal(target, values, references, spans);
        }

        return CallTrampoline(
            entryPoint,
            target,
            values,
            references,
            spans);
    }

    private static int CallOriginal(
        ProfiledWideTarget target,
        int[] values,
        int[] references,
        int[]? spans)
    {
        if (spans is null)
        {
            return ProfiledWideOriginal.Invoke(
                target,
                values[0], ref references[0], Arg.Any<Span<int>>(2),
                values[1], ref references[1], Arg.Any<Span<int>>(5),
                values[2], ref references[2], Arg.Any<Span<int>>(8),
                values[3], ref references[3], Arg.Any<Span<int>>(11),
                values[4], ref references[4], Arg.Any<Span<int>>(14),
                values[5], ref references[5], Arg.Any<Span<int>>(17),
                values[6], ref references[6], Arg.Any<Span<int>>(20),
                values[7], ref references[7], Arg.Any<Span<int>>(23),
                values[8], ref references[8], Arg.Any<Span<int>>(26),
                values[9], ref references[9], Arg.Any<Span<int>>(29),
                values[10], ref references[10], Arg.Any<Span<int>>(32),
                values[11], ref references[11], Arg.Any<Span<int>>(35),
                values[12], ref references[12], Arg.Any<Span<int>>(38),
                values[13], ref references[13], Arg.Any<Span<int>>(41),
                values[14], ref references[14], Arg.Any<Span<int>>(44),
                values[15], ref references[15], Arg.Any<Span<int>>(47));
        }

        return ProfiledWideOriginal.Invoke(
            target,
            values[0], ref references[0], spans.AsSpan(0, 1),
            values[1], ref references[1], spans.AsSpan(1, 1),
            values[2], ref references[2], spans.AsSpan(2, 1),
            values[3], ref references[3], spans.AsSpan(3, 1),
            values[4], ref references[4], spans.AsSpan(4, 1),
            values[5], ref references[5], spans.AsSpan(5, 1),
            values[6], ref references[6], spans.AsSpan(6, 1),
            values[7], ref references[7], spans.AsSpan(7, 1),
            values[8], ref references[8], spans.AsSpan(8, 1),
            values[9], ref references[9], spans.AsSpan(9, 1),
            values[10], ref references[10], spans.AsSpan(10, 1),
            values[11], ref references[11], spans.AsSpan(11, 1),
            values[12], ref references[12], spans.AsSpan(12, 1),
            values[13], ref references[13], spans.AsSpan(13, 1),
            values[14], ref references[14], spans.AsSpan(14, 1),
            values[15], ref references[15], spans.AsSpan(15, 1));
    }

    private static unsafe int CallTrampoline(
        nint entryPoint,
        ProfiledWideTarget target,
        int[] values,
        int[] references,
        int[]? spans)
    {
        var trampoline = (delegate* managed<
            ProfiledWideTarget,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int, ref int, Span<int>,
            int>)entryPoint;
        if (spans is null)
        {
            return trampoline(
                target,
                values[0], ref references[0], Arg.Any<Span<int>>(2),
                values[1], ref references[1], Arg.Any<Span<int>>(5),
                values[2], ref references[2], Arg.Any<Span<int>>(8),
                values[3], ref references[3], Arg.Any<Span<int>>(11),
                values[4], ref references[4], Arg.Any<Span<int>>(14),
                values[5], ref references[5], Arg.Any<Span<int>>(17),
                values[6], ref references[6], Arg.Any<Span<int>>(20),
                values[7], ref references[7], Arg.Any<Span<int>>(23),
                values[8], ref references[8], Arg.Any<Span<int>>(26),
                values[9], ref references[9], Arg.Any<Span<int>>(29),
                values[10], ref references[10], Arg.Any<Span<int>>(32),
                values[11], ref references[11], Arg.Any<Span<int>>(35),
                values[12], ref references[12], Arg.Any<Span<int>>(38),
                values[13], ref references[13], Arg.Any<Span<int>>(41),
                values[14], ref references[14], Arg.Any<Span<int>>(44),
                values[15], ref references[15], Arg.Any<Span<int>>(47));
        }

        return trampoline(
            target,
            values[0], ref references[0], spans.AsSpan(0, 1),
            values[1], ref references[1], spans.AsSpan(1, 1),
            values[2], ref references[2], spans.AsSpan(2, 1),
            values[3], ref references[3], spans.AsSpan(3, 1),
            values[4], ref references[4], spans.AsSpan(4, 1),
            values[5], ref references[5], spans.AsSpan(5, 1),
            values[6], ref references[6], spans.AsSpan(6, 1),
            values[7], ref references[7], spans.AsSpan(7, 1),
            values[8], ref references[8], spans.AsSpan(8, 1),
            values[9], ref references[9], spans.AsSpan(9, 1),
            values[10], ref references[10], spans.AsSpan(10, 1),
            values[11], ref references[11], spans.AsSpan(11, 1),
            values[12], ref references[12], spans.AsSpan(12, 1),
            values[13], ref references[13], spans.AsSpan(13, 1),
            values[14], ref references[14], spans.AsSpan(14, 1),
            values[15], ref references[15], spans.AsSpan(15, 1));
    }
}
