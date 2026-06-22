namespace AlvorKit.Engine.Loop;

/// <summary>Loads root control bindings from TOML configuration text.</summary>
[Root]
public sealed class RootControlsToml(RootControls controls)
{
    /// <summary>Reads TOML from a direct path or root <c>res</c> asset name and binds every named control entry it contains.</summary>
    public void AddFromFile(string file) => Load(File.ReadAllText(RootAssetFiles.Resolve(file)));

    /// <summary>Parses TOML text and binds every named control entry it contains.</summary>
    public void Load(string text)
    {
        var options = new TomlModelOptions { ConvertPropertyName = static name => name };
        var bindings = Toml.ToModel<Dictionary<string, KeyBinding>>(text, null, options);
        foreach (var binding in bindings)
            controls[ControlName(binding.Key)].Bind(binding.Value);
    }

    private static string ControlName(string key)
    {
        var dash = key.IndexOf('-');
        return dash < 0 ? key : key[..dash];
    }
}
