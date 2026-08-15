namespace AlvorKit;

/// <summary>Reports source diagnostics or an invalid LiveCode entry-point shape.</summary>
internal sealed class LiveCodeCompilationException(string message) : Exception(message);
