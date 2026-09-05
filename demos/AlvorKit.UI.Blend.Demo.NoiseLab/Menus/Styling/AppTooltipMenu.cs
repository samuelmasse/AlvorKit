namespace AlvorKit;

/// <summary>App-scoped Blend tooltip layer; parameter fields carry authored descriptions as tooltips.</summary>
[App]
public class AppTooltipMenu(RootUiMouse uiMouse, AppStyle s) : BlendTooltipMenu(uiMouse, s);
