namespace AlvorKit.Windowing;

/// <summary>Represents one named user action with one or more key or mouse bindings.</summary>
public sealed class Control
{
    private readonly string name;
    private readonly WindowControls controls;
    private readonly WindowMouse mouse;
    private readonly WindowKeyboard keyboard;
    private readonly HashSet<KeyBinding> bindings = [];

    /// <summary>Creates a control owned by a window control table.</summary>
    internal Control(string name, WindowControls controls, WindowMouse mouse, WindowKeyboard keyboard)
    {
        this.name = name;
        this.controls = controls;
        this.mouse = mouse;
        this.keyboard = keyboard;
    }

    /// <summary>Gets the control name.</summary>
    public string Name => name;

    /// <summary>Gets whether any binding is currently active.</summary>
    public bool IsActive
    {
        get
        {
            foreach (var binding in bindings)
            {
                if (IsBindingActive(binding))
                    return true;
            }

            return false;
        }
    }

    /// <summary>Adds a binding to this control.</summary>
    public void Bind(KeyBinding binding) => bindings.Add(binding);

    /// <summary>Removes a binding from this control.</summary>
    public void Unbind(KeyBinding binding) => bindings.Remove(binding);

    /// <summary>Runs the control and records a hit when active.</summary>
    public bool Run()
    {
        var active = IsActive;
        if (active)
            controls.Hit(this);

        return active;
    }

    private bool IsBindingActive(KeyBinding binding)
    {
        if (!ValidModifiers(binding))
            return false;

        if (binding.KeyDown.HasValue && keyboard.IsKeyDown(binding.KeyDown.Value))
            return true;

        if (binding.KeyPress.HasValue && keyboard.IsKeyPressed(binding.KeyPress.Value))
            return true;

        if (binding.KeyPressRepeat.HasValue && keyboard.IsKeyPressedRepeated(binding.KeyPressRepeat.Value))
            return true;

        if (binding.MouseScroll.HasValue)
        {
            if (binding.MouseScroll.Value == MouseScrollDirection.Up && mouse.Wheel.Y > 0)
                return true;
            if (binding.MouseScroll.Value == MouseScrollDirection.Down && mouse.Wheel.Y < 0)
                return true;
        }

        return !binding.KeyDown.HasValue &&
            !binding.KeyPress.HasValue &&
            !binding.KeyPressRepeat.HasValue &&
            !binding.MouseScroll.HasValue;
    }

    private bool ValidModifiers(KeyBinding binding) =>
        ValidModifier(IsControlDown(), binding.Control) &&
        ValidModifier(IsShiftDown(), binding.Shift) &&
        ValidModifier(IsAltDown(), binding.Alt);

    private bool IsControlDown() => keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);

    private bool IsShiftDown() => keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);

    private bool IsAltDown() => keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);

    private static bool ValidModifier(bool down, KeyModifierState state) =>
        state == KeyModifierState.Any || (state == KeyModifierState.Down ? down : !down);
}
