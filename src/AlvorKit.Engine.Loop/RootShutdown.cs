namespace AlvorKit.Engine.Loop;

/// <summary>Tracks whether the root loop is shutting down after a requested close or runtime failure.</summary>
[Root]
public class RootShutdown(RootArgs args, RootScreen screen)
{
    private bool started;

    /// <summary>Gets whether shutdown has started.</summary>
    public bool Started => started;

    /// <summary>Registers process, console, and unhandled-exception shutdown hooks.</summary>
    [ExcludeFromCodeCoverage(Justification = "Registers process-wide callbacks that are not isolated for unit tests.")]
    public void Register()
    {
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            ShutDown(true);
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => ShutDown(false);
        AppDomain.CurrentDomain.UnhandledException += (_, _) =>
        {
            if (args.Failsafe)
                ShutDown(false);
            else
                started = true;
        };
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
