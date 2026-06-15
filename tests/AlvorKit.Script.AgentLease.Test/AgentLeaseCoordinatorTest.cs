namespace AlvorKit.Script.AgentLease.Test;

/// <summary>Tests advisory lease coordination behavior against a temporary lease store.</summary>
[TestClass]
public sealed class AgentLeaseCoordinatorTest
{
    /// <summary>Shared starting timestamp for deterministic lease fixtures.</summary>
    private static readonly DateTimeOffset StartTime = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);

    /// <summary>Start creates a lease JSON file with normalized paths and default expiration.</summary>
    [TestMethod]
    public void Execute_Start_CreatesLease()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        var coordinator = Coordinator(repository);

        var result = coordinator.Execute(Command(AgentLeaseCommandKind.Start, agent: "codex-a", task: "Edit helper", paths: [@"./scripts\Tool.cs"]));
        var lease = repository.Read("codex-a");

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNotNull(lease);
        Assert.AreEqual("Edit helper", lease.Task);
        Assert.AreEqual("write", lease.Mode);
        CollectionAssert.AreEqual(new[] { "scripts/Tool.cs" }, lease.Paths.ToArray());
        Assert.AreEqual(StartTime + TimeSpan.FromMinutes(5), lease.ExpiresAt);
    }

    /// <summary>Start reports an advisory overlap without blocking lease creation.</summary>
    [TestMethod]
    public void Execute_StartWithOverlap_ReportsOverlap()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("codex-b", paths: ["scripts/**"]));

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Start, agent: "codex-a", task: "Edit helper", paths: ["scripts/Tool.cs"]));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsTrue(result.Lines.Any(line => line.Contains("Advisory overlap", StringComparison.Ordinal)));
        Assert.IsTrue(result.Lines.Any(line => line.Contains("codex-b", StringComparison.Ordinal)));
    }

    /// <summary>Start generates an agent id when neither explicit nor ambient identity exists.</summary>
    [TestMethod]
    public void Execute_StartWithoutAgent_GeneratesAgent()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Start, task: "Generated", paths: ["src/**"]));
        var lease = repository.ReadAll().Single();

        Assert.AreEqual(0, result.ExitCode);
        StringAssert.StartsWith(lease.Agent, "codex-20260615-180000-");
    }

    /// <summary>Start honors explicit modes and notes.</summary>
    [TestMethod]
    public void Execute_StartWithModeAndNotes_WritesValues()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        var command = new AgentLeaseCommand(
            AgentLeaseCommandKind.Start,
            "C:/repo",
            "codex-a",
            "Run formatter",
            "format",
            ["src/**"],
            "Broad but scoped",
            null,
            AgentLeaseCommand.DefaultTimeout,
            false);

        var result = Coordinator(repository).Execute(command);
        var lease = repository.Read("codex-a");

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNotNull(lease);
        Assert.AreEqual("format", lease.Mode);
        Assert.AreEqual("Broad but scoped", lease.Notes);
    }

    /// <summary>Start rejects missing task descriptions when called directly.</summary>
    [TestMethod]
    public void Execute_StartWithoutTask_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.ThrowsExactly<ArgumentException>(
            () => Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Start, agent: "codex-a", paths: ["src/**"])));
    }

    /// <summary>Touch refreshes an existing lease and preserves its original start time.</summary>
    [TestMethod]
    public void Execute_Touch_RefreshesExistingLease()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("codex-a", paths: ["src/**"], expiresAt: StartTime.AddMinutes(1)));
        var clock = new FakeClock(StartTime.AddMinutes(3));

        var coordinator = new AgentLeaseCoordinator(repository, clock, () => "codex-a");
        var result = coordinator.Execute(Command(AgentLeaseCommandKind.Touch, task: "New task", mode: "test", paths: ["tests/**"]));
        var lease = repository.Read("codex-a");

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNotNull(lease);
        Assert.AreEqual(StartTime, lease.StartedAt);
        Assert.AreEqual(StartTime.AddMinutes(3), lease.UpdatedAt);
        Assert.AreEqual(StartTime.AddMinutes(8), lease.ExpiresAt);
        Assert.AreEqual("New task", lease.Task);
        Assert.AreEqual("test", lease.Mode);
        CollectionAssert.AreEqual(new[] { "tests/**" }, lease.Paths.ToArray());
    }

    /// <summary>Touch can update notes while preserving existing paths when no path replacement is supplied.</summary>
    [TestMethod]
    public void Execute_TouchWithNoPaths_PreservesPaths()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("codex-a", paths: ["src/**"]));

        var command = new AgentLeaseCommand(
            AgentLeaseCommandKind.Touch,
            "C:/repo",
            "codex-a",
            null,
            null,
            [],
            "Keep narrow",
            null,
            AgentLeaseCommand.DefaultTimeout,
            false);
        var result = Coordinator(repository).Execute(command);
        var lease = repository.Read("codex-a");

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNotNull(lease);
        CollectionAssert.AreEqual(new[] { "src/**" }, lease.Paths.ToArray());
        Assert.AreEqual("Keep narrow", lease.Notes);
    }

    /// <summary>Touch reports missing leases when an explicit agent has no lease file.</summary>
    [TestMethod]
    public void Execute_TouchMissingLease_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Touch, agent: "missing")));
    }

    /// <summary>List hides stale leases by default and tells the caller how to show them.</summary>
    [TestMethod]
    public void Execute_List_HidesStaleByDefault()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("active", paths: ["src/**"]));
        repository.Write(Lease("stale", paths: ["tests/**"], expiresAt: StartTime.AddMinutes(-1)));

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.List));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsTrue(result.Lines.Any(line => line.Contains("active", StringComparison.Ordinal)));
        Assert.IsFalse(result.Lines.Any(line => line.Contains("stale test", StringComparison.Ordinal)));
        Assert.IsTrue(result.Lines.Any(line => line.Contains("Stale leases omitted: 1", StringComparison.Ordinal)));
    }

    /// <summary>List reports a friendly empty message when the lease directory is missing.</summary>
    [TestMethod]
    public void Execute_ListWithoutLeases_ReturnsEmptyMessage()
    {
        using var workspace = TempWorkspace.Create();

        var result = Coordinator(new AgentLeaseRepository(workspace.Root)).Execute(Command(AgentLeaseCommandKind.List));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "No active agent leases found.");
    }

    /// <summary>List can include stale leases when requested.</summary>
    [TestMethod]
    public void Execute_ListIncludeStale_ReturnsStaleLease()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("stale", paths: ["tests/**"], expiresAt: StartTime.AddMinutes(-1)));

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.List, includeStale: true));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsTrue(result.Lines.Any(line => line.StartsWith("stale stale", StringComparison.Ordinal)));
    }

    /// <summary>List reports a distinct empty message when stale output is requested and no leases exist.</summary>
    [TestMethod]
    public void Execute_ListIncludeStaleWithoutLeases_ReturnsEmptyMessage()
    {
        using var workspace = TempWorkspace.Create();

        var result = Coordinator(new AgentLeaseRepository(workspace.Root)).Execute(Command(AgentLeaseCommandKind.List, includeStale: true));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "No agent leases found.");
    }

    /// <summary>Check returns advisory conflict status when another active lease overlaps.</summary>
    [TestMethod]
    public void Execute_CheckWithOverlap_ReturnsConflictExitCode()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("other", paths: ["repo-wide"]));

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Check, paths: ["AGENTS.md"]));

        Assert.AreEqual(2, result.ExitCode);
        Assert.IsTrue(result.Lines.Any(line => line.Contains("other", StringComparison.Ordinal)));
    }

    /// <summary>Check reports overlapping leases in stable agent order.</summary>
    [TestMethod]
    public void Execute_CheckWithMultipleOverlaps_OrdersByAgent()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("z-agent", paths: ["src/**"]));
        repository.Write(Lease("a-agent", paths: ["src/**"]));

        var result = Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Check, paths: ["src/Foo.cs"]));

        Assert.AreEqual("- a-agent: a-agent test (write; src/**)", result.Lines[1]);
        Assert.AreEqual("- z-agent: z-agent test (write; src/**)", result.Lines[2]);
    }

    /// <summary>Check ignores stale leases and the current agent's own lease.</summary>
    [TestMethod]
    public void Execute_Check_IgnoresOwnAndStaleLeases()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("self", paths: ["src/**"]));
        repository.Write(Lease("stale", paths: ["src/**"], expiresAt: StartTime.AddMinutes(-1)));

        var result = Coordinator(repository, () => "self").Execute(Command(AgentLeaseCommandKind.Check, paths: ["src/Foo.cs"]));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "No active overlapping leases.");
    }

    /// <summary>Done removes the current agent's lease.</summary>
    [TestMethod]
    public void Execute_Done_RemovesLease()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("codex-a", paths: ["src/**"]));

        var result = Coordinator(repository, () => "codex-a").Execute(Command(AgentLeaseCommandKind.Done));

        Assert.AreEqual(0, result.ExitCode);
        Assert.IsNull(repository.Read("codex-a"));
    }

    /// <summary>Done reports clearly when no lease exists for the current agent.</summary>
    [TestMethod]
    public void Execute_DoneWithoutLease_ReturnsNoLeaseMessage()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        var result = Coordinator(repository, () => "missing").Execute(Command(AgentLeaseCommandKind.Done));

        Assert.AreEqual(0, result.ExitCode);
        CollectionAssert.Contains(result.Lines.ToArray(), "No lease found for agent missing.");
    }

    /// <summary>Conflict writes a markdown note with task, reason, paths, and overlap context.</summary>
    [TestMethod]
    public void Execute_Conflict_WritesMarkdownNote()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        repository.Write(Lease("other", paths: ["scripts/**"]));

        var result = Coordinator(repository).Execute(Command(
            AgentLeaseCommandKind.Conflict,
            agent: "codex-a",
            task: "Need shared file",
            paths: ["scripts/Shared.cs"],
            reason: "Small mechanical edit"));

        Assert.AreEqual(0, result.ExitCode);
        var note = Directory.EnumerateFiles(Path.Combine(workspace.Root, "out", "agents", "conflicts"), "*.md").Single();
        var text = File.ReadAllText(note);
        StringAssert.Contains(text, "Need shared file");
        StringAssert.Contains(text, "Small mechanical edit");
        StringAssert.Contains(text, "other");
    }

    /// <summary>Conflict notes can be written without an explicit task summary.</summary>
    [TestMethod]
    public void Execute_ConflictWithoutTask_UsesFallbackTask()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Conflict, agent: "codex-a", paths: ["src/**"], reason: "Need it"));

        var note = Directory.EnumerateFiles(Path.Combine(workspace.Root, "out", "agents", "conflicts"), "*.md").Single();
        StringAssert.Contains(File.ReadAllText(note), "Unspecified task");
    }

    /// <summary>Unknown command kinds return the help text defensively.</summary>
    [TestMethod]
    public void Execute_UnknownCommand_ReturnsHelp()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);
        var command = Command((AgentLeaseCommandKind)999);

        var result = Coordinator(repository).Execute(command);

        Assert.AreEqual(0, result.ExitCode);
        Assert.AreEqual(AgentLeaseCommandParser.HelpText, result.Lines.Single());
    }

    /// <summary>The system clock returns a current UTC timestamp.</summary>
    [TestMethod]
    public void SystemAgentLeaseClock_UtcNow_ReturnsCurrentTime()
    {
        var before = DateTimeOffset.UtcNow;
        var now = new SystemAgentLeaseClock().UtcNow;
        var after = DateTimeOffset.UtcNow;

        Assert.IsTrue(now >= before);
        Assert.IsTrue(now <= after);
    }

    /// <summary>Commands that mutate an existing lease require an explicit or ambient agent identifier.</summary>
    [TestMethod]
    public void Execute_TouchWithoutAgent_Throws()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.ThrowsExactly<InvalidOperationException>(() => Coordinator(repository).Execute(Command(AgentLeaseCommandKind.Touch)));
    }

    /// <summary>Creates a coordinator with the standard fixed test clock.</summary>
    private static AgentLeaseCoordinator Coordinator(AgentLeaseRepository repository, Func<string?>? currentAgent = null) =>
        new(repository, new FakeClock(StartTime), currentAgent ?? (() => null));

    /// <summary>Builds a parsed command shape for focused coordinator tests.</summary>
    private static AgentLeaseCommand Command(
        AgentLeaseCommandKind kind,
        string? agent = null,
        string? task = null,
        string? mode = null,
        IReadOnlyList<string>? paths = null,
        string? reason = null,
        bool includeStale = false) =>
        new(kind, "C:/repo", agent, task, mode, paths ?? [], null, reason, AgentLeaseCommand.DefaultTimeout, includeStale);

    /// <summary>Builds a lease fixture with active defaults.</summary>
    private static AgentLease Lease(
        string agent,
        IReadOnlyList<string> paths,
        DateTimeOffset? expiresAt = null) =>
        new()
        {
            Agent = agent,
            Task = agent + " test",
            Mode = "write",
            Paths = paths,
            StartedAt = StartTime,
            UpdatedAt = StartTime,
            ExpiresAt = expiresAt ?? StartTime.AddMinutes(5)
        };

    /// <summary>Mutable test clock used to make lease timestamps deterministic.</summary>
    private sealed class FakeClock(DateTimeOffset now) : IAgentLeaseClock
    {
        /// <inheritdoc />
        public DateTimeOffset UtcNow { get; set; } = now;
    }
}
