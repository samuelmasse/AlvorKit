namespace AlvorKit.Script.MathsGen;

/// <summary>Emits one generated vector swizzle source file.</summary>
internal static class SwizzleFileEmitter
{
    /// <summary>Returns swizzle source code for <paramref name="vector"/>.</summary>
    public static string Emit(VectorSpec vector)
    {
        var members = new MemberBlock();
        SwizzleEmitter.Emit(vector, members);
        return MathsTemplate.Render(
            "swizzle-file.cs.tmpl",
            ("TypeName", vector.TypeName),
            ("Members", members.ToString()));
    }
}
