namespace AlvorKit.Engine.Loop;

/// <summary>Base class for a game state owned by the current root loop.</summary>
public class State
{
    private Vec2? drawArea;

    /// <summary>Gets or sets the draw surface for two-dimensional rendering, or <c>null</c> for the whole canvas.</summary>
    public virtual Vec2? DrawArea { get => drawArea; set => drawArea = value; }

    /// <summary>Runs one-time setup after the state becomes current.</summary>
    public virtual void Load() { }

    /// <summary>Runs cleanup before the state is replaced or the root loop unloads.</summary>
    public virtual void Unload() { }

    /// <summary>Runs fixed or variable update work for the state.</summary>
    public virtual void Update(double delta) { }

    /// <summary>Runs frame work immediately before rendering.</summary>
    public virtual void Frame(double delta) { }

    /// <summary>Writes two-dimensional draw commands for the state.</summary>
    public virtual void Draw() { }

    /// <summary>Runs direct render work for the state.</summary>
    public virtual void Render() { }
}
