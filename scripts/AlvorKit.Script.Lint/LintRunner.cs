using System.Globalization;

namespace AlvorKit.Script.Lint;

/// <summary>Coordinates formatter, EditorConfig, and GitHub Actions lint checks.</summary>
/// <param name="options">Validated lint options for the current run.</param>
/// <param name="processRunner">Process runner used to execute external tools.</param>
/// <param name="actionlintTool">Installer and resolver for the actionlint executable.</param>
internal sealed class LintRunner(
    LintOptions options,
    IProcessRunner processRunner,
    IActionlintTool actionlintTool)
{
    /// <summary>Synchronizes progress output from parallel lint tasks.</summary>
    private readonly object consoleLock = new();

    /// <summary>Runs every configured lint command concurrently and returns the first non-zero exit code.</summary>
    public async Task<int> RunAsync()
    {
        var repoRoot = Path.GetFullPath(options.RepoRoot);
        var scope = options.IncludePatterns.Count == 0 ? null : LintScope.FromPatterns(repoRoot, options.IncludePatterns);
        if (scope is { IsEmpty: true })
        {
            WriteProgress("[lint:skip] no existing files matched scoped lint includes");
            return 0;
        }

        var actionlintTask = LintPlan.RequiresActionlint(scope) ? ResolveActionlintAsync(repoRoot) : null;
        var preActionlintCommands = scope is null
            ? LintPlan.CommandsBeforeActionlint(repoRoot, options.Fix)
            : LintPlan.CommandsBeforeActionlint(repoRoot, options.Fix, scope);
        var commandTasks = preActionlintCommands
            .Select(RunCommandAsync)
            .ToList();

        var actionlintPath = actionlintTask is null ? null : await actionlintTask;
        if (actionlintPath is not null)
            commandTasks.Add(RunCommandAsync(LintPlan.ActionlintCommand(repoRoot, actionlintPath, scope)));

        var results = await Task.WhenAll(commandTasks);
        return results.FirstOrDefault(result => result.ExitCode != 0)?.ExitCode ?? (LintPlan.RequiresActionlint(scope) && actionlintPath is null ? 1 : 0);
    }

    /// <summary>Resolves actionlint while other lint commands are allowed to run.</summary>
    private async Task<string?> ResolveActionlintAsync(string repoRoot)
    {
        var started = DateTimeOffset.UtcNow;
        WriteProgress($"[lint:start] actionlint setup");
        try
        {
            var path = await actionlintTool.EnsureAsync(repoRoot);
            WriteProgress($"[lint:done] actionlint setup in {ElapsedText(started)}");
            return path;
        }
        catch (Exception ex)
        {
            WriteProgress($"[lint:done] actionlint setup failed in {ElapsedText(started)}");
            WriteProgress(ex.Message);
            return null;
        }
    }

    /// <summary>Runs one command with start, completion, and captured output logging.</summary>
    private async Task<CommandResult> RunCommandAsync(CommandSpec command)
    {
        var started = DateTimeOffset.UtcNow;
        WriteProgress($"[lint:start] {Label(command)}");
        WriteProgress("> " + CommandText.Display(command));

        try
        {
            var result = await processRunner.RunAsync(command);
            WriteResult(result, started);
            return result;
        }
        catch (Exception ex)
        {
            var result = new CommandResult(command, 1, ex.Message);
            WriteResult(result, started);
            return result;
        }
    }

    /// <summary>Writes a completed command result and its captured output.</summary>
    private void WriteResult(CommandResult result, DateTimeOffset started)
    {
        WriteProgress($"[lint:done] {Label(result.Command)} exit {result.ExitCode} in {ElapsedText(started)}");
        if (!string.IsNullOrWhiteSpace(result.Output))
            WriteProgress(result.Output.TrimEnd());
    }

    /// <summary>Writes one synchronized progress block to the console.</summary>
    private void WriteProgress(string message)
    {
        lock (consoleLock)
            Console.WriteLine(message);
    }

    /// <summary>Returns a stable display label for a command.</summary>
    private static string Label(CommandSpec command) =>
        command.Label;

    /// <summary>Formats elapsed time for progress output.</summary>
    private static string ElapsedText(DateTimeOffset started) =>
        (DateTimeOffset.UtcNow - started).ToString(@"mm\:ss\.fff", CultureInfo.InvariantCulture);
}
