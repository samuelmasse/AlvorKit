namespace AlvorKit.Script.Bindgen;

/// <summary>Span overload configuration for one native library binding.</summary>
public sealed partial class BindgenConfig
{
    /// <summary>Whether span convenience overloads should be generated.</summary>
    public bool SpanExtensions { get; set; }

    /// <summary>Native functions skipped when generating span overloads.</summary>
    public Dictionary<string, string> SpanSkip { get; set; } = [];

    /// <summary>Maps native functions to parameters used by generated span overloads.</summary>
    public Dictionary<string, string[]> SpanParams { get; set; } = [];

    /// <summary>Pointer-plus-count returns that should expose read-only span convenience views.</summary>
    public Dictionary<string, string> SpanReturns { get; set; } = [];
}
