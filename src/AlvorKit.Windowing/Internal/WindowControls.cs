namespace AlvorKit.Windowing;

/// <summary>Internal table of named controls for a window loop.</summary>
internal sealed class WindowControls(WindowMouse mouse, WindowKeyboard keyboard)
{
    private readonly Dictionary<string, Control> controls = [];
    private readonly Dictionary<Control, int> hits = [];

    /// <summary>Gets current-frame hit counts.</summary>
    internal IReadOnlyDictionary<Control, int> Hits => hits;

    /// <summary>Gets or creates a named control.</summary>
    internal Control this[string name]
    {
        get
        {
            if (controls.TryGetValue(name, out var value))
                return value;

            var control = new Control(name, this, mouse, keyboard);
            controls.Add(name, control);
            return control;
        }
    }

    /// <summary>Records one run for a control this frame.</summary>
    internal void Hit(Control control)
    {
        if (hits.TryGetValue(control, out var hit))
            hits[control] = hit + 1;
        else hits.Add(control, 1);
    }

    /// <summary>Clears current-frame hit counts.</summary>
    internal void Tick() => hits.Clear();
}
