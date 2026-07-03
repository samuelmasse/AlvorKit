namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootRuntimeModelTest
{
    /// <summary>The base state lifecycle and rendering callbacks are safe no-ops.</summary>
    [TestMethod]
    public void State_DefaultCallbacks_AreNoOps()
    {
        var state = new State();

        state.Load();
        state.Update(1);
        state.Frame(2);
        state.Draw();
        state.Render();
        state.Unload();
    }

    /// <summary>The base script inherits state callbacks and defaults to order zero and full-canvas drawing.</summary>
    [TestMethod]
    public void Script_DefaultCallbacks_AreNoOps()
    {
        var script = new Script();

        script.Load();
        script.Update(1);
        script.Frame(2);
        script.Draw();
        script.Render();
        script.Unload();

        Assert.AreEqual(0, script.Order);
        Assert.IsNull(script.DrawArea);
    }

    /// <summary>Scripts can override the draw area used for two-dimensional rendering.</summary>
    [TestMethod]
    public void Script_DrawArea_CanBeOverridden()
    {
        var script = new DrawAreaScript();

        Assert.AreEqual(new Vec2(80, 45), script.DrawArea);
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

    /// <summary>Root metrics exposes the old frame metric and timer-backed frame window.</summary>
    [TestMethod]
    public void RootMetrics_ExposesFrameMetricAndWindow()
    {
        var metrics = new RootMetrics();

        metrics.FrameMetric.Start();
        metrics.FrameMetric.End();
        metrics.Start();
        metrics.Stop();

        Assert.AreEqual(1, metrics.Frame.Ticks);
        Assert.IsTrue(metrics.Frame.Last >= 0);
    }

    private sealed class TrackingState : State
    {
        internal int Loads { get; private set; }

        internal int Unloads { get; private set; }

        public override void Load() => Loads++;

        public override void Unload() => Unloads++;
    }

    private sealed class DrawAreaScript : Script
    {
        public override Vec2? DrawArea => new(80, 45);
    }
}
