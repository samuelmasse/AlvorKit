namespace AlvorKit.Script.Lint;

/// <summary>Coordinates formatter, EditorConfig, and GitHub Actions lint checks.</summary>
/// <param name="options">Validated lint options for the current run.</param>
/// <param name="processRunner">Process runner used to execute external tools.</param>
/// <param name="actionlintTool">Installer and resolver for the actionlint executable.</param>
/// <param name="requestedMaxParallelCommands">Optional process concurrency override. Zero uses the repository default.</param>
internal sealed class LintRunner(
    LintOptions options,
    IProcessRunner processRunner,
    IActionlintTool actionlintTool,
    int requestedMaxParallelCommands = 0)
{
    /// <summary>Default maximum number of external lint commands to run at once.</summary>
    private static int DefaultMaxParallelCommands => Math.Min(4, Math.Max(1, Environment.ProcessorCount));

    /// <summary>Synchronizes progress output from parallel lint tasks.</summary>
    private readonly Lock consoleLock = new();

    /// <summary>Maximum number of external lint commands allowed to run concurrently.</summary>
    private readonly int maxParallelCommands = ResolveMaxParallelCommands(requestedMaxParallelCommands);

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
        using var commandGate = new SemaphoreSlim(maxParallelCommands);
        using var dotNetFormatGate = new SemaphoreSlim(1);
        var commandTasks = preActionlintCommands
            .Select(command => RunCommandAsync(command, commandGate, dotNetFormatGate))
            .ToList();

        var actionlintPath = actionlintTask is null ? null : await actionlintTask;
        if (actionlintPath is not null)
            commandTasks.Add(RunCommandAsync(LintPlan.ActionlintCommand(repoRoot, actionlintPath, scope), commandGate, dotNetFormatGate));

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
    private async Task<CommandResult> RunCommandAsync(
        CommandSpec command,
        SemaphoreSlim commandGate,
        SemaphoreSlim dotNetFormatGate)
    {
        if (!IsDotNetFormatCommand(command))
            return await RunCommandWithGlobalGateAsync(command, commandGate);

        await dotNetFormatGate.WaitAsync();
        try
        {
            return await RunCommandWithGlobalGateAsync(command, commandGate);
        }
        finally
        {
            dotNetFormatGate.Release();
        }
    }

    /// <summary>Runs one command with the global external-process concurrency limit.</summary>
    private async Task<CommandResult> RunCommandWithGlobalGateAsync(CommandSpec command, SemaphoreSlim commandGate)
    {
        await commandGate.WaitAsync();
        try
        {
            return await RunCommandAsync(command);
        }
        finally
        {
            commandGate.Release();
        }
    }

    /// <summary>Runs one command after concurrency admission has been granted.</summary>
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

    /// <summary>Returns true for dotnet format invocations that must not restore the same workspace concurrently.</summary>
    private static bool IsDotNetFormatCommand(CommandSpec command) =>
        string.Equals(command.FileName, "dotnet", StringComparison.OrdinalIgnoreCase)
        && command.Arguments is ["format", ..];

    /// <summary>Formats elapsed time for progress output.</summary>
    private static string ElapsedText(DateTimeOffset started) =>
        (DateTimeOffset.UtcNow - started).ToString(@"mm\:ss\.fff", CultureInfo.InvariantCulture);

    /// <summary>Maps the optional caller override to a safe positive concurrency limit.</summary>
    private static int ResolveMaxParallelCommands(int requested)
    {
        if (requested < 0)
            throw new ArgumentOutOfRangeException(nameof(requested), "Lint command parallelism must be zero or greater.");

        return requested == 0 ? DefaultMaxParallelCommands : requested;
    }
}
