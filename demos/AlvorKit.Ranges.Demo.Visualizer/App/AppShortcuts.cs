namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Maps keyboard and mouse shortcuts onto visualizer app and UI commands.</summary>
[App]
public class AppShortcuts(
    RootKeyboard keyboard,
    RootMouse mouse,
    AppSession session)
{
    public void Update()
    {
        var shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        var wheel = mouse.Wheel.Y;

        if (wheel > 0)
            session.PreviousScenario();
        else if (wheel < 0)
            session.NextScenario();

        if (session.ScenarioPickerOpen && keyboard.IsKeyPressed(Keys.Escape))
            session.CloseScenarioPicker();

        if (keyboard.IsKeyPressed(Keys.Space))
            session.TogglePlayback();
        if (keyboard.IsKeyPressed(Keys.L))
            session.ToggleLabels();
        if (keyboard.IsKeyPressed(Keys.A))
            session.TogglePadding();
        if (keyboard.IsKeyPressedRepeated(Keys.Right))
            session.StepForward();
        if (keyboard.IsKeyPressedRepeated(Keys.Left))
            session.StepBackward();
        if (keyboard.IsKeyPressed(Keys.R))
            session.ResetScenario();
        if (keyboard.IsKeyPressed(Keys.P))
            session.JumpToPack();
        if (keyboard.IsKeyPressed(Keys.Tab))
        {
            if (shift)
                session.PreviousScenario();
            else
                session.NextScenario();
        }

        if (!shift && keyboard.IsKeyPressedRepeated(Keys.Equal))
            session.Faster();

        if (!shift && keyboard.IsKeyPressedRepeated(Keys.Minus))
            session.Slower();
    }
}
