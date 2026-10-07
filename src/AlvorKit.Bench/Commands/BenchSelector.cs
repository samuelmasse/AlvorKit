namespace AlvorKit;

/// <summary>Selects cases in catalog order using include unions followed by exclusions.</summary>
[Bench]
public class BenchSelector
{
    /// <summary>Applies include and exclude patterns without reordering cases.</summary>
    public BenchCase[] Select(BenchCatalog catalog, string[] includes, string[] excludes)
    {
        var includeGlobs = CreateGlobs(includes);
        var excludeGlobs = CreateGlobs(excludes);
        List<BenchCase> selected = [];

        foreach (var benchmark in catalog.Cases)
        {
            var included = includeGlobs.Length == 0 || MatchesAny(benchmark.Id, includeGlobs);
            var excluded = MatchesAny(benchmark.Id, excludeGlobs);

            if (included && !excluded)
                selected.Add(benchmark);
        }

        return [.. selected];
    }

    /// <summary>Builds matchers once per selection, outside timed work.</summary>
    private BenchGlob[] CreateGlobs(string[] patterns) => [.. patterns.Select(pattern => new BenchGlob(pattern))];

    /// <summary>Reports whether at least one authored pattern matches the path.</summary>
    private bool MatchesAny(string value, BenchGlob[] globs)
    {
        foreach (var glob in globs)
        {
            if (glob.Matches(value))
                return true;
        }

        return false;
    }
}
