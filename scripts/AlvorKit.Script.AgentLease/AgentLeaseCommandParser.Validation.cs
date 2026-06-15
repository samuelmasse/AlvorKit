namespace AlvorKit.Script.AgentLease;

/// <summary>Validation and shared helpers for the advisory lease command parser.</summary>
internal static partial class AgentLeaseCommandParser
{
    /// <summary>Validates parsed options against command-specific requirements.</summary>
    /// <param name="kind">Command kind being parsed.</param>
    /// <param name="task">Optional task description.</param>
    /// <param name="mode">Optional coordination mode.</param>
    /// <param name="paths">Path claims supplied by the caller.</param>
    /// <param name="reason">Optional conflict reason.</param>
    private static void Validate(AgentLeaseCommandKind kind, string? task, string? mode, IReadOnlyList<string> paths, string? reason)
    {
        if (mode is not null && !AgentLeaseModes.IsKnown(mode))
            throw new ArgumentException($"Unknown lease mode '{mode}'.");
        if (kind is AgentLeaseCommandKind.Start && string.IsNullOrWhiteSpace(task))
            throw new ArgumentException("start requires --task.");
        if (kind is AgentLeaseCommandKind.Start or AgentLeaseCommandKind.Check or AgentLeaseCommandKind.Conflict && paths.Count == 0)
            throw new ArgumentException($"{kind.ToString().ToLowerInvariant()} requires at least one --path.");
        if (kind is AgentLeaseCommandKind.Conflict && string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("conflict requires --reason.");
        if (kind is AgentLeaseCommandKind.Start or AgentLeaseCommandKind.Touch && paths.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Path claims must not be blank.");
    }

    /// <summary>Reads the value following an option name.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="index">Index of the current option, advanced to the value.</param>
    private static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }

    /// <summary>Finds an explicit repository root before normal root discovery is needed.</summary>
    /// <param name="args">Command-line arguments.</param>
    private static string? RepoRootArgument(IReadOnlyList<string> args)
    {
        for (var index = 0; index < args.Count - 1; index++)
        {
            if (args[index] == "--repo-root")
                return args[index + 1];
        }

        return null;
    }
}
