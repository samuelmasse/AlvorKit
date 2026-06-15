namespace AlvorKit.Script.Bindgen;

/// <summary>Rules for building a managed enum from macro constants.</summary>
public sealed class EnumGroup
{
    /// <summary>Shared native prefix to collect and strip, unless <see cref="Members"/> is explicit.</summary>
    public string Prefix { get; set; } = "";

    /// <summary>Explicit native constant names for groups without a clean shared prefix.</summary>
    public string[]? Members { get; set; }

    /// <summary>Native constants to ignore when prefix collection would pull in unrelated names.</summary>
    public string[] Exclude { get; set; } = [];

    /// <summary>Native suffix to drop from member names, such as a repeated category suffix.</summary>
    public string Suffix { get; set; } = "";

    /// <summary>Whether the generated enum represents bit flags.</summary>
    public bool Flags { get; set; }

    /// <summary>Prefix for member names that would otherwise start with a digit.</summary>
    public string DigitPrefix { get; set; } = "Num";
}
