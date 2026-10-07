namespace AlvorKit;

/// <summary>Owns one repository's discovery snapshot and evaluated filesystem subscriptions.</summary>
internal class SolutionRepositoryWatch(string root, SolutionWatchQueue queue, SolutionFileNotifications notifications) : IDisposable
{
    /// <summary>Owns checkout readiness, transaction observations, and the external writer lease.</summary>
    private readonly SolutionCheckout checkout = new(root, notifications, () => queue.Add(new(root, true)));
    /// <summary>Observes Git membership and ignore rules independently of graph inputs.</summary>
    private SolutionWatchInputs? discovery;
    /// <summary>Retains the latest known graph inputs until replacement subscriptions are established.</summary>
    private SolutionWatchInputs? graph;
    /// <summary>Keeps discovery membership and its latest complete evaluated graph together.</summary>
    private Snapshot snapshot = new() { Projects = [] };

    /// <summary>Exposes only a complete graph whose checkout has not changed since evaluation.</summary>
    internal SolutionGeneration? Generation
        => checkout.Ready && checkout.Revision == snapshot.Revision ? snapshot.Generation : null;
    /// <summary>Blocks aggregate publication while a participating checkout is pending or invalid.</summary>
    internal bool Pending => snapshot.Invalid || (snapshot.Projects.Count > 0 && Generation == null);

    /// <summary>Queries Git after discovery events; unchanged membership does not reevaluate MSBuild.</summary>
    public bool Discover()
    {
        if (!checkout.Refresh())
        {
            Defer();
            return false;
        }

        var next = new SolutionWatchInputs(notifications, () => queue.Add(new(root, true)));

        try
        {
            SolutionDiscovery.Observe(root, next);
            var paths = RepositoryProjects.Discover(root);
            var changed = !snapshot.Projects.SequenceEqual(paths, SolutionPaths.Comparer);
            snapshot.Projects = paths;

            if (paths.Count == 0)
            {
                snapshot.Generation = null;
                snapshot.Invalid = false;
                SolutionGenerator.Invalidate(root);
                graph?.Dispose();
                graph = null;
                return false;
            }

            if (!changed && !snapshot.Invalid)
                snapshot.Revision = checkout.Revision;

            return changed || snapshot.Invalid || graph == null;
        }
        finally
        {
            discovery?.Dispose();
            discovery = next;
        }
    }

    /// <summary>Replaces subscriptions only after evaluation, keeping notifications active throughout the transition.</summary>
    public bool Generate()
    {
        if (snapshot.Projects.Count == 0 && !snapshot.Invalid)
            return false;

        if (!checkout.Ready)
        {
            Defer();
            return false;
        }

        var revision = checkout.Revision;
        var next = new SolutionWatchInputs(notifications, () => queue.Add(new(root, false)));

        try
        {
            var generation = SolutionGenerator.Prepare(root, next);

            if (!checkout.Ready || checkout.Revision != revision)
            {
                Defer();
                return false;
            }

            SolutionGenerator.Publish(generation, false);

            if (snapshot.Invalid)
                Console.WriteLine($"Recovered solution for {root}.");

            snapshot.Invalid = false;
            snapshot.Generation = generation;
            snapshot.Revision = revision;
            return true;
        }
        catch (Exception exception)
        {
            if (graph is not null)
                next.Include(graph);

            if (!checkout.Ready || checkout.Revision != revision)
            {
                Defer();
                return false;
            }

            IEnumerable<Exception> failures = exception is AggregateException aggregate
                ? aggregate.Flatten().InnerExceptions : [exception];

            if (failures.Any(failure => failure is SolutionImportException))
                next.Stop(exception);

            throw;
        }
        finally
        {
            graph?.Dispose();
            graph = next;
        }
    }

    /// <summary>Leaves an incomplete checkout pending until its metadata changes; never publishes its partial graph.</summary>
    private void Defer()
    {
        if (!snapshot.Invalid)
            Console.WriteLine($"Waiting for Git checkout: {root}");

        snapshot.Invalid = true;
        snapshot.Generation = null;
        SolutionGenerator.Invalidate(root);
    }

    /// <summary>Reports a failed graph and removes output rather than publishing stale membership.</summary>
    public void Invalidate(Exception exception)
    {
        snapshot.Invalid = true;
        snapshot.Generation = null;

        if (Directory.Exists(root))
            SolutionGenerator.Invalidate(root);

        Console.Error.WriteLine($"INVALID solution for {root}: {exception.Message}");
    }

    /// <summary>Releases both discovery and graph observations.</summary>
    public void Dispose()
    {
        discovery?.Dispose();
        graph?.Dispose();
        checkout.Dispose();
        discovery = null;
        graph = null;
    }

    /// <summary>Retains repository membership, validity, and the checkout revision represented by its evaluated document.</summary>
    private struct Snapshot
    {
        /// <summary>Tracks the last discovered independent project roots.</summary>
        public IReadOnlyList<string> Projects;
        /// <summary>Retains the last valid evaluated graph, or no graph after invalidation.</summary>
        public SolutionGeneration? Generation;
        /// <summary>Identifies the Git state represented by the retained graph.</summary>
        public long Revision;
        /// <summary>Requires evaluation after a pending checkout or invalid graph recovers.</summary>
        public bool Invalid;
    }
}
