namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Data resolved in a temporary nested scope during a live inspection.</summary>
[Probe]
public sealed class ProbeTelemetry(ColonyGarden garden, ColonySky sky)
{
    /// <summary>Describes the parent colony without changing it.</summary>
    public string Read() =>
        $"{garden.Identity.Name}: {garden.SporeCount} organisms, {sky.Weather}, warp {sky.Warp:0.00}";
}
