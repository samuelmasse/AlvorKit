namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Process runner test double that records commands and returns configured output.</summary>
internal sealed class RecordingProcessRunner : IProcessRunner
{
    /// <summary>Commands passed to RunAsync.</summary>
    public List<CommandSpec> RunCommands { get; } = [];

    /// <summary>Commands passed to CaptureAsync.</summary>
    public List<CommandSpec> CaptureCommands { get; } = [];

    /// <summary>Output returned by CaptureAsync.</summary>
    public string CaptureOutput { get; set; } = "";

    /// <summary>Records a run command.</summary>
    public Task RunAsync(CommandSpec command)
    {
        RunCommands.Add(command);
        return Task.CompletedTask;
    }

    /// <summary>Records a capture command and returns CaptureOutput.</summary>
    public Task<string> CaptureAsync(CommandSpec command)
    {
        CaptureCommands.Add(command);
        return Task.FromResult(CaptureOutput);
    }
}
