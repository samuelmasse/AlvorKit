namespace AlvorKit.Engine.Loop;

/// <summary>Base class for a game state owned by the current root loop.</summary>
public class State
{
    /// <summary>Gets or sets the draw surface for two-dimensional rendering, or <c>null</c> for the whole canvas.</summary>
    public Vec2? DrawArea { get; set; }

    /// <summary>Runs fixed or variable update work for the state.</summary>
    public virtual void Update(double delta) { }

    /// <summary>Runs frame work immediately before rendering.</summary>
    public virtual void Frame(double delta) { }

    /// <summary>Writes two-dimensional draw commands for the state.</summary>
    public virtual void Draw() { }

    /// <summary>Runs direct render work for the state.</summary>
    public virtual void Render() { }
}
