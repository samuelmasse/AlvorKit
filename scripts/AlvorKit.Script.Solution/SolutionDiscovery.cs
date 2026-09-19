namespace AlvorKit;

/// <summary>Observes project discovery independently of MSBuild graph evaluation.</summary>
internal static class SolutionDiscovery
{
    /// <summary>Subscribes to source topology, ignore rules, and Git metadata before querying project membership.</summary>
    public static void Observe(string root, SolutionWatchInputs inputs)
    {
        inputs.File(Path.Combine(root, ".git"));
        inputs.File(Path.Combine(root, ".gitignore"));
        inputs.Glob(root, "*.csproj", false, false);

        foreach (var area in RepositoryProjects.Areas)
        {
            var path = Path.Combine(root, area);
            inputs.Exists(path);

            if (!Directory.Exists(path))
                continue;

            inputs.Glob(path, "*.csproj", true, false);
            inputs.Glob(path, ".gitignore", true, true);
        }

        var metadata = RepositoryGit.Read(root, "rev-parse", "--path-format=absolute",
            "--git-path", "index", "--git-path", "info/exclude", "--git-path", "config");

        foreach (var path in metadata.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            inputs.File(path.TrimEnd('\r'));

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? Path.Combine(home, ".config");
        inputs.File(Environment.GetEnvironmentVariable("GIT_CONFIG_GLOBAL") ?? Path.Combine(home, ".gitconfig"));
        inputs.File(Path.Combine(configHome, "git/config"));
        inputs.File(Path.Combine(configHome, "git/ignore"));

        var config = RepositoryGit.Read(root, "config", "--null", "--list", "--show-origin", "--includes").Split('\0');

        for (var index = 0; index + 1 < config.Length; index += 2)
        {
            var origin = config[index];
            var entry = config[index + 1].Split('\n', 2);

            if (origin.StartsWith("file:", StringComparison.Ordinal))
                inputs.File(Path.GetFullPath(origin[5..], root));

            if (entry[0].Equals("core.excludesfile", StringComparison.OrdinalIgnoreCase) && entry.Length == 2 && entry[1].Length > 0)
            {
                var path = RepositoryGit.Read(root, "config", "--path", "--get", "core.excludesFile").TrimEnd('\r', '\n');
                inputs.File(Path.GetFullPath(path, root));
            }
        }
    }
}
