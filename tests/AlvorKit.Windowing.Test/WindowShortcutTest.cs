namespace AlvorKit;

/// <summary>Verifies that games can own shortcut keys without losing keyboard input.</summary>
[TestClass]
public class WindowShortcutTest
{
    /// <summary>Disabling default shortcuts preserves game input; reenabling restores the built-in toggles.</summary>
    [TestMethod]
    public void DefaultShortcuts_CanBeDisabledWithoutSuppressingInput()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);
        var screen = new WindowScreen(loop);
        var presses = 0;
        loop.Update += delta =>
        {
            if (keyboard.IsKeyPressed(Keys.F11) && keyboard.IsKeyPressed(Keys.F12))
                presses++;
        };
        Assert.IsTrue(loop.EnableDefaultShortcuts);
        loop.EnableDefaultShortcuts = false;
        host.RaiseKeyDown(Keys.F11);
        host.RaiseKeyDown(Keys.F12);
        host.RaiseUpdate();

        Assert.AreEqual(1, presses);
        Assert.IsFalse(screen.IsFullscreen);
        Assert.IsFalse(screen.IsVSyncEnabled);

        host.RaiseKeyUp(Keys.F11);
        host.RaiseKeyUp(Keys.F12);
        host.RaiseUpdate();
        loop.EnableDefaultShortcuts = true;
        host.RaiseKeyDown(Keys.F11);
        host.RaiseKeyDown(Keys.F12);
        host.RaiseUpdate();

        Assert.AreEqual(2, presses);
        Assert.IsTrue(screen.IsFullscreen);
        Assert.IsTrue(screen.IsVSyncEnabled);
    }
}
