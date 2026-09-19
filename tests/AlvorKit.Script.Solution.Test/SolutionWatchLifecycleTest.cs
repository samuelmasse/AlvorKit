namespace AlvorKit;

/// <summary>Verifies checkout lifecycle changes against real Git metadata and Windows directory handles.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionWatchLifecycleTest
{
    /// <summary>Both watch modes release checkout handles and observe repositories recreated at the same path.</summary>
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task DeletesAndRecreatesRepository(bool explicitRoot)
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");

        if (explicitRoot)
            fixture.StartForRepository(root);
        else fixture.Start();

        await fixture.Membership(root, "Game.csproj", true);
        await fixture.Quiet(root);

        for (var iteration = 0; iteration < 3; iteration++)
        {
            Directory.Delete(root, true);
            await fixture.Quiet(root);
            fixture.Repository("Game", "");
            fixture.Write($"Repos/Game/src/Extra{iteration}/Extra{iteration}.csproj", SolutionGeneratorTest.Project(""));
            await fixture.Membership(root, $"Extra{iteration}.csproj", true);
            Assert.IsFalse(File.Exists(Path.Combine(root, ".alvorkit-solution-watch.lock")));
            await fixture.Quiet(root);
        }

        if (!explicitRoot)
        {
            var added = fixture.Repository("Added", "");
            await fixture.Membership(added, "Added.csproj", true);
        }
    }

    /// <summary>Partial Git initialization and checkout wait for metadata events without evaluating incomplete imports.</summary>
    [TestMethod]
    public async Task DefersIncomingCheckoutUntilGitReleasesIndex()
    {
        await using var fixture = new SolutionWatchFixture();
        var ready = fixture.Repository("Ready", "");
        fixture.Start();
        await fixture.Membership(ready, "Ready.csproj", true);
        var incoming = fixture.PathFor("Repos/Incoming");
        Directory.CreateDirectory(Path.Combine(incoming, ".git"));
        fixture.Write("Repos/Incoming/src/Game/Game.csproj",
            SolutionGeneratorTest.Project("<Import Project=\"../../Checkout.props\" />"));
        await fixture.Quiet(ready, incoming);
        Assert.AreEqual(0, fixture.Count(incoming));

        fixture.Write("Repos/Incoming/.git/index.lock", "");
        GitRepositoryFixture.Initialize(incoming);
        await fixture.Quiet(ready, incoming);
        Assert.AreEqual(0, fixture.Count(incoming));
        fixture.Write("Repos/Incoming/Checkout.props", "<Project />");
        File.Delete(Path.Combine(incoming, ".git/index.lock"));
        await fixture.Membership(incoming, "Game.csproj", true);
    }

    /// <summary>An existing checkout removes stale output during Git writes and reevaluates after the lock is removed.</summary>
    [TestMethod]
    public async Task DefersUpdatesDuringGitTransaction()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");
        fixture.Start();
        await fixture.Membership(root, "Game.csproj", true);
        await fixture.Quiet(root);
        var before = fixture.Count(root);
        fixture.Write("Repos/Game/.git/index.lock", "");
        fixture.Write("Repos/Game/src/Game/Game.csproj",
            SolutionGeneratorTest.Project("<Import Project=\"../../Checkout.props\" />"));
        await fixture.Until(() => !File.Exists(RepositoryProjects.SolutionPath(root)));
        await fixture.Quiet(root);
        Assert.AreEqual(before, fixture.Count(root));
        fixture.Write("Repos/Game/Checkout.props", "<Project />");
        File.Delete(Path.Combine(root, ".git/index.lock"));
        await fixture.Membership(root, "Game.csproj", true);
        Assert.IsTrue(fixture.Count(root) > before);
    }

    /// <summary>Parent and explicit modes cannot concurrently own the same checkout; shutdown releases the lease.</summary>
    [TestMethod]
    public async Task RejectsOverlappingOwnershipAndReleasesLease()
    {
        string root;

        await using (var fixture = new SolutionWatchFixture())
        {
            root = fixture.Repository("Game", "");
            fixture.Start();
            await fixture.Membership(root, "Game.csproj", true);
            using var other = new SolutionWatcher(new([root], null, true, false, false));
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var exception = await Assert.ThrowsExactlyAsync<IOException>(() => other.RunAsync(cancellation.Token));
            StringAssert.Contains(exception.Message, "watcher lease");
        }

        using var released = SolutionWatchLease.Acquire(root);
    }
}
