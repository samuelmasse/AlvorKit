namespace AlvorKit.Mocking;

/// <summary>Identifies why the current thread is capturing a mocked call.</summary>
internal enum CaptureOperation
{
    /// <summary>The captured call will publish a setup.</summary>
    Setup,

    /// <summary>The captured call will select invocation history.</summary>
    Verification,

    /// <summary>The captured event accessor will locate handlers to raise.</summary>
    Event
}
