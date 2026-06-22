namespace AlvorKit.Engine.Loop;

/// <summary>Loads root control bindings from TOML configuration text.</summary>
[Root]
public sealed class RootControlsToml(RootControls controls)
{
    /// <summary>Parses TOML text and binds every named control entry it contains.</summary>
    public void Load(string text)
    {
        var options = new TomlModelOptions { ConvertPropertyName = static name => name };
        var bindings = Toml.ToModel<Dictionary<string, KeyBinding>>(text, null, options);
        foreach (var binding in bindings)
            controls[binding.Key].Bind(binding.Value);
    }
}
