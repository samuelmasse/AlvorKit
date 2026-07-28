namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Controls one exact profiled caller behind a coordinator route.</summary>
internal interface IProfiledOwnedCallerRoute
{
    /// <summary>Gets the completion that installed the inert caller body.</summary>
    InterceptionCompletion? PreparationCompletion { get; }

    /// <summary>Gets the completion that restored the original caller body.</summary>
    InterceptionCompletion? RemovalCompletion { get; }

    /// <summary>Prepares an inert exact caller route.</summary>
    MockInterceptionPreparationDiagnostic? Prepare(
        MockInterceptionRoute route);

    /// <summary>Publishes the route pointer behind the coordinator gate.</summary>
    MockInterceptionPreparationDiagnostic? Activate(
        MockInterceptionRoute route);

    /// <summary>Restores the selected caller and retires the route.</summary>
    void Rollback(MockInterceptionRoute route);
}
