namespace AlvorKit.Script.AlvorEye;

/// <summary>Parses timeline and JSONL session actions.</summary>
internal static class AlvorEyeActionParser
{
    /// <summary>Parses an array of actions.</summary>
    /// <param name="element">JSON array containing action objects.</param>
    public static IReadOnlyList<AlvorEyeAction> ParseActions(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("Actions must be an array.");

        return element.EnumerateArray().Select(ParseAction).ToArray();
    }

    /// <summary>Parses one JSONL line as one action or an action batch.</summary>
    /// <param name="jsonLine">JSONL command line.</param>
    public static IReadOnlyList<AlvorEyeAction> ParseJsonLine(string jsonLine)
    {
        using var document = JsonDocument.Parse(jsonLine);
        var root = document.RootElement;
        if (root.TryGetProperty("actions", out var actions))
            return ParseActions(actions);
        return [ParseAction(root)];
    }

    /// <summary>Parses a single action object.</summary>
    public static AlvorEyeAction ParseAction(JsonElement element)
    {
        var kindName = ScenarioJson.String(element, "action")
            ?? ScenarioJson.String(element, "kind")
            ?? throw new ArgumentException("Action requires an action or kind property.");
        var kind = ParseKind(kindName);
        ValidateRequiredFields(kind, element);
        return new()
        {
            Kind = kind,
            Name = ScenarioJson.String(element, "name"),
            Delay = ScenarioJson.Duration(element, "seconds", "milliseconds", TimeSpan.Zero),
            Key = ScenarioJson.String(element, "key"),
            Text = ScenarioJson.String(element, "text"),
            X = ScenarioJson.Int(element, "x") ?? 0,
            Y = ScenarioJson.Int(element, "y") ?? 0,
            ToX = ScenarioJson.Int(element, "toX") ?? 0,
            ToY = ScenarioJson.Int(element, "toY") ?? 0,
            Button = ScenarioJson.String(element, "button") ?? "left",
            CaptureBeforeFreeze = ScenarioJson.Bool(element, "captureBeforeFreeze", true),
            CaptureAfterFreeze = ScenarioJson.Bool(element, "captureAfterFreeze", true),
            CompareTo = ScenarioJson.String(element, "compareTo"),
            Color = ScenarioJson.String(element, "color")
        };
    }

    /// <summary>Parses action names into enum values.</summary>
    private static AlvorEyeActionKind ParseKind(string name) =>
        name switch
        {
            "wait" => AlvorEyeActionKind.Wait,
            "capture" => AlvorEyeActionKind.Capture,
            "key" => AlvorEyeActionKind.Key,
            "keyDown" => AlvorEyeActionKind.KeyDown,
            "keyUp" => AlvorEyeActionKind.KeyUp,
            "text" => AlvorEyeActionKind.Text,
            "mouseMove" => AlvorEyeActionKind.MouseMove,
            "mouseClick" => AlvorEyeActionKind.MouseClick,
            "mouseDrag" => AlvorEyeActionKind.MouseDrag,
            "handoff" => AlvorEyeActionKind.Handoff,
            "resume" => AlvorEyeActionKind.Resume,
            "analyzeBasic" => AlvorEyeActionKind.AnalyzeBasic,
            _ => throw new ArgumentException($"Unknown AlvorEye action '{name}'.")
        };

    /// <summary>Rejects action objects that cannot execute safely.</summary>
    private static void ValidateRequiredFields(AlvorEyeActionKind kind, JsonElement element)
    {
        if (kind is AlvorEyeActionKind.Key or AlvorEyeActionKind.KeyDown or AlvorEyeActionKind.KeyUp &&
            ScenarioJson.String(element, "key") is null)
            throw new ArgumentException($"{kind} requires key.");
        if (kind == AlvorEyeActionKind.Text && ScenarioJson.String(element, "text") is null)
            throw new ArgumentException("text requires text.");
    }
}
