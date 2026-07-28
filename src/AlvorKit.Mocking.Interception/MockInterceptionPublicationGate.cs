namespace AlvorKit.Mocking.Interception;

/// <summary>Publishes or blocks one complete transaction's routes atomically.</summary>
internal sealed class MockInterceptionPublicationGate
{
    /// <summary>Zero while blocked and one after complete publication.</summary>
    private int published;

    /// <summary>Gets whether every route in the transaction is ready for use.</summary>
    internal bool IsPublished => Volatile.Read(ref published) == 1;

    /// <summary>Publishes every ready route in this transaction at once.</summary>
    internal void Publish() =>
        Volatile.Write(ref published, 1);

    /// <summary>Blocks all route use before rollback begins.</summary>
    internal void Unpublish() =>
        Volatile.Write(ref published, 0);
}
