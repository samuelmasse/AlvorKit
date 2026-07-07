namespace AlvorKit.Script.AlvorSense;

/// <summary>Shared environment variables written by AlvorSense target launches.</summary>
internal static class AlvorSenseEnvironment
{
    /// <summary>Environment variable that asks AlvorKit audio startup to avoid real audio output.</summary>
    internal const string AudioSilentVariable = "ALVORKIT_AUDIO_SILENT";

    /// <summary>Enabled value used for boolean AlvorSense environment switches.</summary>
    internal const string EnabledValue = "1";
}
