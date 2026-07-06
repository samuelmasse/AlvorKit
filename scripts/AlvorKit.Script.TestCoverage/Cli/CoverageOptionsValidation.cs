namespace AlvorKit.Script.TestCoverage;

/// <summary>Validates parsed coverage options and shared option value access.</summary>
internal static class CoverageOptionsValidation
{
    /// <summary>Validates parsed option values before constructing the immutable options record.</summary>
    public static void Validate(
        CoverageThresholds thresholds,
        int maxParallel,
        string? outputRoot,
        string? runId,
        double maxTestDurationMilliseconds,
        string? repoRoot)
    {
        thresholds.Validate();
        if (maxParallel < 1)
            throw new ArgumentOutOfRangeException(nameof(maxParallel), "Max parallelism must be at least 1.");
        if (maxTestDurationMilliseconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxTestDurationMilliseconds), "Max test duration must be greater than zero milliseconds.");
        if (outputRoot is { Length: 0 })
            throw new ArgumentException("Output root must not be empty.", nameof(outputRoot));
        if (repoRoot is { Length: 0 })
            throw new ArgumentException("Repository root must not be empty.", nameof(repoRoot));
        if (runId is not null)
            CoverageRunIdValidator.Validate(runId);
    }

    /// <summary>Reads the value following an option name.</summary>
    public static string ReadValue(IReadOnlyList<string> args, ref int index)
    {
        if (++index >= args.Count)
            throw new ArgumentException($"Missing value for '{args[index - 1]}'.");

        return args[index];
    }
}
