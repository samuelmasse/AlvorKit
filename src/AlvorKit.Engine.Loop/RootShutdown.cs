namespace AlvorKit;

/// <summary>Tracks whether the root loop is shutting down after a requested close or runtime failure.</summary>
[Root]
public class RootShutdown(RootFailSafeConfig failSafe, RootScreen screen)
{
    private volatile bool started;

    /// <summary>Gets whether shutdown has started.</summary>
    public bool Started => started;

    /// <summary>Registers process, console, and unhandled-exception shutdown hooks.</summary>
    [ExcludeFromCodeCoverage(Justification = "Registers process-wide callbacks that are not isolated for unit tests.")]
    public void Register()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    /// <summary>Ends shutdown callbacks' access to this window before the host is torn down.</summary>
    public void Unregister()
    {
        lock (this)
            started = true;

        Console.CancelKeyPress -= OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        ShutDown(true);
    }

    private void OnProcessExit(object? sender, EventArgs e) => ShutDown(true);

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) => OnUnhandledException();

    /// <summary>Applies the root's shutdown policy when an unhandled exception ends the process.</summary>
    internal void OnUnhandledException() => ShutDown(failSafe.Enabled);

    /// <summary>Marks shutdown and closes the screen when it is still safe to reach the host window.</summary>
    private void ShutDown(bool closeWindow)
    {
        lock (this)
        {
            if (started)
                return;

            started = true;
            if (closeWindow && !screen.IsExiting)
                screen.Close();
        }
    }
}
