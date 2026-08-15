namespace AlvorKit;

/// <summary>Describes one four-byte metadata-token placeholder in a generated body.</summary>
public sealed class InterceptionGenerationRelocation
{
    private readonly byte[] signature;

    /// <summary>Creates one exact metadata relocation.</summary>
    public InterceptionGenerationRelocation(
        InterceptionGenerationRelocationKind kind,
        uint bodyOffset,
        ReadOnlySpan<byte> signature,
        int parentToken = 0,
        string? memberName = null)
    {
        if (signature.IsEmpty)
            throw new ArgumentException("A metadata signature cannot be empty.", nameof(signature));
        if ((kind == InterceptionGenerationRelocationKind.MemberRef) !=
            !string.IsNullOrEmpty(memberName))
        {
            throw new ArgumentException(
                "Only MemberRef relocations require a non-empty member name.",
                nameof(memberName));
        }
        if (kind is InterceptionGenerationRelocationKind.MemberRef or
                InterceptionGenerationRelocationKind.MethodSpec)
        {
            if (parentToken == 0)
                throw new ArgumentOutOfRangeException(nameof(parentToken));
        }
        else if (parentToken != 0)
        {
            throw new ArgumentException(
                "This relocation kind does not accept a parent token.",
                nameof(parentToken));
        }

        Kind = kind;
        BodyOffset = bodyOffset;
        ParentToken = parentToken;
        MemberName = memberName;
        this.signature = signature.ToArray();
    }

    /// <summary>Gets the metadata token kind.</summary>
    public InterceptionGenerationRelocationKind Kind { get; }

    /// <summary>Gets the complete-body byte offset of the four-byte zero placeholder.</summary>
    public uint BodyOffset { get; }

    /// <summary>Gets the parent token used by MemberRef or MethodSpec emission.</summary>
    public int ParentToken { get; }

    /// <summary>Gets the UTF-8 encoded MemberRef name, or null for other kinds.</summary>
    public string? MemberName { get; }

    /// <summary>Gets a defensive view of the exact ECMA signature blob.</summary>
    public ReadOnlyMemory<byte> Signature => signature;
}
