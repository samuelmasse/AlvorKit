namespace AlvorKit.Script.MathsGen;

/// <summary>Builds measured complete-register Boolean selection expressions.</summary>
internal static class BooleanSelectExpression
{
    /// <summary>Gets whether the Boolean mask and payload exactly fill one profitable Vector128 operation.</summary>
    public static bool Supports(VectorSpec mask, ScalarSpec payload) =>
        mask.Dimension == 4 && payload.Kind is ScalarKind.Float or ScalarKind.Int;

    /// <summary>Returns a native conditional-selection expression without reinterpreting Boolean storage.</summary>
    public static string Select(VectorSpec mask, ScalarSpec payload)
    {
        var payloadType = payload.VectorName(mask.Dimension);
        var registerScalar = payload.CSharpName;
        var registerType = $"System.Runtime.Intrinsics.Vector128<{registerScalar}>";
        var nativeMask =
            $"System.Runtime.Intrinsics.Vector128.Create(X ? -1 : 0, Y ? -1 : 0, Z ? -1 : 0, W ? -1 : 0)";
        if (payload.Kind == ScalarKind.Float)
            nativeMask = $"System.Runtime.Intrinsics.Vector128.As<int, float>({nativeMask})";

        return $"Unsafe.BitCast<{registerType}, {payloadType}>(" +
            $"System.Runtime.Intrinsics.Vector128.ConditionalSelect({nativeMask}, " +
            $"Unsafe.BitCast<{payloadType}, {registerType}>(whenTrue), " +
            $"Unsafe.BitCast<{payloadType}, {registerType}>(whenFalse)))";
    }
}
