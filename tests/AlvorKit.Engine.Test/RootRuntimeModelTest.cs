namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootRuntimeModelTest
{
    /// <summary>The base state stores draw area and its callbacks are safe no-ops.</summary>
    [TestMethod]
    public void State_DefaultCallbacks_AreNoOps()
    {
        var state = new State { DrawArea = new Vec2(10, 20) };

        state.Load();
        state.Update(1);
        state.Frame(2);
        state.Draw();
        state.Render();
        state.Unload();

        Assert.AreEqual(new Vec2(10, 20), state.DrawArea);
    }

    /// <summary>The base script stores priority and draw area while its callbacks are safe no-ops.</summary>
    [TestMethod]
    public void Script_DefaultCallbacks_AreNoOps()
    {
        var script = new Script { Priority = 7, DrawArea = new Vec2(30, 40) };

        script.Load();
        script.Update(1);
        script.Frame(2);
        script.Draw();
        script.Render();
        script.Unload();

        Assert.AreEqual(7, script.Priority);
        Assert.AreEqual(new Vec2(30, 40), script.DrawArea);
    }

    /// <summary>Root state unloads the previous state and loads the replacement.</summary>
    [TestMethod]
    public void RootState_Current_TransitionsStateLifecycle()
    {
        var rootState = new RootState();
        var first = new TrackingState();
        var second = new TrackingState();

        rootState.Current = first;
        rootState.Current = second;

        Assert.AreSame(second, rootState.Current);
        Assert.AreEqual(1, first.Loads);
        Assert.AreEqual(1, first.Unloads);
        Assert.AreEqual(1, second.Loads);
        Assert.AreEqual(0, second.Unloads);
    }

    /// <summary>Root args expose startup settings without renaming root ownership.</summary>
    [TestMethod]
    public void RootArgs_Properties_ReturnConfiguredValues()
    {
        using var gl = new RootGl(new GlNoop());
        var host = new FakeWindowHost();
        var args = new RootArgs { Window = host, Gl = gl, BootState = typeof(State), Failsafe = false };

        Assert.AreSame(host, args.Window);
        Assert.AreSame(gl, args.Gl);
        Assert.AreEqual(typeof(State), args.BootState);
        Assert.IsFalse(args.Failsafe);
    }

    /// <summary>Root shutdown delegates state to the root screen close path.</summary>
    [TestMethod]
    public void RootShutdown_Start_ClosesScreen()
    {
        var host = new FakeWindowHost();
        var shutdown = new RootShutdown(new(new(host)));

        Assert.IsFalse(shutdown.Started);
        shutdown.Start();

        Assert.IsTrue(shutdown.Started);
    }

    /// <summary>Root metrics exposes update, frame, and elapsed-time tracking.</summary>
    [TestMethod]
    public void RootMetrics_StartStop_TracksElapsedAndSamples()
    {
        var metrics = new RootMetrics();

        metrics.Start();
        metrics.Update.Add(0.25);
        metrics.Frame.Add(0.5);
        metrics.Stop();

        Assert.IsTrue(metrics.Elapsed >= TimeSpan.Zero);
        Assert.AreEqual(0.25, metrics.Update[0].Now);
        Assert.AreEqual(0.5, metrics.Frame[0].Now);
    }

    private sealed class TrackingState : State
    {
        internal int Loads { get; private set; }

        internal int Unloads { get; private set; }

        public override void Load() => Loads++;

        public override void Unload() => Unloads++;
    }
}
