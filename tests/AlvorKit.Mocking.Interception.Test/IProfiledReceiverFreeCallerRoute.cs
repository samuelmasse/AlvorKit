namespace AlvorKit;

/// <summary>Exposes exact wrapper-entry evidence for one receiver-free caller.</summary>
internal interface IProfiledReceiverFreeCallerRoute :
    IProfiledOwnedCallerRoute
{
    /// <summary>Gets the number of calls that entered the production wrapper.</summary>
    int HandlerInvocations { get; }
}
