namespace AlvorKit;

/// <summary>An owned native-memory snapshot; requested payload and usable bytes remain distinct.</summary>
public record BenchMemoryUsage(
    string Name,
    double SelfBytes,
    double TotalBytes,
    double SelfRequestedBytes,
    double TotalRequestedBytes,
    BenchMemoryUsage[] Children)
{
    /// <summary>Local usable bytes beyond requested payload.</summary>
    public double SelfOverheadBytes => SelfBytes - SelfRequestedBytes;
    /// <summary>Subtree usable bytes beyond requested payload.</summary>
    public double TotalOverheadBytes => TotalBytes - TotalRequestedBytes;

    /// <summary>Averages a nonempty set of captured trees, matching duplicate sibling names by occurrence.</summary>
    public static BenchMemoryUsage Mean(BenchMemoryUsage[] samples)
    {
        BenchMemoryUsage?[] roots = [.. samples];
        return MeanNode(samples[0].Name, roots);
    }

    /// <summary>Averages corresponding nodes; absent samples contribute zero.</summary>
    private static BenchMemoryUsage MeanNode(string name, BenchMemoryUsage?[] samples)
    {
        var children = new List<BenchMemoryUsage>();
        foreach (var key in ChildKeys(samples))
        {
            var childSamples = new BenchMemoryUsage?[samples.Length];
            for (var i = 0; i < samples.Length; i++)
                childSamples[i] = FindChild(samples[i], key.Name, key.Occurrence);
            children.Add(MeanNode(key.Name, childSamples));
        }

        return new(name,
            samples.Sum(sample => sample?.SelfBytes ?? 0) / samples.Length,
            samples.Sum(sample => sample?.TotalBytes ?? 0) / samples.Length,
            samples.Sum(sample => sample?.SelfRequestedBytes ?? 0) / samples.Length,
            samples.Sum(sample => sample?.TotalRequestedBytes ?? 0) / samples.Length,
            [.. children]);
    }

    /// <summary>Unions sibling identities using name and occurrence so duplicate labels remain distinct.</summary>
    private static List<(string Name, int Occurrence)> ChildKeys(BenchMemoryUsage?[] samples)
    {
        List<(string Name, int Occurrence)> keys = [];
        foreach (var sample in samples)
        {
            if (sample is null)
                continue;
            Dictionary<string, int> counts = [];
            foreach (var child in sample.Children)
            {
                counts.TryGetValue(child.Name, out var occurrence);
                counts[child.Name] = occurrence + 1;
                var key = (child.Name, occurrence);
                if (!keys.Contains(key))
                    keys.Add(key);
            }
        }

        return keys;
    }

    /// <summary>Finds a matching sibling occurrence without conflating duplicate labels.</summary>
    private static BenchMemoryUsage? FindChild(BenchMemoryUsage? parent, string name, int occurrence)
    {
        if (parent is null)
            return null;

        foreach (var child in parent.Children)
        {
            if (child.Name == name && occurrence-- == 0)
                return child;
        }

        return null;
    }
}
