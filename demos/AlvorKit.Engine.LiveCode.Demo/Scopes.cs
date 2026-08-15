namespace AlvorKit;

/// <summary>Marks services owned by one luminous colony lifetime.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ColonyAttribute : InjectorAttribute;

/// <summary>Owns the isolated simulation services for one simultaneously active colony.</summary>
[Colony]
public sealed class ColonyScope : InjectorScope<ColonyAttribute>;

/// <summary>Marks services owned by one short-lived diagnostic probe.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ProbeAttribute : InjectorAttribute;

/// <summary>Creates a nested lifetime that live code can inspect and then close.</summary>
[Probe]
public sealed class ProbeScope : InjectorScope<ProbeAttribute>;
