namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Shared stable JSON conventions for Source Update launch and bridge contracts.</summary>
internal static class SourceUpdateJson
{
    internal static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
