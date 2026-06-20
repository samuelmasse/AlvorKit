namespace AlvorKit.Script.MathsGen;

/// <summary>Builds scalar-specific statement bodies for generated vector value semantics.</summary>
internal static class ValueSemanticsScalarEmitter
{
    /// <summary>Returns the scalar equality expression for this vector family.</summary>
    public static string EqualScalarExpression(VectorSpec vector) =>
        vector.Scalar.IsFloating ? "left.Equals(right)" : "left == right";

    /// <summary>Returns the scalar span-formatting body for this vector family.</summary>
    public static string TryFormatComponentBody(VectorSpec vector) => vector.Scalar.IsBool
        ? BoolFormatBody
        : "        return value.TryFormat(destination, out charsWritten, format, formatProvider);";

    /// <summary>Returns the scalar UTF-8 span-formatting body for this vector family.</summary>
    public static string TryFormatUtf8ComponentBody(VectorSpec vector) => vector.Scalar.IsBool
        ? BoolUtf8FormatBody
        : "        return value.TryFormat(destination, out bytesWritten, format, formatProvider);";

    /// <summary>Returns the scalar span-parsing body for this vector family.</summary>
    public static string TryParseComponentBody(VectorSpec vector) => vector.Scalar.IsBool
        ? BoolParseBody
        : $"        return {vector.Scalar.CSharpName}.TryParse(source, formatProvider, out value);";

    /// <summary>Returns the scalar UTF-8 span-parsing body for this vector family.</summary>
    public static string TryParseUtf8ComponentBody(VectorSpec vector) => vector.Scalar.IsBool
        ? BoolUtf8ParseBody
        : $"        return {vector.Scalar.CSharpName}.TryParse(source, formatProvider, out value);";

    private const string BoolFormatBody = """
        _ = format;
        _ = formatProvider;

        var text = value ? "True" : "False";
        if (!text.AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = text.Length;
        return true;
""";

    private const string BoolUtf8FormatBody = """
        _ = format;
        _ = formatProvider;

        var text = value ? "True"u8 : "False"u8;
        if (!text.TryCopyTo(destination))
        {
            bytesWritten = 0;
            return false;
        }

        bytesWritten = text.Length;
        return true;
""";

    private const string BoolParseBody = """
        _ = formatProvider;

        if (source.Equals("True".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (source.Equals("False".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = default;
        return false;
""";

    private const string BoolUtf8ParseBody = """
        _ = formatProvider;

        if (source.SequenceEqual("True"u8))
        {
            value = true;
            return true;
        }

        if (source.SequenceEqual("False"u8))
        {
            value = false;
            return true;
        }

        value = default;
        return false;
""";
}
