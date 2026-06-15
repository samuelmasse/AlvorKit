namespace AlvorKit.Script.BindgenReview;

/// <summary>Coordinates disposable before/after bindgen snapshots and generated-code diffs.</summary>
/// <param name="store">Filesystem store for review manifests and cleanup.</param>
/// <param name="processRunner">Process runner used for bindgen and git commands.</param>
/// <param name="suffixFactory">Factory for unique five-character directory suffixes.</param>
/// <param name="utcNow">Clock used when writing review manifests.</param>
internal sealed class BindgenReviewCoordinator(
    BindgenReviewStore store,
    IProcessRunner processRunner,
    Func<string> suffixFactory,
    Func<DateTimeOffset> utcNow)
{
    /// <summary>Creates the default coordinator used by the command-line entry point.</summary>
    public static BindgenReviewCoordinator CreateDefault() =>
        new(new BindgenReviewStore(), new SystemProcessRunner(), BindgenReviewPaths.RandomSuffix, () => DateTimeOffset.UtcNow);

    /// <summary>Executes one parsed bindgen review command.</summary>
    /// <param name="command">Parsed command-line request.</param>
    public async Task<BindgenReviewResult> ExecuteAsync(BindgenReviewCommand command) =>
        command.Kind switch
        {
            BindgenReviewCommandKind.Help => BindgenReviewResult.Success(BindgenReviewCommandParser.HelpText),
            BindgenReviewCommandKind.Start => await StartAsync(command),
            BindgenReviewCommandKind.After => await AfterAsync(command),
            BindgenReviewCommandKind.Diff => await DiffAsync(command),
            BindgenReviewCommandKind.Clean => Clean(command),
            BindgenReviewCommandKind.Finish => await FinishAsync(command),
            _ => BindgenReviewResult.Success(BindgenReviewCommandParser.HelpText)
        };

    /// <summary>Creates a unique review session and captures the before snapshot.</summary>
    private async Task<BindgenReviewResult> StartAsync(BindgenReviewCommand command)
    {
        var session = BindgenReviewPaths.Create(command.RepoRoot, command.Library!, command.CaseName, suffixFactory);
        var manifest = new BindgenReviewManifest(command.Library!, command.CaseName ?? command.Library!, session.RelativeRoot, utcNow());
        store.WriteManifest(session, manifest);

        var capture = await RunBindgenAsync(command.RepoRoot, manifest.Library, session.BeforeRelativeRoot);
        return capture.Prepend(
            $"Review root: {session.RelativeRoot}",
            $"Finish command: dotnet run --project scripts\\AlvorKit.Script.BindgenReview -- finish {session.RelativeRoot}");
    }

    /// <summary>Captures the after snapshot for an existing review session.</summary>
    private async Task<BindgenReviewResult> AfterAsync(BindgenReviewCommand command)
    {
        var (session, manifest) = Load(command);
        return await RunBindgenAsync(command.RepoRoot, manifest.Library, session.AfterRelativeRoot);
    }

    /// <summary>Prints the generated-code diff for an existing review session.</summary>
    private async Task<BindgenReviewResult> DiffAsync(BindgenReviewCommand command)
    {
        var (session, _) = Load(command);
        return await RunDiffAsync(command.RepoRoot, session);
    }

    /// <summary>Deletes an existing review session after validating its manifest.</summary>
    private BindgenReviewResult Clean(BindgenReviewCommand command)
    {
        var (session, _) = Load(command);
        store.Delete(session);
        return BindgenReviewResult.Success($"Deleted {session.RelativeRoot}.");
    }

    /// <summary>Captures after, prints the diff, and deletes the review session unless requested otherwise.</summary>
    private async Task<BindgenReviewResult> FinishAsync(BindgenReviewCommand command)
    {
        var (session, manifest) = Load(command);
        var after = await RunBindgenAsync(command.RepoRoot, manifest.Library, session.AfterRelativeRoot);
        if (after.ExitCode != 0)
            return after;

        var diff = await RunDiffAsync(command.RepoRoot, session);
        var combined = new BindgenReviewResult(diff.ExitCode, [.. after.Lines, .. diff.Lines]);
        if (combined.ExitCode != 0 || command.Keep)
            return combined;

        store.Delete(session);
        return combined.Append($"Deleted {session.RelativeRoot}.");
    }

    /// <summary>Loads the validated review session and manifest for an existing review root.</summary>
    private (BindgenReviewSession Session, BindgenReviewManifest Manifest) Load(BindgenReviewCommand command)
    {
        var session = BindgenReviewPaths.Existing(command.RepoRoot, command.ReviewRoot!);
        return (session, store.ReadManifest(session));
    }

    /// <summary>Runs bindgen for one library into a specific snapshot root.</summary>
    private async Task<BindgenReviewResult> RunBindgenAsync(string repoRoot, string library, string outputRoot) =>
        await RunAsync(new("dotnet", ["run", "--project", "scripts/AlvorKit.Script.Bindgen", "--", library, "--output-root", outputRoot], repoRoot));

    /// <summary>Runs git diff over the before and after snapshot roots.</summary>
    private async Task<BindgenReviewResult> RunDiffAsync(string repoRoot, BindgenReviewSession session)
    {
        var result = await RunAsync(new("git", ["diff", "--no-index", "--", session.BeforeRelativeRoot, session.AfterRelativeRoot], repoRoot));
        if (result.ExitCode is 0 && result.Lines.Count == 0)
            return BindgenReviewResult.Success("No generated diff.");

        return result.ExitCode <= 1 ? result with { ExitCode = 0 } : result;
    }

    /// <summary>Runs one child process and returns its captured output as individual lines.</summary>
    private async Task<BindgenReviewResult> RunAsync(CommandSpec command)
    {
        var result = await processRunner.RunAsync(command);
        return new(result.ExitCode, SplitLines(result.Output));
    }

    /// <summary>Splits captured process output without emitting empty trailing lines.</summary>
    private static IReadOnlyList<string> SplitLines(string output) =>
        output.TrimEnd().Length == 0 ? [] : output.TrimEnd().Split(["\r\n", "\n"], StringSplitOptions.None);
}
