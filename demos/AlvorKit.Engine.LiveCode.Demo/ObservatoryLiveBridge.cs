namespace AlvorKit;

/// <summary>Demonstrates a game-owned structured bridge beside the built-in AlvorSense and arbitrary-C# paths.</summary>
internal sealed class ObservatoryLiveBridge(UniverseColonies universe) : ILiveCodeBridge
{
    /// <inheritdoc />
    public LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "observatory",
        1,
        "Transfigure a colony, rewrite its network, or summon the Agent Aurora through stable structured operations.",
        true,
        LiveCodeBridgeLease.None,
        Schema());

    /// <inheritdoc />
    public void Run(LiveCodeBridgeContext context, JsonElement request)
    {
        if (request.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Observatory bridge payload must be a JSON object.");

        var action = RequiredString(request, "action");
        switch (action)
        {
            case "transfigure":
                Transfigure(context, request);
                break;
            case "network":
                Network(context, request);
                break;
            case "summon-aurora":
                SummonAurora(context);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown observatory action '{action}'. Expected transfigure, network, or summon-aurora.");
        }

        context.Value("activeColonies", universe.Span.Length);
        context.Value("selected", universe.Selected?.Name);
    }

    private void Transfigure(LiveCodeBridgeContext context, JsonElement request)
    {
        var name = RequiredString(request, "colony");
        var colony = universe.Find(name)
            ?? throw new InvalidOperationException($"No active colony is named '{name}'.");
        universe.Select(colony);

        if (request.TryGetProperty("spores", out var spores))
            colony.Garden.SporeCount = Math.Clamp(spores.GetInt32(), 1, 256);
        if (request.TryGetProperty("orbitRadius", out var orbitRadius))
            colony.Garden.OrbitRadius = Math.Clamp(orbitRadius.GetSingle(), 24f, 240f);
        if (request.TryGetProperty("rotationSpeed", out var rotationSpeed))
            colony.Garden.RotationSpeed = Math.Clamp(rotationSpeed.GetSingle(), -4f, 4f);
        if (request.TryGetProperty("weather", out var weather))
            colony.Sky.Weather = weather.GetString() ?? colony.Sky.Weather;
        if (request.TryGetProperty("warp", out var warp))
            colony.Sky.Warp = Math.Clamp(warp.GetSingle(), 0f, 1f);

        var burst = request.TryGetProperty("burst", out var requestedBurst)
            ? Math.Clamp(requestedBurst.GetSingle(), 0f, 5f)
            : 1.8f;
        colony.Garden.Burst(burst);
        universe.LastIntervention = $"Structured bridge transfigured {colony.Name}.";

        context.WriteLine($"Transfigured exact colony scope #{colony.Id.Value}: {colony.Name}.");
        context.Value("scopeId", colony.Id.Value);
        context.Value("spores", colony.Garden.SporeCount);
        context.Value("weather", colony.Sky.Weather);
        context.Value("warp", colony.Sky.Warp);
    }

    private void Network(LiveCodeBridgeContext context, JsonElement request)
    {
        var intensity = request.TryGetProperty("intensity", out var value)
            ? value.GetSingle()
            : 0.9f;
        universe.NetworkIntensity = Math.Clamp(intensity, 0f, 1f);
        foreach (var colony in universe.Span)
            colony.Garden.Burst(1.25f + universe.NetworkIntensity);
        universe.LastIntervention = "Structured bridge rewrote the inter-scope constellation.";

        context.WriteLine("Rewrote every active colony link in one predefined operation.");
        context.Value("networkIntensity", universe.NetworkIntensity);
    }

    private void SummonAurora(LiveCodeBridgeContext context)
    {
        var colony = universe.OpenAgentColony();
        universe.NetworkIntensity = 0.92f;
        universe.LastIntervention = "Structured bridge summoned Agent Aurora.";
        context.WriteLine($"Agent Aurora is active at scope #{colony.Id.Value}.");
        context.Value("scopeId", colony.Id.Value);
    }

    private static string RequiredString(JsonElement request, string name)
    {
        if (!request.TryGetProperty(name, out var value)
            || value.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(value.GetString()))
        {
            throw new InvalidOperationException($"Observatory bridge requires a non-empty '{name}' string.");
        }

        return value.GetString()!;
    }

    private static JsonElement Schema() =>
        JsonSerializer.SerializeToElement(new
        {
            type = "object",
            properties = new
            {
                action = new
                {
                    type = "string",
                    @enum = new[] { "transfigure", "network", "summon-aurora" }
                },
                colony = new { type = "string" },
                spores = new { type = "integer", minimum = 1, maximum = 256 },
                orbitRadius = new { type = "number", minimum = 24, maximum = 240 },
                rotationSpeed = new { type = "number", minimum = -4, maximum = 4 },
                weather = new { type = "string" },
                warp = new { type = "number", minimum = 0, maximum = 1 },
                burst = new { type = "number", minimum = 0, maximum = 5 },
                intensity = new { type = "number", minimum = 0, maximum = 1 }
            },
            required = new[] { "action" },
            additionalProperties = false
        });
}
