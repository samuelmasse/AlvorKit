namespace AlvorKit;

/// <summary>Validates the canonical agent-policy manifest, entry budgets, discovery links, and template payload.</summary>
internal static class AgentPolicyGraph
{
    /// <summary>Directory names excluded while discovering active instruction files.</summary>
    private static readonly HashSet<string> ExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        "bin",
        "dist",
        "node_modules",
        "obj",
        "out",
        "packages",
        "tmp",
    };

    /// <summary>Returns structural violations when the repository owns an agent-policy manifest.</summary>
    public static IReadOnlyList<string> FindViolations(string repoRoot)
    {
        var root = Path.GetFullPath(repoRoot);
        var manifestPath = Path.Combine(root, "docs", "AgentRules", "RuleManifest.json");
        if (!File.Exists(manifestPath))
            return [];

        var violations = new List<string>();
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(manifestPath));
            var manifest = document.RootElement;
            ValidateBudgets(root, manifest, violations);
            var modulePaths = ValidateModules(root, manifest, violations);
            ValidateRules(root, manifest, violations);
            ValidateRootExposedRules(root, manifest, violations);
            ValidateMarkdownLinks(root, modulePaths, violations);
            ValidateNewGameInstructionTemplate(root, violations);
        }
        catch (System.Text.Json.JsonException exception)
        {
            violations.Add($"docs/AgentRules/RuleManifest.json: invalid JSON: {exception.Message}");
        }

        return violations.Order(StringComparer.Ordinal).ToArray();
    }

    /// <summary>Validates byte and line budgets declared by the manifest.</summary>
    private static void ValidateBudgets(string repoRoot, System.Text.Json.JsonElement manifest, List<string> violations)
    {
        foreach (var budget in manifest.GetProperty("budgets").EnumerateObject())
        {
            var path = ResolvePolicyPath(repoRoot, budget.Name);
            if (!File.Exists(path))
            {
                violations.Add($"{budget.Name}: budgeted instruction file does not exist");
                continue;
            }

            var maxBytes = budget.Value.GetProperty("maxBytes").GetInt64();
            var maxLines = budget.Value.GetProperty("maxLines").GetInt32();
            var bytes = new FileInfo(path).Length;
            var lines = File.ReadLines(path).Count();
            if (bytes > maxBytes)
                violations.Add($"{budget.Name}: {bytes} bytes exceeds the {maxBytes}-byte agent-policy budget");
            if (lines > maxLines)
                violations.Add($"{budget.Name}: {lines} lines exceeds the {maxLines}-line agent-policy budget");
        }
    }

    /// <summary>Validates unique module identifiers, canonical files, and required discovery headings.</summary>
    private static IReadOnlyList<string> ValidateModules(string repoRoot, System.Text.Json.JsonElement manifest, List<string> violations)
    {
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        var paths = new List<string>();
        foreach (var module in manifest.GetProperty("modules").EnumerateArray())
        {
            var identifier = module.GetProperty("id").GetString() ?? string.Empty;
            var relativePath = module.GetProperty("path").GetString() ?? string.Empty;
            paths.Add(relativePath);
            if (!identifiers.Add(identifier))
                violations.Add($"docs/AgentRules/RuleManifest.json: duplicate module id '{identifier}'");

            var path = ResolvePolicyPath(repoRoot, relativePath);
            if (!File.Exists(path))
            {
                violations.Add($"{relativePath}: registered agent-policy module does not exist");
                continue;
            }

            if (!File.ReadAllText(path).Contains("## Scope", StringComparison.Ordinal))
                violations.Add($"{relativePath}: agent-policy module must declare a Scope section");
        }

        return paths;
    }

    /// <summary>Validates unique rule identifiers, canonical owners, and registered override targets.</summary>
    private static void ValidateRules(string repoRoot, System.Text.Json.JsonElement manifest, List<string> violations)
    {
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rule in manifest.GetProperty("rules").EnumerateArray())
        {
            var identifier = rule.GetProperty("id").GetString() ?? string.Empty;
            var owner = rule.GetProperty("owner").GetString() ?? string.Empty;
            if (!identifiers.Add(identifier))
                violations.Add($"docs/AgentRules/RuleManifest.json: duplicate rule id '{identifier}'");

            var ownerPath = ResolvePolicyPath(repoRoot, owner);
            if (!File.Exists(ownerPath) || !File.ReadAllText(ownerPath).Contains(identifier, StringComparison.Ordinal))
                violations.Add($"{owner}: canonical owner does not expose rule '{identifier}'");
        }

        foreach (var ruleOverride in manifest.GetProperty("overrides").EnumerateArray())
        {
            var rule = ruleOverride.GetProperty("rule").GetString() ?? string.Empty;
            var overridden = ruleOverride.GetProperty("overrides").GetString() ?? string.Empty;
            if (!identifiers.Contains(rule))
                violations.Add($"docs/AgentRules/RuleManifest.json: override rule '{rule}' is not registered");
            if (!identifiers.Contains(overridden))
                violations.Add($"docs/AgentRules/RuleManifest.json: override target '{overridden}' is not registered");
        }
    }

    /// <summary>Validates that every root-exposed gate or invariant remains visible in the root dispatcher.</summary>
    private static void ValidateRootExposedRules(string repoRoot, System.Text.Json.JsonElement manifest, List<string> violations)
    {
        var agentsPath = Path.Combine(repoRoot, "AGENTS.md");
        var agents = File.Exists(agentsPath) ? File.ReadAllText(agentsPath) : string.Empty;
        var identifiers = new HashSet<string>(StringComparer.Ordinal);
        foreach (var rule in manifest.GetProperty("rootExposedRules").EnumerateArray())
        {
            var identifier = rule.GetString() ?? string.Empty;
            if (!identifiers.Add(identifier))
                violations.Add($"docs/AgentRules/RuleManifest.json: duplicate root-exposed rule '{identifier}'");
            if (!agents.Contains(identifier, StringComparison.Ordinal))
                violations.Add($"AGENTS.md: root-exposed rule '{identifier}' is missing");
        }
    }

    /// <summary>Validates relative Markdown links in active instructions and canonical policy modules.</summary>
    private static void ValidateMarkdownLinks(string repoRoot, IReadOnlyList<string> modulePaths, List<string> violations)
    {
        var files = Directory.EnumerateFiles(repoRoot, "AGENTS.md", SearchOption.AllDirectories)
            .Where(path => !HasExcludedDirectory(repoRoot, path))
            .Concat(modulePaths.Select(path => ResolvePolicyPath(repoRoot, path)))
            .Append(Path.Combine(repoRoot, "docs", "GameRepositoryInstructions.md"))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files.Where(File.Exists))
        {
            var text = File.ReadAllText(file);
            var matches = System.Text.RegularExpressions.Regex.Matches(text, @"\[[^\]]+\]\((?<target>[^)#]+)(?:#[^)]*)?\)");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var target = match.Groups["target"].Value;
                if (Uri.TryCreate(target, UriKind.Absolute, out _))
                    continue;

                var decoded = Uri.UnescapeDataString(target).Replace('/', Path.DirectorySeparatorChar);
                var resolved = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!, decoded));
                if (!File.Exists(resolved) && !Directory.Exists(resolved))
                    violations.Add($"{Path.GetRelativePath(repoRoot, file)}: Markdown link target '{target}' does not exist");
            }
        }
    }

    /// <summary>Returns true when a discovered instruction file belongs to generated or dependency output.</summary>
    private static bool HasExcludedDirectory(string repoRoot, string path) =>
        Path.GetRelativePath(repoRoot, path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(ExcludedDirectories.Contains);

    /// <summary>Validates that the new-game instruction payload is inert until generated.</summary>
    private static void ValidateNewGameInstructionTemplate(string repoRoot, List<string> violations)
    {
        var sourceRoot = Path.Combine(repoRoot, "res", "templates", "new-game", "source");
        if (!Directory.Exists(sourceRoot))
            return;

        if (File.Exists(Path.Combine(sourceRoot, "AGENTS.md")))
            violations.Add("res/templates/new-game/source/AGENTS.md: template instruction payload must be inert while editing AlvorKit");
        if (!File.Exists(Path.Combine(sourceRoot, "AGENTS.md.template")))
            violations.Add("res/templates/new-game/source/AGENTS.md.template: inert instruction payload is missing");
    }

    /// <summary>Resolves one manifest-owned repository-relative policy path.</summary>
    private static string ResolvePolicyPath(string repoRoot, string relativePath) =>
        Path.GetFullPath(Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
