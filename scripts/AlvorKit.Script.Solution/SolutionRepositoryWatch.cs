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
    /// <summary>Compares discovered membership without reevaluating unchanged roots.</summary>
    private IReadOnlyList<string> projects = [];
    /// <summary>Requires regeneration after invalidation even when membership is unchanged.</summary>
    private bool invalid;

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
            var changed = !projects.SequenceEqual(paths, SolutionPaths.Comparer);
            projects = paths;
            return changed || invalid || (graph is null && SolutionGenerator.HasGeneratedSolution(root));
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

            if (invalid)
                Console.WriteLine($"Recovered solution for {root}.");

            invalid = false;
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
        if (!invalid)
            Console.WriteLine($"Waiting for Git checkout: {root}");

        invalid = true;
        SolutionGenerator.Invalidate(root);
    }

    /// <summary>Reports a failed graph and removes output rather than publishing stale membership.</summary>
    public void Invalidate(Exception exception)
    {
        invalid = true;

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
}
