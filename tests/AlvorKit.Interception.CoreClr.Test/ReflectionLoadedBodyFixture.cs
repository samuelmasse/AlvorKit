using System.Buffers.Binary;
using System.Reflection;

namespace AlvorKit.Interception.CoreClr.Test;

/// <summary>Copies reflection IL and exception clauses into one complete fat method body.</summary>
internal static class ReflectionLoadedBodyFixture
{
    /// <summary>Reads one reflection method body into the loaded-body wire format.</summary>
    internal static byte[] Read(MethodBase method)
    {
        MethodBody body = method.GetMethodBody() ??
            throw new InvalidOperationException(
                $"Method '{method}' has no managed body.");
        byte[] code = body.GetILAsByteArray() ??
            throw new InvalidOperationException(
                $"Method '{method}' has no readable IL.");
        var clauses = body.ExceptionHandlingClauses;
        int sectionSize = clauses.Count == 0
            ? 0
            : 4 + clauses.Count * 24;
        int sectionStart = clauses.Count == 0
            ? 12 + code.Length
            : (12 + code.Length + 3) & ~3;
        var bytes = new byte[sectionStart + sectionSize];
        ushort flags = (ushort)(
            0x0003 |
            (body.InitLocals ? 0x0010 : 0) |
            (clauses.Count == 0 ? 0 : 0x0008) |
            (3 << 12));
        BinaryPrimitives.WriteUInt16LittleEndian(bytes, flags);
        BinaryPrimitives.WriteUInt16LittleEndian(
            bytes.AsSpan(2),
            checked((ushort)body.MaxStackSize));
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(4),
            code.Length);
        BinaryPrimitives.WriteInt32LittleEndian(
            bytes.AsSpan(8),
            body.LocalSignatureMetadataToken);
        code.CopyTo(bytes, 12);
        if (clauses.Count == 0)
            return bytes;

        Span<byte> section = bytes.AsSpan(sectionStart);
        section[0] = 0x41;
        section[1] = checked((byte)sectionSize);
        section[2] = checked((byte)(sectionSize >> 8));
        section[3] = checked((byte)(sectionSize >> 16));
        for (int index = 0; index < clauses.Count; ++index)
        {
            ExceptionHandlingClause clause = clauses[index];
            Span<byte> encoded = section.Slice(4 + index * 24, 24);
            BinaryPrimitives.WriteUInt32LittleEndian(
                encoded,
                (uint)clause.Flags);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[4..],
                clause.TryOffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[8..],
                clause.TryLength);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[12..],
                clause.HandlerOffset);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[16..],
                clause.HandlerLength);
            BinaryPrimitives.WriteInt32LittleEndian(
                encoded[20..],
                clause.Flags == ExceptionHandlingClauseOptions.Filter
                    ? clause.FilterOffset
                    : clause.Flags ==
                        ExceptionHandlingClauseOptions.Clause
                        ? clause.CatchType!.MetadataToken
                        : 0);
        }

        return bytes;
    }
}
