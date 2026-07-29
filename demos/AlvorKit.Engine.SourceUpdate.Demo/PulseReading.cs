namespace AlvorKit.Engine.SourceUpdate.Demo;

/// <summary>Visible result produced by the editable method.</summary>
public readonly record struct PulseReading(
    string Label,
    Vec4 Color,
    float Energy,
    int Updates);
