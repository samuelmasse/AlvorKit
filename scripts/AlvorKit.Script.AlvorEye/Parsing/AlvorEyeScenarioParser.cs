namespace AlvorKit.Script.AlvorEye;

/// <summary>Parses AlvorEye scenario JSON files.</summary>
internal static class AlvorEyeScenarioParser
{
    /// <summary>Loads and parses a scenario file from disk.</summary>
    /// <param name="path">Scenario JSON path.</param>
    public static AlvorEyeScenario ParseFile(string path) => Parse(File.ReadAllText(path, Encoding.UTF8), Path.GetDirectoryName(path));

    /// <summary>Parses scenario JSON text.</summary>
    /// <param name="json">Scenario JSON text.</param>
    /// <param name="scenarioDirectory">Directory used to resolve relative run paths.</param>
    public static AlvorEyeScenario Parse(string json, string? scenarioDirectory = null)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var run = root.TryGetProperty("run", out var runElement) ? ParseRun(runElement, scenarioDirectory) : null;
        if (!root.TryGetProperty("window", out var windowElement))
            throw new ArgumentException("Scenario requires a window section.");

        var output = root.TryGetProperty("output", out var outputElement) ? ParseOutput(outputElement) : new ScenarioOutput();
        var freeze = root.TryGetProperty("freeze", out var freezeElement) ? ParseFreeze(freezeElement) : new FreezeOptions();
        var timeline = root.TryGetProperty("timeline", out var timelineElement)
            ? AlvorEyeActionParser.ParseActions(timelineElement)
            : [];

        return new()
        {
            Run = run,
            Window = ParseWindow(windowElement),
            Output = output,
            Freeze = freeze,
            Timeline = timeline
        };
    }

    /// <summary>Parses the process launch section.</summary>
    private static ScenarioRun ParseRun(JsonElement element, string? scenarioDirectory)
    {
        var executable = ScenarioJson.String(element, "executable") ?? throw new ArgumentException("run.executable is required.");
        var args = element.TryGetProperty("args", out var argsElement)
            ? argsElement.EnumerateArray().Select(arg => arg.GetString() ?? "").ToArray()
            : [];
        var workingDirectory = ScenarioJson.String(element, "workingDirectory");
        if (workingDirectory is not null && !Path.IsPathRooted(workingDirectory) && scenarioDirectory is not null)
            workingDirectory = Path.GetFullPath(Path.Combine(scenarioDirectory, workingDirectory));

        return new()
        {
            Executable = executable,
            Args = args,
            WorkingDirectory = workingDirectory,
            Environment = ParseEnvironment(element)
        };
    }

    /// <summary>Parses environment variables from a run section.</summary>
    private static IReadOnlyDictionary<string, string> ParseEnvironment(JsonElement element)
    {
        if (!element.TryGetProperty("environment", out var envElement) || envElement.ValueKind != JsonValueKind.Object)
            return new Dictionary<string, string>();

        return envElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.GetString() ?? "");
    }

    /// <summary>Parses window matching and placement settings.</summary>
    private static ScenarioWindow ParseWindow(JsonElement element)
    {
        var title = ScenarioJson.String(element, "title") ?? throw new ArgumentException("window.title is required.");
        return new()
        {
            Title = title,
            Exact = ScenarioJson.Bool(element, "exact"),
            Timeout = ScenarioJson.Duration(element, "timeoutSeconds", "timeoutMilliseconds", TimeSpan.FromSeconds(15)),
            Width = ScenarioJson.Int(element, "width"),
            Height = ScenarioJson.Int(element, "height")
        };
    }

    /// <summary>Parses output settings.</summary>
    private static ScenarioOutput ParseOutput(JsonElement element) =>
        new()
        {
            RunId = ScenarioJson.String(element, "runId"),
            Directory = ScenarioJson.String(element, "directory")
        };

    /// <summary>Parses freeze behavior settings.</summary>
    private static FreezeOptions ParseFreeze(JsonElement element) =>
        new()
        {
            Strategy = ScenarioJson.String(element, "strategy") ?? "processSuspend",
            ResumeSettle = ScenarioJson.Duration(element, "resumeSettleSeconds", "resumeSettleMilliseconds", TimeSpan.FromMilliseconds(250))
        };
}
