namespace AlvorKit;

/// <summary>Attaches the root UI runtime to the engine update and drawing phases.</summary>
public class UiSetup : IRootSetup
{
    public void Apply(RootScope root)
    {
        var scripts = root.Get<RootScripts>();
        var ui = root.Get<RootUiScript>();

        if (!scripts.Span.Contains(ui, ReferenceEqualityComparer.Instance))
            scripts.Add(ui);
    }
}
