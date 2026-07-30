namespace AlvorKit.Interception.Performance.Fixture;

/// <summary>Encodes a same-module template method as a complete profiler replacement body.</summary>
internal static class ReflectionMethodBodyEncoder
{
    private const ushort FatFormat = 0x0003;
    private const ushort MoreSections = 0x0008;
    private const ushort InitializeLocals = 0x0010;
    private const ushort FatHeaderDwords = 3;

    /// <summary>Reads a template with no exception regions into CoreCLR fat-method format.</summary>
    internal static InterceptionMethodBody Read(MethodInfo method)
    {
        var body = method.GetMethodBody() ??
            throw new InvalidOperationException(
                "The template method has no IL body.");
        if (body.ExceptionHandlingClauses.Count != 0)
        {
            throw new NotSupportedException(
                "The performance template cannot contain exception regions.");
        }

        var il = body.GetILAsByteArray() ??
            throw new InvalidOperationException(
                "The template IL body is unavailable.");
        var bytes = new byte[12 + il.Length];
        var flags = (ushort)(
            FatFormat |
            (body.InitLocals ? InitializeLocals : 0) |
            (FatHeaderDwords << 12));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, flags);
        BinaryPrimitives.WriteUInt16LittleEndian(
            bytes.AsSpan(2),
            ((ushort)body.MaxStackSize));
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(4),
            il.Length);
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(8),
            body.LocalSignatureMetadataToken);
        il.CopyTo(bytes, 12);

        if ((flags & MoreSections) != 0)
        {
            throw new InvalidOperationException(
                "Unexpected extra method sections.");
        }

        return InterceptionMethodBody.FromRaw(bytes);
    }
}
