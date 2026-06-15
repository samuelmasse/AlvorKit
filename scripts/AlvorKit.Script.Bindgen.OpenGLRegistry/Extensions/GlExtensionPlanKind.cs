namespace AlvorKit.Script.Bindgen;

/// <summary>Transformation chosen for one generated combined-overload parameter.</summary>
internal enum GlExtensionPlanKind
{
    /// <summary>The raw parameter remains in the overload signature.</summary>
    Keep,

    /// <summary>The pointer parameter becomes a typed span.</summary>
    SpanTyped,

    /// <summary>The void pointer parameter becomes a generic span whose byte length is inferred.</summary>
    SpanGenericSized,

    /// <summary>The void pointer parameter becomes a generic span without dropping a size parameter.</summary>
    SpanGenericUnsized,

    /// <summary>The GLchar pointer parameter becomes a managed string.</summary>
    StringIn,

    /// <summary>The GLchar pointer array parameter becomes a span of managed strings.</summary>
    StringArray,

    /// <summary>The raw parameter is supplied automatically and removed from the overload signature.</summary>
    Dropped
}
