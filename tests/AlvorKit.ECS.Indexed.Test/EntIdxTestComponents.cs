namespace AlvorKit.ECS.Indexed.Test;

public abstract class EntIdxManualIntComponent : IComponent
{
    public static EntComponent Component => new(typeof(int), typeof(EntIdxManualIntComponent));
}

[Components]
public interface IEntIdxTestComponents
{
    public bool IsLoaded { get; set; }
    public bool IsThing { get; set; }
    public bool IsOther { get; set; }
    public bool IsGateA { get; set; }
    public bool IsGateB { get; set; }
    public Guid Id { get; set; }
    public int Value { get; set; }
    public string? Name { get; set; }
}

