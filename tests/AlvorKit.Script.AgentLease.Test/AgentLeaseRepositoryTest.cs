namespace AlvorKit.Script.AgentLease.Test;

/// <summary>Tests lease repository filesystem behavior.</summary>
[TestClass]
public sealed class AgentLeaseRepositoryTest
{
    /// <summary>Shared starting timestamp for deterministic repository fixtures.</summary>
    private static readonly DateTimeOffset StartTime = new(2026, 6, 15, 18, 0, 0, TimeSpan.Zero);

    /// <summary>Missing coordination directories read as an empty lease set.</summary>
    [TestMethod]
    public void ReadAll_MissingDirectory_ReturnsEmpty()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.AreEqual(0, repository.ReadAll().Count);
    }

    /// <summary>Invalid JSON files are ignored so a partial write cannot break all coordination.</summary>
    [TestMethod]
    public void ReadAll_InvalidJson_IgnoresFile()
    {
        using var workspace = TempWorkspace.Create();
        var agents = workspace.CreateDirectory("out", "agents");
        File.WriteAllText(Path.Combine(agents, "broken.json"), "{");
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.AreEqual(0, repository.ReadAll().Count);
    }

    /// <summary>Delete reports false when no lease file exists.</summary>
    [TestMethod]
    public void Delete_MissingLease_ReturnsFalse()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        Assert.IsFalse(repository.Delete("missing"));
    }

    /// <summary>Agent identifiers are converted to safe filenames when leases are written.</summary>
    [TestMethod]
    public void Write_SpecialAgentName_UsesSafeFilename()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        repository.Write(Lease("codex/a:b"));

        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, "out", "agents", "codex-a-b.json")));
    }

    /// <summary>Completely unsafe agent identifiers fall back to a readable filename.</summary>
    [TestMethod]
    public void Write_UnsafeAgentName_UsesFallbackFilename()
    {
        using var workspace = TempWorkspace.Create();
        var repository = new AgentLeaseRepository(workspace.Root);

        repository.Write(Lease("///"));

        Assert.IsTrue(File.Exists(Path.Combine(workspace.Root, "out", "agents", "agent.json")));
    }

    /// <summary>Builds a minimal active lease for repository tests.</summary>
    private static AgentLease Lease(string agent) =>
        new()
        {
            Agent = agent,
            Task = "Test",
            Mode = "write",
            Paths = ["src/**"],
            StartedAt = StartTime,
            UpdatedAt = StartTime,
            ExpiresAt = StartTime.AddMinutes(5)
        };
}
