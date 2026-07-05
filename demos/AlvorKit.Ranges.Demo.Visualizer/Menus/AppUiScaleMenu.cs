namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Binds the Shift +/- keyboard shortcuts to ui-scale changes.</summary>
[App]
public class AppUiScaleMenu(RootKeyboard keyboard, AppUiScale uiScale)
{
    public void Create(EntMut root)
    {
        Node(root)
            .OnUpdateF(() =>
            {
                if (!keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.RightShift))
                    return;

                if (keyboard.IsKeyPressedRepeated(Keys.Equal))
                    uiScale.ScaleUp();

                if (keyboard.IsKeyPressedRepeated(Keys.Minus))
                    uiScale.ScaleDown();
            });
    }
}
