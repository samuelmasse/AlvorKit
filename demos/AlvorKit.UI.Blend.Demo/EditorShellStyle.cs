namespace AlvorKit.UI.Blend.Demo;

/// <summary>Loads the Blend package style with the shared Inter font asset.</summary>
[App]
public class EditorShellStyle(RootFonts fonts, RootGl gl, RootUiScale scale) : BlendStyle(
    fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(EditorShellStyle)), "fonts", "Inter.ttf") }),
    fonts.Open(new() { File = Path.Combine(ProjectRoot.ResDirectory(typeof(EditorShellStyle)), "fonts", "Inter-SemiBold.ttf") }),
    new BlendControlChrome(gl, scale));
