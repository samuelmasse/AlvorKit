namespace AlvorKit.Script.AlvorSense;

/// <summary>Formats foreground-friendly reasons for missing target responses.</summary>
internal static class AlvorSenseTargetFailure
{
    /// <summary>Returns a targeted failure reason when the expected response was not observed.</summary>
    /// <param name="observed">Whether the expected state or exit condition was observed.</param>
    /// <param name="targetExited">Whether the hosted target had exited.</param>
    /// <param name="exitCode">Hosted target exit code when available.</param>
    internal static string? Message(bool observed, bool targetExited, int? exitCode)
    {
        if (observed)
            return null;
        if (!targetExited)
            return "Timed out waiting for target.";
        return exitCode is { } code
            ? $"Target exited before the expected response (exit code {code})."
            : "Target exited before the expected response.";
    }
}
