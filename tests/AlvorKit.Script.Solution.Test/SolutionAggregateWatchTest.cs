namespace AlvorKit;

/// <summary>Exercises aggregate lifecycle behavior through real filesystem notifications.</summary>
[TestClass]
[DoNotParallelize]
public class SolutionAggregateWatchTest
{
    /// <summary>Membership and checkout changes update the aggregate without reevaluating unrelated repositories.</summary>
    [TestMethod]
    public async Task ReconcilesAggregateMembershipAndRepositoryTopology()
    {
        await using var fixture = new SolutionWatchFixture();
        var first = fixture.Repository("First", "");
        var second = fixture.Repository("Second", "");
        var output = fixture.PathFor("Repos/Workspace.slnx");
        fixture.StartWithAggregate();
        await fixture.Until(() => Contains(output, "First.csproj") && Contains(output, "Second.csproj"));
        await fixture.Quiet(first, second);
        var initial = fixture.Count(second);
        fixture.Write("Repos/First/src/Extra/Extra.csproj", SolutionGeneratorTest.Project(""));
        await fixture.Until(() => Contains(output, "Extra.csproj"));
        Assert.AreEqual(initial, fixture.Count(second));
        File.Delete(fixture.PathFor("Repos/First/src/Extra/Extra.csproj"));
        await fixture.Until(() => File.Exists(output) && !Contains(output, "Extra.csproj"));
        var third = fixture.Repository("Third", "");
        await fixture.Until(() => Contains(output, "Third.csproj"));
        var renamed = fixture.PathFor("Repos/Renamed");
        Directory.Move(third, renamed);
        await fixture.Until(() => Contains(output, "Renamed/src/Third/Third.csproj"));
        Assert.IsFalse(Contains(output, "Third/src/Third/Third.csproj"));
        Directory.Delete(renamed, true);
        await fixture.Until(() => File.Exists(output) && !Contains(output, "Third.csproj"));
        File.Delete(fixture.PathFor("Repos/First/src/First/First.csproj"));
        await fixture.Until(() => File.Exists(output) && !Contains(output, "First.csproj"));
        Assert.IsTrue(Contains(output, "Second.csproj"));
        await fixture.Quiet(first, second);
        Assert.AreEqual(initial, fixture.Count(second));
    }

    /// <summary>Pending Git transactions and invalid graphs remove the aggregate until complete graphs recover.</summary>
    [TestMethod]
    public async Task InvalidatesAggregateForPendingAndInvalidGraphs()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");
        var output = fixture.PathFor("Repos/Workspace.slnx");
        fixture.StartWithAggregate();
        await fixture.Until(() => Contains(output, "Game.csproj"));
        fixture.Write("Repos/Game/.git/index.lock", "");
        await fixture.Until(() => !File.Exists(output));
        await fixture.Quiet(root);
        Assert.IsFalse(File.Exists(output));
        File.Delete(fixture.PathFor("Repos/Game/.git/index.lock"));
        await fixture.Until(() => Contains(output, "Game.csproj"));
        fixture.Write("Repos/Game/src/Game/Game.csproj", SolutionGeneratorTest.Project(
            "<ItemGroup><ProjectReference Include=\"../Missing/Missing.csproj\" /></ItemGroup>"));
        await fixture.Until(() => !File.Exists(output));
        fixture.Write("Repos/Game/src/Missing/Missing.csproj", SolutionGeneratorTest.Project(""));
        await fixture.Until(() => Contains(output, "Missing.csproj"));
        Assert.IsTrue(Contains(output, "Game.csproj"));
    }

    /// <summary>Separate repository selections cannot concurrently write the same aggregate file.</summary>
    [TestMethod]
    public async Task RejectsDuplicateAggregateOwnership()
    {
        await using var fixture = new SolutionWatchFixture();
        var root = fixture.Repository("Game", "");
        var output = fixture.PathFor("Repos/Workspace.slnx");
        fixture.StartWithAggregate();
        await fixture.Until(() => Contains(output, "Game.csproj"));
        using var other = new SolutionWatcher(new([root], null, true, false, false, output));
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var exception = await Assert.ThrowsExactlyAsync<IOException>(() => other.RunAsync(cancellation.Token));
        StringAssert.Contains(exception.Message, "watcher lease");
        Assert.IsTrue(Contains(output, "Game.csproj"));
    }

    /// <summary>Reads only atomically published documents when observing expected membership.</summary>
    private static bool Contains(string output, string member)
        => File.Exists(output) && File.ReadAllText(output).Contains(member, StringComparison.Ordinal);
}
