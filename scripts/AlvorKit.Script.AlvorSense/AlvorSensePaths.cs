namespace AlvorKit.Script.AlvorSense;

/// <summary>Centralizes file and directory names used by persistent AlvorSense sessions.</summary>
internal static class AlvorSensePaths
{
    /// <summary>Gets the default root directory for persistent AlvorSense sessions.</summary>
    internal static string Root => Path.Combine("out", "alvorsense-sessions");

    /// <summary>Gets a session directory for the given id.</summary>
    internal static string SessionDir(string id) => Path.Combine(Root, id);

    /// <summary>Gets the session manifest path.</summary>
    internal static string Manifest(string sessionDir) => Path.Combine(sessionDir, "session.json");

    /// <summary>Gets the host ready marker path.</summary>
    internal static string Ready(string sessionDir) => Path.Combine(sessionDir, "ready.json");

    /// <summary>Gets the isolated runtime directory used by the detached host process.</summary>
    internal static string HostRuntime(string sessionDir) => Path.Combine(sessionDir, "host-runtime");

    /// <summary>Gets a request path before it is atomically committed.</summary>
    internal static string RequestTemp(string sessionDir, string id) => Path.Combine(sessionDir, "requests", id + ".tmp");

    /// <summary>Gets a committed request path.</summary>
    internal static string Request(string sessionDir, string id) => Path.Combine(sessionDir, "requests", id + ".json");

    /// <summary>Gets a response path.</summary>
    internal static string Response(string sessionDir, string id) => Path.Combine(sessionDir, "responses", id + ".json");

    /// <summary>Gets the target stdout log path.</summary>
    internal static string Stdout(string sessionDir) => Path.Combine(sessionDir, "stdout.log");

    /// <summary>Gets the target stderr log path.</summary>
    internal static string Stderr(string sessionDir) => Path.Combine(sessionDir, "stderr.log");
}
