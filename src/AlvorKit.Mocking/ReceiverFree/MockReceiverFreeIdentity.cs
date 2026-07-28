namespace AlvorKit.Mocking;

/// <summary>Identifies one session-owned receiver-free dispatch target.</summary>
internal sealed record MockReceiverFreeIdentity(
    long SessionId,
    MockInterceptionSiteDescriptor Site,
    MemberInfo Operation);
