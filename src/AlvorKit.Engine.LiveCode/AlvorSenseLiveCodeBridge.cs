namespace AlvorKit;

/// <summary>Runs finite AlvorSense command batches against the same window used by an ordinary live game launch.</summary>
internal sealed class AlvorSenseLiveCodeBridge(
    AgentGlfwWindowHost host,
    WindowLoop window,
    RootGl gl) : ILiveCodeBridge
{
    private const int MaximumCommands = 512;
    private const int MaximumCommandCharacters = 4096;
    private readonly AgentWindowPuppet puppet = new(host, window, gl);

    /// <inheritdoc />
    public LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "alvorsense",
        1,
        "Execute an atomic AlvorSense input/frame/screenshot command batch against the live window.",
        true,
        LiveCodeBridgeLease.ExclusiveInput,
        Schema());

    /// <inheritdoc />
    public void Run(LiveCodeBridgeContext context, JsonElement request)
    {
        var commands = ReadCommands(request);
        var result = puppet.Run(commands);
        context.Value("commandsExecuted", result.CommandsExecuted);
        context.Value("time", result.Time);
        context.Value("updates", result.Updates);
        context.Value("renders", result.Renders);
        context.Value("mouse", new[] { result.Mouse.X, result.Mouse.Y });

        foreach (var line in result.Output.Split(
            ['\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries))
        {
            context.WriteLine(line);
        }

        foreach (var artifact in result.Artifacts)
            context.Artifact(artifact.Name, "image/png", artifact.Png);
    }

    private static string[] ReadCommands(JsonElement request)
    {
        if (request.ValueKind != JsonValueKind.Object
            || !request.TryGetProperty("commands", out var commands)
            || commands.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("AlvorSense bridge payload requires a 'commands' string array.");
        }

        var result = new List<string>();
        foreach (var command in commands.EnumerateArray())
        {
            if (command.ValueKind != JsonValueKind.String)
                throw new InvalidOperationException("Every AlvorSense command must be a string.");

            var value = command.GetString() ?? string.Empty;
            if (value.Length > MaximumCommandCharacters)
                throw new InvalidOperationException($"An AlvorSense command exceeds {MaximumCommandCharacters} characters.");
            if (!string.IsNullOrWhiteSpace(value))
                result.Add(value);
            if (result.Count > MaximumCommands)
                throw new InvalidOperationException($"An AlvorSense batch exceeds {MaximumCommands} commands.");
        }

        return [.. result];
    }

    private static JsonElement Schema() =>
        JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                commands = new
                {
                    type = "array",
                    maxItems = MaximumCommands,
                    items = new
                    {
                        type = "string",
                        maxLength = MaximumCommandCharacters
                    }
                }
            },
            required = new[] { "commands" },
            additionalProperties = false
        });
}
