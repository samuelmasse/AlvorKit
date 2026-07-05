namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>App-scoped Blend tooltip layer; parameter fields carry metadata descriptions as tooltips.</summary>
[App]
public class AppTooltipMenu(RootUiMouse uiMouse, AppStyle s) : BlendTooltipMenu(uiMouse, s);
