namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>App-scoped dropdown popup layer shared by every Noise Lab dropdown field.</summary>
[App]
public class AppDropdownMenu(RootKeyboard keyboard, AppStyle s) : BlendDropdownMenu(keyboard, s);
