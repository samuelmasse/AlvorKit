namespace AlvorKit.Engine.Loop;

/// <summary>Base class for root-loop extension logic ordered by execution order.</summary>
public class Script : State
{
    /// <summary>Gets the order used when scripts are sorted; lower values run first.</summary>
    public virtual float Order => 0;

    /// <summary>Gets the draw surface for two-dimensional rendering, or <c>null</c> for the whole canvas.</summary>
    public virtual Vec2? DrawArea => null;
}
