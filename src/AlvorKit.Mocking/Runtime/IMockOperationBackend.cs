namespace AlvorKit;

/// <summary>
/// Binds prepared concrete and receiver-free operations to executable
/// interception routes.
/// </summary>
internal interface IMockOperationBackend
{
    /// <summary>Gets the provider name used in capability diagnostics.</summary>
    string Name { get; }

    /// <summary>Binds one validated interception operation.</summary>
    TDelegate BindInterception<TDelegate>(
        MockInterceptionSiteDescriptor site,
        MemberInfo operation,
        TDelegate original)
        where TDelegate : Delegate;
}
