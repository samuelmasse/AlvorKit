namespace AlvorKit;

/// <summary>Owns the engine window's UI runtime, default surface, and shared input state until root shutdown.</summary>
[Root]
public class RootUi : UiRuntime
{
    public RootUi(RootCanvas canvas, RootKeyboard keyboard, RootMouse mouse, RootSprites sprites, RootScale scale, RootUnload unload)
        : base(canvas, keyboard, mouse, sprites.Batch, scale.Scale) => unload.Add(Dispose);
}
