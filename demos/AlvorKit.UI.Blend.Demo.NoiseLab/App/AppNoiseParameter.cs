namespace AlvorKit;

/// <summary>Chooses the Blend control used to edit a typed noise parameter.</summary>
public enum AppNoiseParameterKind
{
    /// <summary>A floating-point variable, with a slider when bounded.</summary>
    Float,
    /// <summary>An integer variable whose edits are rounded before writing.</summary>
    Int,
    /// <summary>A dropdown index mapped to a typed enum value.</summary>
    Enum,
    /// <summary>A scalar value assigned to a hybrid input.</summary>
    Hybrid,
}

/// <summary>One authored control bound to a typed node setter; its value is the last successfully applied edit.</summary>
public record AppNoiseParameter(
    AppNoiseParameterKind Kind,
    string Name,
    string Tooltip,
    float Min,
    float Max,
    IReadOnlyList<BlendDropdownItem> EnumItems,
    Action<float> Write)
{
    /// <summary>Gets whether the authored bounds support a slider.</summary>
    public bool HasRange => float.IsFinite(Min) && float.IsFinite(Max) && Min < Max;
    /// <summary>Gets the last value successfully applied to the typed node.</summary>
    public float Value { get; private set; }

    /// <summary>Applies an edit to the node before updating the displayed value.</summary>
    public void Set(float value)
    {
        var applied = Kind is AppNoiseParameterKind.Int or AppNoiseParameterKind.Enum ? MathF.Round(value) : value;
        Write(applied);
        Value = applied;
    }

    /// <summary>Binds a floating-point control and applies its initial value.</summary>
    public static AppNoiseParameter Float(
        FnGraphNode node, FnFloatVariable variable, string name, string tooltip, float value, float min, float max) =>
        Create(AppNoiseParameterKind.Float, name, tooltip, value, min, max, [], next => node.Float(variable, next));

    /// <summary>Binds an integer control and applies its initial value.</summary>
    public static AppNoiseParameter Integer(
        FnGraphNode node, FnIntegerVariable variable, string name, string tooltip, int value, float min, float max) =>
        Create(AppNoiseParameterKind.Int, name, tooltip, value, min, max, [], next => node.Integer(variable, (int)next));

    /// <summary>Binds an unbounded scalar control to a typed hybrid input.</summary>
    public static AppNoiseParameter Hybrid(FnGraphNode node, FnHybrid hybrid, string name, string tooltip, float value) =>
        Create(AppNoiseParameterKind.Hybrid, name, tooltip, value, float.NegativeInfinity, float.PositiveInfinity,
            [], next => node.Hybrid(hybrid, next));

    /// <summary>Builds dropdown choices from the enum and maps their ordinal indices back to enum values.</summary>
    public static AppNoiseParameter Choice<T>(string name, string tooltip, T value, Action<T> write) where T : struct, Enum
    {
        var options = Enum.GetValues<T>();
        var items = options.Select(option => new BlendDropdownItem(option.ToString())).ToArray();
        var selected = options.AsSpan().IndexOf(value);
        return Create(AppNoiseParameterKind.Enum, name, tooltip, selected, 0, options.Length - 1,
            items, next => write(options[(int)next]));
    }

    /// <summary>Creates a control whose displayed default has already been written to the node.</summary>
    private static AppNoiseParameter Create(
        AppNoiseParameterKind kind, string name, string tooltip, float value, float min, float max,
        IReadOnlyList<BlendDropdownItem> items, Action<float> write)
    {
        var parameter = new AppNoiseParameter(kind, name, tooltip, min, max, items, write);
        parameter.Set(value);
        return parameter;
    }
}
