namespace AlvorKit;

/// <summary>Reports an invalid scope-graph ownership or lifecycle operation.</summary>
public class InjectorScopeGraphException(string message) : Exception(message);
