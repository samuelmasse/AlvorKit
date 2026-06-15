namespace AlvorKit.Script.Lint.Test;

/// <summary>Tests the lint runner's parallel command coordination behavior.</summary>
[TestClass]
public sealed class LintRunnerTest
{
    /// <summary>Starts every command before waiting for any command to finish.</summary>
    [TestMethod]
    public async Task RunAsyncStartsCommandsConcurrently()
    {
        using var workspace = TempWorkspace.Create();
        var processRunner = new BlockingProcessRunner(expectedStarts: 3);
        var actionlintTool = new FakeActionlintTool("actionlint");
        var runner = new LintRunner(new(workspace.Root, Fix: false, ShowHelp: false), processRunner, actionlintTool);

        var runnerTask = runner.RunAsync();
        await processRunner.AllStarted.Task;

        Assert.IsFalse(runnerTask.IsCompleted);
        processRunner.Release();
        var exitCode = await runnerTask;
        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(actionlintTool.Called);
        Assert.AreEqual(3, processRunner.Commands.Count);
    }

    /// <summary>Returns a failing exit code only after all commands have had a chance to run.</summary>
    [TestMethod]
    public async Task RunAsyncReturnsFailureAfterRunningAllCommands()
    {
        using var workspace = TempWorkspace.Create();
        var processRunner = new FakeProcessRunner(new Queue<int>([0, 9, 0]));
        var actionlintTool = new FakeActionlintTool("actionlint");
        var runner = new LintRunner(new(workspace.Root, Fix: false, ShowHelp: false), processRunner, actionlintTool);

        var exitCode = await runner.RunAsync();

        Assert.AreEqual(9, exitCode);
        Assert.IsTrue(actionlintTool.Called);
        Assert.AreEqual(3, processRunner.Commands.Count);
    }

    /// <summary>Returns failure after pre-actionlint commands complete when actionlint setup fails.</summary>
    [TestMethod]
    public async Task RunAsyncReturnsFailureWhenActionlintSetupFails()
    {
        using var workspace = TempWorkspace.Create();
        var processRunner = new FakeProcessRunner(new Queue<int>([0, 0]));
        var actionlintTool = new FailingActionlintTool();
        var runner = new LintRunner(new(workspace.Root, Fix: false, ShowHelp: false), processRunner, actionlintTool);

        var exitCode = await runner.RunAsync();

        Assert.AreEqual(1, exitCode);
        Assert.AreEqual(2, processRunner.Commands.Count);
    }

    /// <summary>Converts a process runner exception into a failing command result after other commands complete.</summary>
    [TestMethod]
    public async Task RunAsyncReturnsFailureWhenProcessRunnerThrows()
    {
        using var workspace = TempWorkspace.Create();
        var processRunner = new ThrowingProcessRunner();
        var actionlintTool = new FakeActionlintTool("actionlint");
        var runner = new LintRunner(new(workspace.Root, Fix: false, ShowHelp: false), processRunner, actionlintTool);

        var exitCode = await runner.RunAsync();

        Assert.AreEqual(1, exitCode);
        Assert.AreEqual(3, processRunner.Commands.Count);
    }

    /// <summary>Fake process runner that returns predetermined exit codes.</summary>
    private sealed class FakeProcessRunner(Queue<int> exitCodes) : IProcessRunner
    {
        /// <summary>Commands observed during the test run.</summary>
        public List<CommandSpec> Commands { get; } = [];

        /// <summary>Records a command and returns the next configured exit code.</summary>
        public Task<CommandResult> RunAsync(CommandSpec command)
        {
            Commands.Add(command);
            return Task.FromResult(new CommandResult(command, exitCodes.Dequeue(), ""));
        }
    }

    /// <summary>Fake process runner that throws for every observed command.</summary>
    private sealed class ThrowingProcessRunner : IProcessRunner
    {
        /// <summary>Commands observed during the test run.</summary>
        public List<CommandSpec> Commands { get; } = [];

        /// <summary>Records a command, then throws a representative process failure.</summary>
        public Task<CommandResult> RunAsync(CommandSpec command)
        {
            Commands.Add(command);
            throw new InvalidOperationException("process did not start");
        }
    }

    /// <summary>Fake process runner that blocks every command until released by the test.</summary>
    private sealed class BlockingProcessRunner(int expectedStarts) : IProcessRunner
    {
        /// <summary>Commands observed during the test run.</summary>
        public List<CommandSpec> Commands { get; } = [];

        /// <summary>Task completed after all expected commands have started.</summary>
        public TaskCompletionSource AllStarted { get; } = new();

        /// <summary>Release gate for blocked command tasks.</summary>
        private readonly TaskCompletionSource release = new();

        /// <summary>Records a command and waits until the test releases all commands.</summary>
        public async Task<CommandResult> RunAsync(CommandSpec command)
        {
            Commands.Add(command);
            if (Commands.Count == expectedStarts)
                AllStarted.SetResult();
            await release.Task;
            return new(command, 0, "");
        }

        /// <summary>Allows all blocked commands to complete.</summary>
        public void Release() =>
            release.SetResult();
    }

    /// <summary>Fake actionlint resolver for sequencing tests.</summary>
    private sealed class FakeActionlintTool(string path) : IActionlintTool
    {
        /// <summary>True once the runner asks for actionlint.</summary>
        public bool Called { get; private set; }

        /// <summary>Returns the configured actionlint path.</summary>
        public Task<string> EnsureAsync(string repoRoot)
        {
            Called = true;
            return Task.FromResult(path);
        }
    }

    /// <summary>Fake actionlint resolver that fails before returning a command path.</summary>
    private sealed class FailingActionlintTool : IActionlintTool
    {
        /// <summary>Throws a setup failure for actionlint resolution.</summary>
        public Task<string> EnsureAsync(string repoRoot) =>
            throw new InvalidOperationException("No actionlint today.");
    }
}
