namespace AlvorKit.Engine.Loop;

/// <summary>Base class for root-loop extension logic ordered by priority.</summary>
public class Script
{
    /// <summary>Gets or sets the order used when scripts are sorted; lower values run first.</summary>
    public int Priority { get; set; }

    /// <summary>Gets or sets the draw surface for two-dimensional rendering, or <c>null</c> for the whole canvas.</summary>
    public Vec2? DrawArea { get; set; }

    /// <summary>Runs setup after the script is added to <see cref="RootScripts"/>.</summary>
    public virtual void Load() { }

    /// <summary>Runs teardown before the script leaves <see cref="RootScripts"/>.</summary>
    public virtual void Unload() { }

    /// <summary>Runs fixed or variable update work for the script.</summary>
    public virtual void Update(double delta) { }

    /// <summary>Runs frame work immediately before rendering.</summary>
    public virtual void Frame(double delta) { }

    /// <summary>Writes two-dimensional draw commands for the script.</summary>
    public virtual void Draw() { }

    /// <summary>Runs direct render work for the script.</summary>
    public virtual void Render() { }
}
