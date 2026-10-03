namespace AlvorKit;

/// <summary>Supplies shared engine resources to Blend toolkits without retaining their styles or mounted controls.</summary>
[Root]
public class RootBlend(
    RootInter inter,
    RootUiScale scale,
    RootSprites sprites,
    RootKeyboard keyboard,
    RootUiMouse mouse)
{
    internal RootInter Inter => inter;
    internal RootUiScale Scale => scale;
    internal RootSprites Sprites => sprites;
    internal RootKeyboard Keyboard => keyboard;
    internal RootUiMouse Mouse => mouse;
}
