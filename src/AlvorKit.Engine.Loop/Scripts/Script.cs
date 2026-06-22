namespace AlvorKit.Engine.Loop;

/// <summary>Base class for root-loop extension logic ordered by priority.</summary>
public class Script : State
{
    /// <summary>Gets or sets the order used when scripts are sorted; lower values run first.</summary>
    public virtual float Order => Priority;

    /// <summary>Gets or sets the integer compatibility priority used by the default <see cref="Order"/> implementation.</summary>
    public int Priority { get; set; }
}
