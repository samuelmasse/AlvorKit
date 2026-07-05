namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>App-scoped Blend tooltip layer for hovered visualizer nodes.</summary>
[App]
public class AppTooltipMenu(RootUiMouse uiMouse, AppStyle s) : BlendTooltipMenu(uiMouse, s);
