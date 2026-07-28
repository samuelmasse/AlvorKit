namespace AlvorKit.Mocking;

/// <summary>
/// Guards capture placeholders while heap-safe argument carriers are built.
/// </summary>
internal static class MockTypedCaptureRuntime
{
    /// <summary>
    /// Returns whether one capture placeholder must not be dereferenced while
    /// constructing the heap-safe carrier.
    /// </summary>
    internal static bool ShouldSkipArgument(int declaredIndex) =>
        Capture.Context.IsActive &&
        Capture.HasIndexedMatcher(declaredIndex);

}
