namespace AlvorKit.Engine;

/// <summary>Root-scoped named control binding table.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootControls(WindowLoop window)
{
    private readonly Controls controls = new(window);

    /// <summary>Gets controls that ran during the current control frame and their hit counts.</summary>
    public IReadOnlyDictionary<Control, int> Hits => controls.Hits;

    /// <summary>Gets or creates a named control.</summary>
    public Control this[string name] => controls[name];

    /// <summary>Clears current-frame control hit counts.</summary>
    public void Tick() => controls.Tick();
}
