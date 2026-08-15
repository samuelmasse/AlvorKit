namespace AlvorKit;

/// <summary>Centralizes workspace-owned Source Update coordinator artifacts.</summary>
internal static class SourceUpdateCoordinatorPaths
{
    internal static string Directory(string workspacePath) =>
        Path.Combine(workspacePath, "source");

    internal static string Manifest(string workspacePath) =>
        Path.Combine(Directory(workspacePath), "coordinator.json");

    internal static string Runtime(string workspacePath) =>
        Path.Combine(Directory(workspacePath), "coordinator-runtime");

    internal static string Evidence(string workspacePath, string operationId) =>
        Path.Combine(Directory(workspacePath), "evidence", operationId + ".json");

    internal static string Error(string workspacePath) =>
        Path.Combine(Directory(workspacePath), "coordinator-error.txt");
}
