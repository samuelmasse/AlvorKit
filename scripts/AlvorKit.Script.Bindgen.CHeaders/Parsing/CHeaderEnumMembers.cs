namespace AlvorKit.Script.Bindgen;

/// <summary>Reads enum members and fixes comment placement from common C trailing-comment style.</summary>
internal static class CHeaderEnumMembers
{
    /// <summary>Reads all enum members from a Clang enum declaration.</summary>
    public static List<BindingEnumMember> Read(BindgenConfig config, EnumDecl enumDecl)
    {
        var rows = enumDecl.Enumerators
            .Select(enumerator =>
            {
                enumerator.Location.GetExpansionLocation(out _, out var declarationLine, out _, out _);
                enumerator.Handle.CommentRange.Start.GetExpansionLocation(out _, out var commentLine, out _, out _);
                return new EnumMemberRow(
                    new(
                        CSharpName.FromNativeIdentifier(enumerator.Name, config.Prefix, config.DigitNamePrefix),
                        enumerator.Handle.EnumConstantDeclValue,
                        XmlDocComment.Member(enumerator.Handle.RawCommentText.ToString())),
                    declarationLine,
                    commentLine);
            })
            .ToList();
        FixTrailingMemberComments(rows);
        return [.. rows.Select(row => row.Member)];
    }

    /// <summary>Moves a trailing comment from the following enum member to the previous one.</summary>
    private static void FixTrailingMemberComments(List<EnumMemberRow> rows)
    {
        for (var i = 1; i < rows.Count; i++)
        {
            if (rows[i].Member.Documentation is null
                || rows[i].CommentLine != rows[i - 1].DeclarationLine
                || rows[i].DeclarationLine == rows[i - 1].DeclarationLine)
                continue;

            if (rows[i - 1].Member.Documentation is null)
                rows[i - 1] = rows[i - 1] with { Member = rows[i - 1].Member with { Documentation = rows[i].Member.Documentation } };
            rows[i] = rows[i] with { Member = rows[i].Member with { Documentation = null } };
        }
    }
}
