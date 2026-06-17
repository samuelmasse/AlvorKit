namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Declares generated component accessors used by the ECS benchmark demo.</summary>
[Components]
public interface IEcsBenchComponents
{
    /// <summary>A hot integer component used by get, has, set, and unset benchmark paths.</summary>
    int First { get; set; }

    /// <summary>A second integer component used to keep benchmark entities shaped like ordinary game objects.</summary>
    int Second { get; set; }
}
