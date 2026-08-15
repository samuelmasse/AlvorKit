namespace AlvorKit;

/// <summary>Controls which severity levels an application log accepts.</summary>
public enum LogLevel
{
    /// <summary>Suppresses every severity-prefixed entry.</summary>
    None,
    /// <summary>Suppresses every severity-prefixed entry.</summary>
    Off,
    /// <summary>Accepts fatal failures.</summary>
    Fatal,
    /// <summary>Accepts errors and more severe entries.</summary>
    Error,
    /// <summary>Accepts warnings and more severe entries.</summary>
    Warn,
    /// <summary>Accepts informational and more severe entries.</summary>
    Info,
    /// <summary>Accepts debugging and more severe entries.</summary>
    Debug,
    /// <summary>Accepts tracing and more severe entries.</summary>
    Trace,
    /// <summary>Accepts every entry.</summary>
    All
}
