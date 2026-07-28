namespace AlvorKit.Mocking;

/// <summary>Keys one exact interception wrapper artifact without site or mock state.</summary>
internal sealed record MockInterceptionWrapperCacheKey(
    MockDispatchCacheKey Dispatch,
    Type DelegateType);
