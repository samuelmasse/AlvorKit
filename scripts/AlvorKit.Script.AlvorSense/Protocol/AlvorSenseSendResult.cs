namespace AlvorKit.Script.AlvorSense;

/// <summary>Foreground JSON result for a send request, optionally enriched with local diagnostics.</summary>
/// <param name="Id">Unique request id copied from the matching request.</param>
/// <param name="Ok">Whether the host observed the expected state transition or exit.</param>
/// <param name="CommandCount">Number of command lines accepted from the foreground request.</param>
/// <param name="StateLine">Most recent state line captured while handling the request, when one was observed.</param>
/// <param name="OutputLines">Target stdout lines captured while the request was active.</param>
/// <param name="ProcessExited">Whether the hosted game process had exited when the response was written.</param>
/// <param name="ExitCode">Hosted game process exit code when available.</param>
/// <param name="Error">Error message when the request failed.</param>
/// <param name="StderrTail">Requested stderr tail for a failed request whose target exited.</param>
internal sealed record AlvorSenseSendResult(
    string Id,
    bool Ok,
    int CommandCount,
    string? StateLine,
    string[] OutputLines,
    bool ProcessExited,
    int? ExitCode,
    string? Error,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    string[]? StderrTail)
{
    /// <summary>Creates a foreground result from a raw host response and optional stderr tail.</summary>
    /// <param name="response">Raw mailbox response from the host.</param>
    /// <param name="stderrTail">Optional stderr lines requested for diagnostics.</param>
    internal static AlvorSenseSendResult From(AlvorSenseResponse response, string[]? stderrTail) =>
        new(
            response.Id,
            response.Ok,
            response.CommandCount,
            response.StateLine,
            response.OutputLines,
            response.ProcessExited,
            response.ExitCode,
            response.Error,
            stderrTail);
}
