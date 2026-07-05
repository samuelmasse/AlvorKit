namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>
/// UI-facing session state for the Noise Lab: view transform, seed, post options, and the dirty flag that
/// drives auto-regeneration. Menus read and mutate this; the noise field does the actual work.
/// </summary>
[App]
public class AppSession(AppNoiseField field)
{
    private bool dirty = true;

    /// <summary>Gets the noise field owning the graph, parameters, and texture.</summary>
    public AppNoiseField Field { get; } = field;

    /// <summary>Gets or sets whether parameter changes regenerate automatically each update.</summary>
    public bool Auto { get; set; } = true;

    /// <summary>Gets the FastNoise2 seed.</summary>
    public int Seed { get; private set; } = 12345;

    /// <summary>Gets or sets whether output is normalized to the generated min/max instead of [-1, 1].</summary>
    public bool Normalize { get; set; } = true;

    /// <summary>Gets or sets whether the ramp is applied inverted.</summary>
    public bool Invert { get; set; }

    /// <summary>Gets or sets the selected <see cref="AppRamps"/> index.</summary>
    public int RampIndex { get; set; }

    /// <summary>Gets or sets the sample-space pan offset in texture pixels.</summary>
    public Vec2 Offset { get; set; }

    /// <summary>Gets or sets the world-space distance between adjacent samples.</summary>
    public float Step { get; set; } = 1f;

    /// <summary>Gets or sets the sampled z slice.</summary>
    public float Z { get; set; } = 60f;

    /// <summary>Gets a revision that menus watch to rebuild structural UI (toolbar actives, parameter dock).</summary>
    public int UiRevision { get; private set; }

    /// <summary>Gets whether parameters changed since the last generation.</summary>
    public bool Dirty => dirty;

    /// <summary>Marks the current parameters as needing a regenerate.</summary>
    public void MarkDirty() => dirty = true;

    /// <summary>Regenerates now if parameters are dirty and Auto is enabled; call once per update.</summary>
    public void Update()
    {
        if (dirty && Auto)
            RegenerateNow();
    }

    /// <summary>Resizes the sample grid to the visible viewport area, marking the field dirty on change.</summary>
    public void ResizeView(int width, int height)
    {
        if (Field.Resize(width, height))
            MarkDirty();
    }

    /// <summary>Runs a generation with the current parameters and clears the dirty flag; waits for a sized viewport.</summary>
    public void RegenerateNow()
    {
        if (Field.Width == 0)
            return;

        Field.Generate(Seed, Offset, Step, Z, Normalize, Invert, RampIndex);
        dirty = false;
    }

    /// <summary>Sets the seed directly, marking the field dirty.</summary>
    public void SetSeed(int seed)
    {
        Seed = seed;
        MarkDirty();
    }

    /// <summary>Picks a new random seed, marking the field dirty.</summary>
    public void RandomizeSeed() => SetSeed(Random.Shared.Next());

    /// <summary>Toggles auto-regeneration and rebuilds revision-gated UI.</summary>
    public void ToggleAuto()
    {
        Auto = !Auto;
        RebuildUi();
        MarkDirty();
    }

    /// <summary>Swaps the fractal node, resetting its parameters and rebuilding the dock.</summary>
    public void SelectFractal(int index)
    {
        Field.SelectFractal(index);
        RebuildUi();
        MarkDirty();
    }

    /// <summary>Swaps the source node, resetting its parameters and rebuilding the dock.</summary>
    public void SelectSource(int index)
    {
        Field.SelectSource(index);
        RebuildUi();
        MarkDirty();
    }

    /// <summary>Bumps the UI revision so revision-gated menus rebuild.</summary>
    public void RebuildUi() => UiRevision++;
}
