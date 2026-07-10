namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Owns one isolated worker setup and its single measured body.</summary>
internal sealed record EcsArchBenchCase(
    string Unit,
    long Operations,
    Action Body,
    object Root,
    bool Repeatable = false,
    Action? Quiesce = null);

/// <summary>Keeps allocs and Ent handles rooted through diagnostic capture.</summary>
internal sealed record EcsArchBenchState(EntArena[] Allocs, EntMut[] Ents, object? Extra = null);
