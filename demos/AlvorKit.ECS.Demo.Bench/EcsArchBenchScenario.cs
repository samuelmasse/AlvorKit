namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Describes one stable archetypal benchmark case.</summary>
internal readonly record struct EcsArchBenchScenario(string Id, string Category, string Unit, int Width = 0);
