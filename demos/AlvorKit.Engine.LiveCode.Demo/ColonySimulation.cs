namespace AlvorKit.Engine.LiveCode.Demo;

/// <summary>Normal per-frame behavior running inside one colony scope.</summary>
[Colony]
public sealed class ColonySimulation(ColonyGarden garden, ColonySky sky)
{
    /// <summary>Advances local orbital and atmospheric systems.</summary>
    public void Update(double delta)
    {
        garden.Update(delta);
        sky.Warp = Math.Clamp(
            sky.Warp + (float)Math.Sin(garden.Phase * 0.37) * (float)delta * 0.025f,
            0.02f,
            0.85f);
    }
}
