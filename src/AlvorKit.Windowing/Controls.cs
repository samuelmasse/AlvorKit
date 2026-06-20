namespace AlvorKit.Windowing;

/// <summary>Stores named control bindings for a window loop.</summary>
public sealed class Controls(WindowLoop window)
{
    /// <summary>Gets controls that ran during the current control frame and their hit counts.</summary>
    public IReadOnlyDictionary<Control, int> Hits => window.Controls.Hits;

    /// <summary>Gets or creates a named control.</summary>
    public Control this[string name] => window.Controls[name];

    /// <summary>Clears current-frame control hit counts.</summary>
    public void Tick() => window.Controls.Tick();
}
