namespace AlvorKit;

/// <summary>Adds UI update, drawing, and shutdown to the root loop.</summary>
public class UiSetup : IRootSetup
{
    public void Apply(RootScope root) => root.Get<RootScripts>().Add(root.Get<RootUiScript>());
}
