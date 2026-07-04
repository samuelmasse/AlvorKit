namespace AlvorKit.Ranges.Demo.Visualizer;

[App]
public class AppUiScaleMenu(RootKeyboard keyboard, AppUiScale uiScale)
{
    public void Create(EntMut root)
    {
        root.Mutate().OnUpdateF(() =>
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
