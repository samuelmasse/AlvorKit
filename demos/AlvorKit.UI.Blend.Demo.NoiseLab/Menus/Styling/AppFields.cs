namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>App-scoped Blend form-control builders wired to the shared dropdown popup.</summary>
[App]
public class AppFields(
    AppStyle s,
    RootUiScale uiScale,
    RootSprites sprites,
    RootUiMouse uiMouse,
    RootKeyboard keyboard,
    AppDropdownMenu dropdown) : BlendFields(s, uiScale, sprites, uiMouse, keyboard, dropdown);
