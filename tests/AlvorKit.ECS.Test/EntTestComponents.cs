namespace AlvorKit.ECS.Test;


public record struct FirstComponent;
public record struct SecondComponent;
public record struct ThirdComponent;

[Components]
public interface IEntTestComponents
{
    [ComponentToString] public int First { get; set; }
    public int Second { get; set; }
    [ComponentToString] public string? Third { get; set; }
    [ComponentToString] public EntObj MyEnt { get; set; }
}

