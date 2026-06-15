namespace AlvorKit.Script.Bindgen;

/// <summary>Tracks an enum member with source lines used to repair trailing comments.</summary>
/// <param name="Member">Binding member read from Clang.</param>
/// <param name="DeclarationLine">Source line where the enum member is declared.</param>
/// <param name="CommentLine">Source line where Clang attached the comment.</param>
internal record EnumMemberRow(BindingEnumMember Member, uint DeclarationLine, uint CommentLine);
