namespace AlvorKit.Script.AgentLease;

/// <summary>Entry point for the advisory agent lease helper.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs a lease helper command and returns a process exit code.</summary>
    /// <param name="args">Command-line arguments supplied by the caller.</param>
    public static int Main(string[] args)
    {
        try
        {
            var command = AgentLeaseCommandParser.Parse(args);
            if (command.Kind == AgentLeaseCommandKind.Help)
            {
                Console.WriteLine(AgentLeaseCommandParser.HelpText);
                return 0;
            }

            var repository = new AgentLeaseRepository(command.RepoRoot);
            var coordinator = new AgentLeaseCoordinator(repository, new SystemAgentLeaseClock(), CurrentAgent);
            var result = coordinator.Execute(command);
            foreach (var line in result.Lines)
                Console.WriteLine(line);

            return result.ExitCode;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }

    /// <summary>Returns the ambient agent identifier from the process environment.</summary>
    private static string? CurrentAgent() =>
        Environment.GetEnvironmentVariable("ALVORKIT_AGENT_ID");
}
