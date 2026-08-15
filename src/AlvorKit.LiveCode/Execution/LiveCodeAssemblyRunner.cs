namespace AlvorKit;

/// <summary>Loads and invokes compiled commands against exact active graph scopes.</summary>
internal sealed class LiveCodeAssemblyRunner(InjectorScopeGraph graph)
{
    internal LiveCodeExecutionResult Run(LiveCodePendingExecution pending)
        => Run(pending.ScopeId, pending.EntryType, pending.Assembly, pending.Symbols);

    internal LiveCodeExecutionResult Run(
        long scopeId,
        string entryType,
        byte[] assembly,
        byte[]? symbols)
    {
        if (!graph.TryGetActiveScope(new(scopeId), out var scope))
        {
            return Failure(
                scopeId,
                LiveCodeExecutionStatus.ScopeEnded,
                "The selected injector scope is no longer active.");
        }

        var timer = Stopwatch.StartNew();
        var loadContext = new LiveCodeLoadContext();
        try
        {
            using var assemblyStream = new MemoryStream(assembly, writable: false);
            using var symbolsStream = symbols is null
                ? null
                : new MemoryStream(symbols, writable: false);
            var loadedAssembly = symbolsStream is null
                ? loadContext.LoadFromStream(assemblyStream)
                : loadContext.LoadFromStream(assemblyStream, symbolsStream);
            var type = loadedAssembly.GetType(entryType, throwOnError: false);
            if (type is null)
            {
                return Failure(
                    scopeId,
                    LiveCodeExecutionStatus.InvalidCommand,
                    $"Entry type '{entryType}' was not found.",
                    timer);
            }

            if (type.IsAbstract || type.IsInterface || !typeof(ILiveCodeCommand).IsAssignableFrom(type))
            {
                return Failure(
                    scopeId,
                    LiveCodeExecutionStatus.InvalidCommand,
                    $"Entry type '{entryType}' must be a concrete {nameof(ILiveCodeCommand)}.",
                    timer);
            }

            var command = (ILiveCodeCommand)scope.New(type);
            var output = new LiveCodeContext();
            command.Run(output);
            timer.Stop();
            return new(
                LiveCodeExecutionStatus.Completed,
                scopeId,
                output.Lines(),
                output.Values(),
                timer.Elapsed.TotalMilliseconds,
                null,
                null,
                null);
        }
        catch (Exception exception)
        {
            var observed = exception is TargetInvocationException { InnerException: not null }
                ? exception.InnerException
                : exception;
            return new(
                LiveCodeExecutionStatus.Failed,
                scopeId,
                [],
                [],
                timer.Elapsed.TotalMilliseconds,
                observed.Message,
                observed.GetType().FullName,
                observed.StackTrace);
        }
        finally
        {
            loadContext.Unload();
        }
    }

    private static LiveCodeExecutionResult Failure(
        long scopeId,
        LiveCodeExecutionStatus status,
        string error,
        Stopwatch? timer = null) =>
        new(
            status,
            scopeId,
            [],
            [],
            timer?.Elapsed.TotalMilliseconds ?? 0,
            error,
            null,
            null);
}
