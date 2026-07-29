namespace AlvorKit.LiveCode;

/// <summary>Records one project-wide C# import for LiveCode submission compilation.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class LiveCodeGlobalUsingAttribute(string clause) : Attribute
{
    /// <summary>Gets the C# clause following <c>global using</c> and preceding its semicolon.</summary>
    public string Clause { get; } = clause;
}
