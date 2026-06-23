namespace AlvorKit.Script.MathsGen;

/// <summary>Accumulates generated member fragments with normalized line endings.</summary>
internal sealed class MemberBlock
{
    /// <summary>The underlying text builder.</summary>
    private readonly StringBuilder builder = new();

    /// <summary>Appends one rendered member fragment.</summary>
    public void Append(string text) => builder.Append(text.ReplaceLineEndings());

    /// <summary>Returns the accumulated members without trailing blank lines.</summary>
    public override string ToString() => builder.ToString().TrimEnd();
}
