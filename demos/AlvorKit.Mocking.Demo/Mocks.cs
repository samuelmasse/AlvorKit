namespace AlvorKit.Mocking.Demo;

/// <summary>Rendering surface used to demonstrate interface mocking and event raising.</summary>
public interface IRenderer
{
    /// <summary>Raised when the surface size changes.</summary>
    event Action<SurfaceSize> Resized;

    /// <summary>Draws a sprite on a logical layer.</summary>
    bool Draw(string sprite, int layer);
}

/// <summary>Logical surface size passed through a mocked event.</summary>
public readonly record struct SurfaceSize(int Width, int Height);

/// <summary>Concrete input device used to demonstrate class mocking.</summary>
public class MouseInput
{
    /// <summary>Returns whether the requested button is currently pressed.</summary>
    public bool IsPressed(MouseButton button)
    {
        _ = button;
        return false;
    }

    /// <summary>Returns the normalized value for one input axis.</summary>
    public float Axis(int axis)
    {
        _ = axis;
        return 0f;
    }
}

/// <summary>Mouse buttons used by the demo input device.</summary>
public enum MouseButton
{
    /// <summary>The primary pointer button.</summary>
    Primary,

    /// <summary>The secondary pointer button.</summary>
    Secondary,
}

/// <summary>Concrete object used to demonstrate partial instance mocking.</summary>
public class Counter
{
    private int current;

    /// <summary>Gets the current counter value.</summary>
    public int Current => current;

    /// <summary>Advances and returns the counter value.</summary>
    public int Next() => ++current;
}

/// <summary>Concrete object used to demonstrate explicit constructed generic method setup.</summary>
public class GenericFormatter
{
    /// <summary>Formats a value using the concrete generic method selected by the caller.</summary>
    public string Format<T>(T value) => value?.ToString() ?? string.Empty;
}
