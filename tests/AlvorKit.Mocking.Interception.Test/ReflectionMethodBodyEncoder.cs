namespace AlvorKit;

/// <summary>Encodes one same-module test template as a complete raw profiler body.</summary>
internal static class ReflectionMethodBodyEncoder
{
    /// <summary>The CoreCLR fat method-body format flag.</summary>
    private const ushort FatFormat = 0x0003;

    /// <summary>The method-body flag for additional sections.</summary>
    private const ushort MoreSections = 0x0008;

    /// <summary>The method-body flag that initializes locals.</summary>
    private const ushort InitializeLocals = 0x0010;

    /// <summary>The number of 32-bit words in a fat method header.</summary>
    private const ushort FatHeaderDwords = 3;

    /// <summary>Reads a template without exception regions into CoreCLR fat-method format.</summary>
    internal static InterceptionMethodBody Read(MethodInfo method)
    {
        var body = method.GetMethodBody() ??
            throw new InvalidOperationException(
                "The caller template has no IL body.");
        if (body.ExceptionHandlingClauses.Count != 0)
        {
            throw new NotSupportedException(
                "The caller template cannot contain exception regions.");
        }

        var il = body.GetILAsByteArray() ??
            throw new InvalidOperationException(
                "The caller template IL is unavailable.");
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
