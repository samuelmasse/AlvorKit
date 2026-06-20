namespace AlvorKit.Script.AlvorSense;

/// <summary>Response emitted by the background host after a command batch completes or times out.</summary>
/// <param name="Id">Unique request id copied from the matching request.</param>
/// <param name="Ok">Whether the host observed the expected state transition or exit.</param>
/// <param name="CommandCount">Number of command lines accepted from the foreground request.</param>
/// <param name="StateLine">Most recent state line captured while handling the request, when one was observed.</param>
/// <param name="OutputLines">Target stdout lines captured while the request was active.</param>
/// <param name="ProcessExited">Whether the hosted game process had exited when the response was written.</param>
/// <param name="ExitCode">Hosted game process exit code when available.</param>
/// <param name="Error">Error message when the request failed.</param>
internal sealed record AlvorSenseResponse(
    string Id,
    bool Ok,
    int CommandCount,
    string? StateLine,
    string[] OutputLines,
    bool ProcessExited,
    int? ExitCode,
    string? Error);
