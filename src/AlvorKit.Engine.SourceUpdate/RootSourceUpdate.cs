namespace AlvorKit.Engine.SourceUpdate;

/// <summary>Explicitly composes the editable-source bridge into one development LiveCode session.</summary>
public sealed class RootSourceUpdate(
    LiveCodeBridgeRegistry bridges,
    SourceUpdateHostOptions options)
{
    private bool enabled;

    /// <summary>Registers the exact allowlisted module ledger and Source Update bridge.</summary>
    public SourceUpdateModuleLedger Enable()
    {
        if (enabled)
            throw new InvalidOperationException("Root Source Update has already been enabled.");

        var ledger = new SourceUpdateModuleLedger(options);
        bridges.Register(new SourceUpdateBridge(ledger));
        enabled = true;
        return ledger;
    }
}
