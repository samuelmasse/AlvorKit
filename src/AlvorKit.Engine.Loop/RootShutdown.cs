namespace AlvorKit;

/// <summary>Tracks whether the root loop is shutting down after a requested close or runtime failure.</summary>
[Root]
public class RootShutdown(RootFailSafeConfig failSafe, RootScreen screen)
{
    private bool started;

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

    /// <summary>Removes this root's shutdown hooks on unload.</summary>
    public void Unregister()
    {
        Console.CancelKeyPress -= OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        ShutDown(true);
    }

    private void OnProcessExit(object? sender, EventArgs e) => ShutDown(false);

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) => OnUnhandledException();

    /// <summary>Applies the root's shutdown policy when an unhandled exception ends the process.</summary>
    internal void OnUnhandledException()
    {
        if (failSafe.Enabled)
            ShutDown(false);
        else
            started = true;
    }

    /// <summary>Marks shutdown and closes the screen when it is still safe to reach the host window.</summary>
    private void ShutDown(bool cancel)
    {
        lock (this)
        {
            if (!started)
            {
                started = true;
                if ((cancel || ScreenIsStillAlive()) && !screen.IsExiting)
                    screen.Close();
            }
        }
    }

    private bool ScreenIsStillAlive()
    {
        try
        {
            _ = screen.MonitorSize;
            return true;
        }
        catch
        {
            return false;
        }
    }
}
