namespace AlvorKit.Script.MathsGen;

/// <summary>Resolves generated scalar pairs according to C# binary numeric promotion rules.</summary>
internal static class ScalarPromotion
{
    /// <summary>Returns the scalar produced by a C# numeric binary operator, or null when the scalar pair is invalid.</summary>
    public static ScalarSpec? BinaryNumericResult(ScalarSpec left, ScalarSpec right)
    {
        if (left.IsBool || right.IsBool)
            return null;
        if (left.Kind == ScalarKind.Half || right.Kind == ScalarKind.Half)
            return PromotedHalf(left, right);
        if (left.Kind == ScalarKind.Double || right.Kind == ScalarKind.Double)
            return IsClassicFloatingOperand(left) && IsClassicFloatingOperand(right) ? VectorCatalog.Double : null;
        if (left.Kind == ScalarKind.Float || right.Kind == ScalarKind.Float)
            return IsClassicFloatingOperand(left) && IsClassicFloatingOperand(right) ? VectorCatalog.Float : null;
        if (left.Kind == ScalarKind.Int128 || right.Kind == ScalarKind.Int128)
            return PromotedInt128(left, right);
        if (left.Kind == ScalarKind.UInt128 || right.Kind == ScalarKind.UInt128)
            return PromotedUInt128(left, right);

        return PromotedClassicInteger(left, right);
    }

    /// <summary>Returns the scalar produced by a C# integer binary operator, or null when the scalar pair is invalid.</summary>
    public static ScalarSpec? BinaryIntegerResult(ScalarSpec left, ScalarSpec right)
    {
        if (!left.IsInteger || !right.IsInteger)
            return null;
        if (left.Kind == ScalarKind.Int128 || right.Kind == ScalarKind.Int128)
            return PromotedInt128(left, right);
        if (left.Kind == ScalarKind.UInt128 || right.Kind == ScalarKind.UInt128)
            return PromotedUInt128(left, right);

        return PromotedClassicInteger(left, right);
    }

    /// <summary>Returns the scalar produced by a C# unary plus operator, or null when the operator is invalid.</summary>
    public static ScalarSpec? UnaryPlusResult(ScalarSpec scalar)
    {
        if (scalar.Kind is ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16)
            return VectorCatalog.Int;
        return scalar.IsBool ? null : scalar;
    }

    /// <summary>Returns the scalar produced by a C# unary negation operator, or null when the operator is invalid.</summary>
    public static ScalarSpec? UnaryNegationResult(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int => VectorCatalog.Int,
        ScalarKind.UInt => VectorCatalog.Int64,
        ScalarKind.Int64 => VectorCatalog.Int64,
        ScalarKind.UInt64 => null,
        ScalarKind.Int128 => VectorCatalog.Int128,
        ScalarKind.UInt128 => VectorCatalog.UInt128,
        ScalarKind.Half => VectorCatalog.Half,
        ScalarKind.Float => VectorCatalog.Float,
        ScalarKind.Double => VectorCatalog.Double,
        _ => null,
    };

    /// <summary>Returns the scalar produced by a C# bitwise-complement operator, or null when the operator is invalid.</summary>
    public static ScalarSpec? UnaryComplementResult(ScalarSpec scalar) => scalar.Kind switch
    {
        ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int => VectorCatalog.Int,
        ScalarKind.UInt => VectorCatalog.UInt,
        ScalarKind.Int64 => VectorCatalog.Int64,
        ScalarKind.UInt64 => VectorCatalog.UInt64,
        ScalarKind.Int128 => VectorCatalog.Int128,
        ScalarKind.UInt128 => VectorCatalog.UInt128,
        _ => null,
    };

    /// <summary>Returns the scalar produced by a C# shift operator, or null when the left operand is invalid.</summary>
    public static ScalarSpec? ShiftResult(ScalarSpec left) => left.Kind switch
    {
        ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int => VectorCatalog.Int,
        ScalarKind.UInt => VectorCatalog.UInt,
        ScalarKind.Int64 => VectorCatalog.Int64,
        ScalarKind.UInt64 => VectorCatalog.UInt64,
        ScalarKind.Int128 => VectorCatalog.Int128,
        ScalarKind.UInt128 => VectorCatalog.UInt128,
        _ => null,
    };

