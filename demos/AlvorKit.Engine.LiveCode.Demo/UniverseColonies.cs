namespace AlvorKit;

/// <summary>Owns all simultaneously active colony lifetimes beneath the engine root.</summary>
[Root]
public sealed class UniverseColonies(
    RootScope root,
    InjectorScopeGraph graph,
    UniverseClock clock)
{
    private readonly List<UniverseColony> colonies = [];
    private int selectedIndex;

    /// <summary>Gets shared constellation time.</summary>
    public UniverseClock Clock => clock;

    /// <summary>Gets active colonies without allocating.</summary>
    public ReadOnlySpan<UniverseColony> Span => CollectionsMarshal.AsSpan(colonies);

    /// <summary>Gets the selected colony, if one exists.</summary>
    public UniverseColony? Selected =>
        colonies.Count == 0 ? null : colonies[Math.Clamp(selectedIndex, 0, colonies.Count - 1)];

    /// <summary>Gets or sets the brightness of inter-scope links.</summary>
    public float NetworkIntensity { get; set; } = 0.35f;

    /// <summary>Gets or sets the network link color.</summary>
    public Vec4 NetworkColor { get; set; } = (0.3f, 0.65f, 1f, 0.5f);

    /// <summary>Gets or sets the latest human or agent action shown in the observatory.</summary>
    public string LastIntervention { get; set; } = "Awaiting a live-code intervention...";

    /// <summary>Creates and seeds another independently addressable colony scope.</summary>
    public UniverseColony Open(
        string name,
        string sigil,
        Vec2 anchor,
        Vec4 primary,
        Vec4 secondary)
    {
        var scope = graph.Scope<ColonyScope>(root, name);
        var identity = scope.Get<ColonyIdentity>();
        identity.Name = name;
        identity.Sigil = sigil;
        identity.BornAtTick = clock.Tick;

        var garden = scope.Get<ColonyGarden>();
        garden.Anchor = anchor;
        garden.Primary = primary;
        garden.Secondary = secondary;
        garden.Phase = colonies.Count * 1.71;

        var sky = scope.Get<ColonySky>();
        sky.Halo = (primary.X, primary.Y, primary.Z, 0.5f);

        var colony = new UniverseColony(
            graph.GetId(scope),
            scope,
            identity,
            garden,
            sky,
            scope.Get<ColonySimulation>());
        colonies.Add(colony);
        return colony;
    }

    /// <summary>Creates the dramatic fourth colony used by the root LiveCode sample.</summary>
    public UniverseColony OpenAgentColony()
    {
        var existing = Find("Agent Aurora");
        if (existing is not null)
            return existing;

        var colony = Open(
            "Agent Aurora",
            "A",
            (0.5f, 0.48f),
            (1f, 0.24f, 0.74f, 1f),
            (0.18f, 1f, 0.84f, 1f));
        colony.Garden.Radius = 78f;
        colony.Garden.OrbitRadius = 132f;
        colony.Garden.SporeCount = 44;
        colony.Garden.RotationSpeed = -1.25f;
        colony.Garden.Form = "agent-authored";
        colony.Garden.Burst(2.4f);
        colony.Sky.Halo = (0.75f, 0.2f, 1f, 0.72f);
        colony.Sky.Warp = 0.62f;
        colony.Sky.Weather = "synthetic aurora";
        Select(colony);
        return colony;
    }

    /// <summary>Ends one exact colony lifetime after removing it from the render loop.</summary>
    public void Close(string name)
    {
        for (var index = 0; index < colonies.Count; index++)
        {
            var colony = colonies[index];
            if (!string.Equals(colony.Name, name, StringComparison.Ordinal))
                continue;

            colonies.RemoveAt(index);
            graph.End(colony.Scope);
            selectedIndex = Math.Clamp(selectedIndex, 0, Math.Max(0, colonies.Count - 1));
            LastIntervention = $"Ended {name}; its scope remains as a lifecycle tombstone.";
            return;
        }

        throw new InvalidOperationException($"No active colony is named '{name}'.");
    }

    /// <summary>Advances shared time and every currently active colony.</summary>
    public void Update(double delta)
    {
        clock.Advance(delta);
        foreach (var colony in Span)
            colony.Simulation.Update(delta);
    }

    /// <summary>Selects the next active scope.</summary>
    public void SelectNext()
    {
        if (colonies.Count > 0)
            selectedIndex = (selectedIndex + 1) % colonies.Count;
    }

    /// <summary>Selects one active colony.</summary>
    public void Select(UniverseColony colony)
    {
        var index = colonies.IndexOf(colony);
        if (index >= 0)
            selectedIndex = index;
    }

    /// <summary>Finds an active colony by its exact graph label.</summary>
    public UniverseColony? Find(string name)
    {
        foreach (var colony in Span)
        {
            if (string.Equals(colony.Name, name, StringComparison.Ordinal))
                return colony;
        }

        return null;
    }

    /// <summary>Returns a cold-path copy suitable for ad-hoc live code.</summary>
    public UniverseColony[] Snapshot() => [.. colonies];

    /// <summary>Ends every active child scope during engine shutdown.</summary>
    public void CloseAll()
    {
        while (colonies.Count > 0)
            Close(colonies[^1].Name);
    }
}
