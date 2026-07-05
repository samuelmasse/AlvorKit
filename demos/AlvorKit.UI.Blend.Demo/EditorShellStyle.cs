namespace AlvorKit.UI.Blend.Demo;

/// <summary>Blend-backed UI style for the editor-shell demo.</summary>
[App]
public class EditorShellStyle(RootInter inter, RootGl gl, RootUiScale scale, RootKeyboard keyboard)
    : BlendStyle(inter, gl, scale, keyboard);
