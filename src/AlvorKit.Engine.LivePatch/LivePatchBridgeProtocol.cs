namespace AlvorKit.Engine;

/// <summary>Defines the LivePatch bridge schema and validates its JSON primitives.</summary>
internal sealed class LivePatchBridgeProtocol
{
    /// <summary>Gets the versioned bridge descriptor advertised to LiveCode clients.</summary>
    internal LiveCodeBridgeDescriptor Descriptor { get; } = new(
        "livepatch",
        1,
        "Install, inspect, atomically replace, and remove exact-signature C# handlers selected by injector ownership.",
        true,
        LiveCodeBridgeLease.None,
        JsonSerializer.SerializeToElement(new
        {
            type = "object",
            required = new[] { "operation" },
            properties = new
            {
                operation = new
                {
                    @enum = new[]
                    {
                        "capabilities",
                        "list",
                        "status",
                        "install",
                        "replace",
                        "remove"
                    }
                },
                patchId = new { type = "integer" },
                executorScopeId = new { type = "integer" },
                selector = new
                {
                    type = "object",
                    properties = new
                    {
                        kind = new
                        {
                            @enum = new[]
                            {
                                "exactInstance",
                                "exactScope",
                                "descendants",
                                "all"
                            }
                        },
                        scopeId = new { type = "integer" }
                    }
                },
                target = new
                {
                    type = "object",
                    properties = new
                    {
                        assembly = new { type = "string" },
                        type = new { type = "string" },
                        method = new { type = "string" }
                    }
                },
                entryType = new { type = "string" },
                assembly = new { type = "string", contentEncoding = "base64" },
                symbols = new { type = "string", contentEncoding = "base64" },
                name = new { type = "string" }
            }
        }));

    /// <summary>Reads a required, non-empty string property.</summary>
    internal string RequiredString(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.ValueKind == JsonValueKind.String &&
        !string.IsNullOrWhiteSpace(element.GetString())
            ? element.GetString()!
            : throw new ArgumentException($"LivePatch request requires string '{name}'.");

    /// <summary>Reads an optional string property.</summary>
    internal string? OptionalString(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.ValueKind == JsonValueKind.String
            ? element.GetString()
            : null;

    /// <summary>Reads a required signed integer property.</summary>
    internal long RequiredInt64(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.TryGetInt64(out var result)
            ? result
            : throw new ArgumentException($"LivePatch request requires integer '{name}'.");

    /// <summary>Reads a required unsigned integer property.</summary>
    internal ulong RequiredUInt64(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.TryGetUInt64(out var result)
            ? result
            : throw new ArgumentException($"LivePatch request requires unsigned integer '{name}'.");

    /// <summary>Decodes a required base64 property.</summary>
    internal byte[] RequiredBytes(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.ValueKind == JsonValueKind.String
            ? element.GetBytesFromBase64()
            : throw new ArgumentException($"LivePatch request requires base64 '{name}'.");

    /// <summary>Decodes an optional base64 property.</summary>
    internal byte[]? OptionalBytes(JsonElement value, string name) =>
        value.TryGetProperty(name, out var element) &&
        element.ValueKind == JsonValueKind.String
            ? element.GetBytesFromBase64()
            : null;
}
