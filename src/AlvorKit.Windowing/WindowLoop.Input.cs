namespace AlvorKit.Windowing;

public sealed partial class WindowLoop
{
    private void TickInputState()
    {
        mouse.Tick();
        keyboard.Tick();
        text.Tick();
        controls.Tick();
    }
}
