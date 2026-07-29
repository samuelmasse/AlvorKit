namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Runtime boundary used to apply one compiler-produced metadata delta.</summary>
internal interface ISourceUpdateRuntime
{
    bool IsSupported { get; }

    void ApplyUpdate(
        Assembly assembly,
        byte[] metadataDelta,
        byte[] ilDelta,
        byte[] pdbDelta);
}

/// <summary>Production metadata-update runtime backed directly by CoreCLR.</summary>
internal sealed class SourceUpdateRuntime : ISourceUpdateRuntime
{
    public bool IsSupported => MetadataUpdater.IsSupported;

    public void ApplyUpdate(
        Assembly assembly,
        byte[] metadataDelta,
        byte[] ilDelta,
        byte[] pdbDelta) =>
        MetadataUpdater.ApplyUpdate(
            assembly,
            metadataDelta,
            ilDelta,
            pdbDelta);
}
