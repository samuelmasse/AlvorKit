namespace AlvorKit.Logging;

/// <summary>Describes one timestamped log entry.</summary>
internal readonly record struct LogEntry(DateTime Time, LogLevel Level, string? File, int? Line);
