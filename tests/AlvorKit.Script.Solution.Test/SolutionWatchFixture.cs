namespace AlvorKit;

/// <summary>Runs real notifications against isolated repositories and exposes completed evaluations.</summary>
internal class SolutionWatchFixture : IAsyncDisposable
{
    private readonly TempWorkspace workspace = TempWorkspace.Create();
    private readonly CancellationTokenSource cancellation = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, int> evaluations = [];
    private SolutionWatcher? watcher;
    private Task? running;

    /// <summary>Returns a fixture path, keeping external imports outside the watched checkout parent.</summary>
    public string PathFor(string path) => Path.Combine(workspace.Root, path);

    /// <summary>Creates a real Git checkout before adding its authored project.</summary>
    public string Repository(string name, string body)
    {
        var root = workspace.CreateDirectory($"Repos/{name}");
        GitRepositoryFixture.Initialize(root);
        Write($"Repos/{name}/src/{name}/{name}.csproj", SolutionGeneratorTest.Project(body));
        return root;
    }

    /// <summary>Writes a watched input, creating its containing directories when needed.</summary>
    public void Write(string path, string content) => workspace.Write(path, content);

    /// <summary>Starts one watcher after the initial fixture has been authored.</summary>
    public void Start()
        => Start(new([], PathFor("Repos"), true, false, false));

    /// <summary>Exercises explicit checkout ownership with the same observable harness.</summary>
    public void StartForRepository(string root)
        => Start(new([root], null, true, false, false));

    /// <summary>Subscribes to evaluation diagnostics before starting the selected watch mode.</summary>
    private void Start(SolutionOptions options)
    {
        watcher = new(options);
        watcher.Evaluated += root => evaluations.AddOrUpdate(root, 1, (_, count) => count + 1);
        running = watcher.RunAsync(cancellation.Token);
    }

    /// <summary>Counts graph evaluations, even when they produce byte-identical output.</summary>
    public int Count(string root) => evaluations.GetValueOrDefault(Path.GetFullPath(root));

    /// <summary>Waits for output membership without swallowing a failed watcher.</summary>
    public Task Membership(string root, string member, bool present) => Until(() =>
    {
        var path = RepositoryProjects.SolutionPath(root);
        return File.Exists(path) && File.ReadAllText(path).Contains(member, StringComparison.Ordinal) == present;
    });

    /// <summary>Bounds event-driven assertions while surfacing a native watcher failure immediately.</summary>
    public async Task Until(Func<bool> predicate)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);

        while (!predicate())
        {
            if (running!.IsCompleted)
                await running;

            Assert.IsTrue(DateTime.UtcNow < deadline, "Watcher did not produce the expected change.");
            await Task.Delay(25);
        }
    }

    /// <summary>Checks that a quiet interval performs no graph work.</summary>
    public async Task Quiet(params string[] roots)
    {
        await Task.Delay(600);
        var before = roots.Select(Count).ToArray();
        await Task.Delay(600);

        if (running!.IsCompleted)
            await running;

        CollectionAssert.AreEqual(before, roots.Select(Count).ToArray(), "An idle watcher reevaluated a graph.");
    }

    /// <summary>Stops native notifications before removing the fixture directories.</summary>
    public async ValueTask DisposeAsync()
    {
        cancellation.Cancel();

        try
        {
            if (running is not null)
                await running;
        }
        catch (OperationCanceledException) { }
        finally
        {
            watcher?.Dispose();
            cancellation.Dispose();
            workspace.Dispose();
        }
    }
}
