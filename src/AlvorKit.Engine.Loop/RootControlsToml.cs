namespace AlvorKit.Engine.Loop;

/// <summary>Loads root control bindings from TOML configuration text.</summary>
[Root]
public class RootControlsToml(RootControls controls)
{
    /// <summary>Reads TOML from a direct path and binds every named control entry it contains.</summary>
    public void AddFromFile(string file)
    {
        var text = File.ReadAllText(file);
        var options = new TomlModelOptions { ConvertPropertyName = static name => name };
        var bindings = Toml.ToModel<Dictionary<string, KeyBinding>>(text, null, options);
        foreach (var key in bindings.Keys)
            controls[key.Split('-')[0]].Bind(bindings[key]);
    }
}
