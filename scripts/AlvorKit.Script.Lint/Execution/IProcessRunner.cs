namespace AlvorKit.Script.Lint;

/// <summary>Runs planned process commands for the lint coordinator.</summary>
internal interface IProcessRunner
{
    /// <summary>Runs a command and returns its captured result.</summary>
    Task<CommandResult> RunAsync(CommandSpec command);
}
