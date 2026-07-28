namespace AlvorKit.LiveCode;

/// <summary>Wire-safe snapshot of the active development process scope graph.</summary>
public sealed record LiveCodeScopeGraph(
    long Revision,
    long RootId,
    LiveCodeScopeNode[] Nodes);
