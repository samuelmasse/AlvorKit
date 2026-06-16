namespace AlvorKit.Script.AlvorEye;

/// <summary>Result emitted by an AlvorEye command.</summary>
/// <param name="ExitCode">Process exit code.</param>
/// <param name="Lines">Human-readable output lines.</param>
internal sealed record AlvorEyeResult(int ExitCode, IReadOnlyList<string> Lines)
{
    /// <summary>Successful result with output lines.</summary>
    public static AlvorEyeResult Success(params string[] lines) => new(0, lines);
}
