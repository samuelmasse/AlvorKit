namespace AlvorKit.Interception.CallerProof.Test;

internal static class GenericCallerRoute<T>
{
    internal static nint Pointer;
}

internal static class GenericCallerBridgeTarget
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Caller<T>(T value) =>
        Original(value);

    [MethodImpl(
        MethodImplOptions.NoInlining |
        MethodImplOptions.NoOptimization)]
    internal static unsafe T RoutedTemplate<T>(T value)
    {
        var operand = value;
        var route = Volatile.Read(ref GenericCallerRoute<T>.Pointer);
        if (route == 0)
            return Original(operand);

        return ((delegate* managed<T, T>)route)(operand);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Original<T>(T value) =>
        value;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string ReplaceString(string value) =>
        $"{value}:string";

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static object ReplaceObject(object value) =>
        ReferenceEquals(value, ObjectSentinel)
            ? StringSentinel
            : ObjectSentinel;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static int ReplaceInt32(int value) =>
        value + 100;

    internal static object ObjectSentinel { get; } = new();

    internal static object StringSentinel { get; } = new();
}