    /// <summary>Returns whether a scalar can be used as a C# shift count.</summary>
    public static bool IsShiftCount(ScalarSpec scalar) =>
        scalar.Kind is ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or ScalarKind.Int;

    /// <summary>Returns whether scalar arithmetic can already use the same-scalar vector operator.</summary>
    public static bool ExistingScalarOperatorCovers(ScalarSpec vectorScalar, ScalarSpec scalar) =>
        vectorScalar.Kind == scalar.Kind || VectorCatalog.IsImplicitConversion(scalar, vectorScalar);

    /// <summary>Returns whether vector arithmetic can already use one operand's same-scalar vector operator.</summary>
    public static bool ExistingVectorOperatorCovers(ScalarSpec left, ScalarSpec right, ScalarSpec result) =>
        (result.Kind == left.Kind && VectorCatalog.IsImplicitConversion(right, result)) ||
        (result.Kind == right.Kind && VectorCatalog.IsImplicitConversion(left, result));

    private static ScalarSpec? PromotedHalf(ScalarSpec left, ScalarSpec right)
    {
        if (left.Kind != ScalarKind.Half && right.Kind != ScalarKind.Half)
            return null;

        var other = left.Kind == ScalarKind.Half ? right : left;
        return other.Kind is ScalarKind.Half or ScalarKind.Int8 or ScalarKind.UInt8 ? VectorCatalog.Half : null;
    }

    private static ScalarSpec? PromotedInt128(ScalarSpec left, ScalarSpec right)
    {
        if (left.Kind == ScalarKind.Int128 && right.Kind == ScalarKind.Int128)
            return VectorCatalog.Int128;

        var other = left.Kind == ScalarKind.Int128 ? right : left;
        return other.Kind == ScalarKind.UInt128 || !IsClassicIntegerOperand(other) ? null : VectorCatalog.Int128;
    }

    private static ScalarSpec? PromotedUInt128(ScalarSpec left, ScalarSpec right)
    {
        if (left.Kind == ScalarKind.UInt128 && right.Kind == ScalarKind.UInt128)
            return VectorCatalog.UInt128;

        var other = left.Kind == ScalarKind.UInt128 ? right : left;
        return IsClassicUnsignedIntegerOperand(other) ? VectorCatalog.UInt128 : null;
    }

    private static ScalarSpec? PromotedClassicInteger(ScalarSpec left, ScalarSpec right)
    {
        if (!IsClassicIntegerOperand(left) || !IsClassicIntegerOperand(right))
            return null;

        if (left.Kind == ScalarKind.UInt64 || right.Kind == ScalarKind.UInt64)
            return IsClassicUnsignedIntegerOperand(left) && IsClassicUnsignedIntegerOperand(right)
                ? VectorCatalog.UInt64
                : null;
        if (left.Kind == ScalarKind.Int64 || right.Kind == ScalarKind.Int64)
            return VectorCatalog.Int64;
        if (left.Kind == ScalarKind.UInt || right.Kind == ScalarKind.UInt)
        {
            var other = left.Kind == ScalarKind.UInt ? right : left;
            return other.Kind is ScalarKind.Int8 or ScalarKind.Int16 or ScalarKind.Int ? VectorCatalog.Int64 : VectorCatalog.UInt;
        }

        return VectorCatalog.Int;
    }

    private static bool IsClassicFloatingOperand(ScalarSpec scalar) =>
        scalar.Kind is
            ScalarKind.Float or ScalarKind.Double or ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or
            ScalarKind.Int or ScalarKind.UInt or ScalarKind.Int64 or ScalarKind.UInt64;

    private static bool IsClassicIntegerOperand(ScalarSpec scalar) =>
        scalar.Kind is
            ScalarKind.Int8 or ScalarKind.UInt8 or ScalarKind.Int16 or ScalarKind.UInt16 or
            ScalarKind.Int or ScalarKind.UInt or ScalarKind.Int64 or ScalarKind.UInt64;

    private static bool IsClassicUnsignedIntegerOperand(ScalarSpec scalar) =>
        scalar.Kind is ScalarKind.UInt8 or ScalarKind.UInt16 or ScalarKind.UInt or ScalarKind.UInt64 or ScalarKind.UInt128;
}
