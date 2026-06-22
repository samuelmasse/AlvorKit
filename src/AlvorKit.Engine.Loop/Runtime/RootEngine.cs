namespace AlvorKit.Engine.Loop;

/// <summary>Coordinates root state, scripts, metrics, rendering, and cleanup for a running game.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootEngine(
    RootArgs args,
    RootState state,
    RootScripts scripts,
    RootGl gl,
    RootGraphics2D graphics2D,
    RootMetrics metrics,
    RootText text,
    RootShutdown shutdown,
    RootBinEmptyer binEmptyer)
{
    /// <summary>Starts metrics and prepares shutdown state before the host loop begins.</summary>
    public void Load()
    {
        metrics.Start();
        shutdown.Register();
    }

    /// <summary>Releases root-owned rendering and script resources after the host loop ends.</summary>
    public void Unload()
    {
        state.Current = new();
        graphics2D.Unload();
        gl.Dispose();
        metrics.Stop();
        scripts.RemoveAllReverse();
    }

    /// <summary>Runs update work unless shutdown has started.</summary>
    public void Update(double delta) => Run(() =>
    {
        foreach (var script in scripts.Span)
            script.Update(delta);
        state.Current.Update(delta);
        metrics.AddUpdate(delta);
    });

    /// <summary>Runs frame work, clears transient text, and drains deferred GL deletion bins.</summary>
    public void Frame(double delta) => Run(() =>
    {
        foreach (var script in scripts.Span)
            script.Frame(delta);
        state.Current.Frame(delta);
        text.Clear();
        binEmptyer.Empty();
        metrics.AddFrame(delta);
    });

    /// <summary>Runs direct render work and two-dimensional draw flushing.</summary>
    public void Render() => Run(() =>
    {
        foreach (var script in scripts.Span)
            script.Render();
        state.Current.Render();
        text.Clear();
        graphics2D.Render();
    });

    private void Run(Action action)
    {
        if (shutdown.Started)
            return;

        if (!args.Failsafe)
        {
            action();
            return;
        }

        try
        {
            action();
        }
        catch
        {
            shutdown.Start();
            throw;
        }
    }
}
