namespace AlvorKit.Windowing.Test;

[TestClass]
public class ControlsTest
{
    [TestMethod]
    public void Indexer_ReturnsSameInstance()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);

        var control1 = controls["Id"];
        var control2 = controls["Id"];

        Assert.AreSame(control1, control2);
        Assert.AreEqual("Id", control1.Name);
    }

    [TestMethod]
    public void Control_WithKeyBind_RunsWhenActive()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var control = controls["Id"];
        control.Bind(new() { KeyDown = Keys.Space });

        Assert.IsFalse(control.Run());

        host.RaiseKeyDown(Keys.Space);

        Assert.IsTrue(control.Run());
    }

    [TestMethod]
    public void Control_MultipleWithKeyBind_OnlyActiveRuns()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var jump = controls["Jump"];
        var forward = controls["Forward"];
        var switchWeapon = controls["SwitchWeapon"];
        jump.Bind(new() { KeyDown = Keys.Space });
        forward.Bind(new() { KeyDown = Keys.W });
        switchWeapon.Bind(new() { MouseScroll = MouseScrollDirection.Up });

        host.RaiseKeyDown(Keys.W);
        host.RaiseMouseWheel(new(0, 1));

        Assert.IsFalse(jump.Run());
        Assert.IsTrue(forward.Run());
        Assert.IsTrue(switchWeapon.Run());
    }

    [TestMethod]
    public void Control_EmptyBinding_AlwaysRunsWhenModifiersAreUp()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var control = controls["Id"];
        control.Bind(new());

        Assert.IsTrue(control.Run());
    }

    [TestMethod]
    public void Control_EmptyBindingWithAnyModifier_AlwaysRuns()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var control = controls["Id"];
        control.Bind(new() { Alt = KeyModifierState.Any });

        host.RaiseKeyDown(Keys.LeftAlt);

        Assert.IsTrue(control.Run());
    }

    [TestMethod]
    public void Control_OnControlRuns_ShouldBeInControlFrame()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var control = controls["Id"];
        control.Bind(new());

        Assert.IsTrue(control.Run());
        Assert.IsTrue(control.Run());
        Assert.AreEqual(1, controls.Hits.Count);
        Assert.AreSame(control, controls.Hits.First().Key);
        Assert.AreEqual(2, controls.Hits.First().Value);

        controls.Tick();

        Assert.AreEqual(0, controls.Hits.Count);
    }

    [TestMethod]
    public void Control_RemoveBinding_StopsRunning()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var control = controls["Id"];
        var binding = new KeyBinding();

        control.Bind(binding);
        Assert.IsTrue(control.Run());
        control.Unbind(binding);

        Assert.IsFalse(control.Run());
    }

    [TestMethod]
    public void Control_EdgeCases_Work()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var mouse = new Mouse(loop);
        var control = new Controls(loop)["Id"];
        control.Bind(new() { KeyDown = Keys.H });
        control.Bind(new() { KeyPress = Keys.G });
        control.Bind(new() { KeyPressRepeat = Keys.J });
        control.Bind(new() { MouseScroll = MouseScrollDirection.Up });
        control.Bind(new() { MouseScroll = MouseScrollDirection.Down });

        host.RaiseKeyDown(Keys.H);
        Assert.IsTrue(control.Run());
        host.RaiseKeyUp(Keys.H);
        Assert.IsFalse(control.Run());

        host.RaiseKeyDown(Keys.G);
        Assert.IsTrue(control.Run());
        new Keyboard(loop).Tick();
        Assert.IsFalse(control.Run());

        host.RaiseKeyDown(Keys.J, true);
        Assert.IsTrue(control.Run());

        host.RaiseMouseWheel(new(0, 1));
        Assert.IsTrue(control.Run());
        mouse.Tick();
        new Keyboard(loop).Tick();
        host.RaiseMouseWheel(new(0, -1));
        Assert.IsTrue(control.Run());
    }

    [TestMethod]
    public void Control_ModifierDownOnly_ShouldRun()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var controls = new Controls(loop);
        var downControl = controls["Down"];
        var upControl = controls["Up"];
        downControl.Bind(new()
        {
            Alt = KeyModifierState.Down,
            Shift = KeyModifierState.Down,
            Control = KeyModifierState.Down
        });
        upControl.Bind(new()
        {
            Alt = KeyModifierState.Up,
            Shift = KeyModifierState.Up,
            Control = KeyModifierState.Up
        });

        Assert.IsFalse(downControl.Run());
        Assert.IsTrue(upControl.Run());

        host.RaiseKeyDown(Keys.LeftAlt);
        host.RaiseKeyDown(Keys.LeftShift);
        host.RaiseKeyDown(Keys.LeftControl);

        Assert.IsTrue(downControl.Run());
        Assert.IsFalse(upControl.Run());
    }

    [TestMethod]
    public void Binding_Equal_AreEqual()
    {
        var binding1 = new KeyBinding();
        var binding2 = new KeyBinding();
        var binding3 = binding2 with { Alt = KeyModifierState.Down };

        Assert.AreEqual(binding1, binding2);
        Assert.AreNotEqual(binding1, binding3);
    }
}
