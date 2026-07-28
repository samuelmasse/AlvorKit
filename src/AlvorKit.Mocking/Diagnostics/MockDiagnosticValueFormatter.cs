namespace AlvorKit.Mocking;

/// <summary>Appends bounded values using only known framework formatting paths.</summary>
internal static class MockDiagnosticValueFormatter
{
    private const int MaximumStringLength = 80;

    /// <summary>Appends one bounded value without calling an override.</summary>
    internal static void AppendValue(
        StringBuilder message,
        object? value,
        Type? declaredType)
    {
        if (value is null)
        {
            message.Append("null");
            return;
        }

        var valueType = declaredType?.IsByRef == true
            ? declaredType.GetElementType()
            : declaredType;
        if (valueType?.IsPointer == true)
        {
            AppendPointer(message, value);
            return;
        }

        switch (value)
        {
            case string text:
                AppendQuotedString(message, text);
                return;
            case char character:
                message.Append('\'');
                AppendCharacter(message, character);
                message.Append('\'');
                return;
            case bool boolean:
                message.Append(boolean ? "true" : "false");
                return;
            case byte or sbyte or short or ushort or int or uint or long
                or ulong or float or double or decimal or nint or nuint:
                message.Append(
                    Convert.ToString(
                        value,
                        CultureInfo.InvariantCulture));
                return;
            case Array array:
                message.Append('<');
                AppendType(message, value.GetType());
                message
                    .Append(" length=")
                    .Append(array.Length)
                    .Append('>');
                return;
        }

        var type = value.GetType();
        if (type.IsEnum)
        {
            AppendEnum(message, value, type);
            return;
        }

        message.Append('<');
        var mocked = Mock.GetMocked(value);
        if (mocked is null)
        {
            AppendType(message, type);
        }
        else
        {
            message.Append("mock ");
            AppendType(message, mocked.Type.Type);
        }

        message.Append('>');
    }

    /// <summary>Appends a runtime type name without calling a user formatter.</summary>
    internal static void AppendType(
        StringBuilder message,
        Type type) =>
        message.Append(type.FullName ?? type.Name);

    private static void AppendPointer(
        StringBuilder message,
        object value)
    {
        message.Append("<pointer");
        if (value is nint signed)
        {
            message
                .Append(" 0x")
                .Append(
                    unchecked((nuint)signed)
                        .ToString(
                            "x",
                            CultureInfo.InvariantCulture));
        }
        else if (value is nuint unsigned)
        {
            message
                .Append(" 0x")
                .Append(
                    unsigned.ToString(
                        "x",
                        CultureInfo.InvariantCulture));
        }

        message.Append('>');
    }

    private static void AppendEnum(
        StringBuilder message,
        object value,
        Type type)
    {
        message.Append('<');
        AppendType(message, type);
        message.Append(" value=");
        var underlying = Enum.GetUnderlyingType(type);
        if (underlying == typeof(sbyte) ||
            underlying == typeof(short) ||
            underlying == typeof(int) ||
            underlying == typeof(long))
        {
            message.Append(
                Convert.ToInt64(
                    value,
                    CultureInfo.InvariantCulture));
        }
        else
        {
            message.Append(
                Convert.ToUInt64(
                    value,
                    CultureInfo.InvariantCulture));
        }

        message.Append('>');
    }

    private static void AppendQuotedString(
        StringBuilder message,
        string value)
    {
        message.Append('"');
        var count = Math.Min(
            value.Length,
            MaximumStringLength);
        for (var i = 0; i < count; i++)
            AppendCharacter(message, value[i]);

        if (value.Length > count)
            message.Append('…');
        message.Append('"');
    }

    private static void AppendCharacter(
        StringBuilder message,
        char value)
    {
        switch (value)
        {
            case '\\':
                message.Append(@"\\");
                break;
            case '"':
                message.Append("\\\"");
                break;
            case '\'':
                message.Append("\\'");
                break;
            case '\r':
                message.Append(@"\r");
                break;
            case '\n':
                message.Append(@"\n");
                break;
            case '\t':
                message.Append(@"\t");
                break;
            default:
                if (char.IsControl(value))
                {
                    message
                        .Append(@"\u")
                        .Append(
                            ((int)value).ToString(
                                "x4",
                                CultureInfo.InvariantCulture));
                }
                else
                {
                    message.Append(value);
                }

                break;
        }
    }
}
