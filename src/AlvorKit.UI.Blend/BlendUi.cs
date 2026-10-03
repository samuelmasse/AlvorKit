namespace AlvorKit;

/// <summary>
/// Composes reusable Blend styles and builders over shared engine resources and an explicitly chosen graphics owner.
/// Register the finished instance in its application scope; mounted controls retain their own interaction state.
/// </summary>
public class BlendUi
{
    private readonly BlendStyle s;
    private readonly BlendScrollView scrollView;
    private readonly BlendFields fields;
    private readonly BlendDropdownMenu dropdown;
    private readonly BlendTooltipMenu tooltip;

    /// <summary>Gets the standard recipes and appearance shared by every builder in this toolkit.</summary>
    public BlendStyle S => s;

    /// <summary>Gets the reusable scroll-view builder.</summary>
    public BlendScrollView ScrollView => scrollView;

    /// <summary>Gets the reusable form-control builders.</summary>
    public BlendFields Fields => fields;

    /// <summary>Gets the builder for independently mounted dropdown popups.</summary>
    public BlendDropdownMenu Dropdown => dropdown;

    /// <summary>Gets the mouse-following tooltip builder.</summary>
    public BlendTooltipMenu Tooltip => tooltip;

    /// <summary>Creates a toolkit with the default appearance and control textures owned by <paramref name="gl"/>.</summary>
    public BlendUi(RootBlend root, GlLayer gl) : this(root, gl, BlendPalette.Default, new BlendMetrics()) { }

    /// <summary>Creates a toolkit sharing the supplied appearance across its style and builders.</summary>
    public BlendUi(RootBlend root, GlLayer gl, BlendPalette palette, BlendMetrics metrics)
    {
        s = new(root, gl, palette, metrics);
        scrollView = new(s);
        fields = new(s, root.Scale, root.Sprites, root.Mouse, root.Keyboard);
        dropdown = new(root.Keyboard, s);
        tooltip = new(root.Mouse, s);
    }
}
