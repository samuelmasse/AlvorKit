namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Controls one exact construction behind a generic caller definition.</summary>
internal interface IProfiledGenericConstructionRoute
{
    /// <summary>Creates and binds the exact Mocking wrapper and trampoline.</summary>
    void Prepare(
        IInterceptionBackend profiler,
        MockInterceptionRoute route);

    /// <summary>Publishes the construction-specific managed route pointer.</summary>
    void Publish();

    /// <summary>Returns the construction to inert original behavior.</summary>
    void Unpublish();

    /// <summary>Clears and retires the exact construction trampoline.</summary>
    void Retire();
}
