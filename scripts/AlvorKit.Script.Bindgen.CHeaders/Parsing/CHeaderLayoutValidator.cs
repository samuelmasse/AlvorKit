namespace AlvorKit.Script.Bindgen;

/// <summary>Validates that generated structs can use natural CLR layout for the target ABI.</summary>
internal static class CHeaderLayoutValidator
{
    /// <summary>Throws when a native record has bitfields or non-natural field offsets.</summary>
    public static void ValidateNaturalRecordLayout(CHeaderParseState state, RecordDecl record, string targetName)
    {
        var isUnion = record.Handle.Kind == CXCursorKind.CXCursor_UnionDecl;
        var nextNaturalOffset = 0L;

        foreach (var field in record.Fields)
        {
            if (field.Handle.IsBitField)
                throw new InvalidOperationException($"{targetName}: {record.Name}.{field.Name} is a bitfield");

            var bitOffset = field.Handle.OffsetOfField;
            if (bitOffset % 8 != 0)
                throw new InvalidOperationException($"{targetName}: {record.Name}.{field.Name} is a bitfield");

            var actualOffset = bitOffset / 8;
            var fieldSize = field.Type.Handle.SizeOf;
            var fieldAlignment = field.Type.Handle.AlignOf;
            var expectedOffset = isUnion ? 0 : Align(nextNaturalOffset, fieldAlignment);
            if (actualOffset != expectedOffset)
                throw new InvalidOperationException(
                    $"{targetName}: {record.Name}.{field.Name} at offset {actualOffset}, natural layout expects {expectedOffset} - needs manual handling");
            nextNaturalOffset = expectedOffset + fieldSize;

            if (field.Type.CanonicalType.Handle.kind == CXTypeKind.CXType_Record
                && state.RecordByNativeName.TryGetValue(CHeaderNameMapper.CleanTypeSpelling(field.Type.Handle), out var nestedRecord))
                ValidateNaturalRecordLayout(state, nestedRecord, targetName);
        }
    }

    /// <summary>Rounds an offset up to the next alignment boundary.</summary>
    private static long Align(long offset, long alignment) => (offset + alignment - 1) / alignment * alignment;
}
