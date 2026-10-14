namespace AlvorKit;

[TestClass]
public class KeyboardTest
{
    /// <summary>A complete tap between frames produces one press, without leaving the key held.</summary>
    [TestMethod]
    public void Keyboard_TapBetweenUpdates_PreservesPress()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);
        host.RaiseKeyDown(Keys.A);
        host.RaiseKeyUp(Keys.A);

        Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
        Assert.IsTrue(keyboard.IsKeyUp(Keys.A));
        host.RaiseUpdate();
        Assert.IsFalse(keyboard.IsKeyPressed(Keys.A));

        host.RaiseKeyDown(Keys.A);
        Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
    }

    /// <summary>F11 must toggle once even when native key down and up arrive in a single event poll.</summary>
    [TestMethod]
    public void Keyboard_F11Tap_TogglesFullscreenOnce()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var screen = new WindowScreen(loop);
        host.RaiseKeyDown(Keys.F11);
        host.RaiseKeyUp(Keys.F11);
        host.RaiseUpdate();
        Assert.IsTrue(screen.IsFullscreen);
        host.RaiseUpdate();
        Assert.IsTrue(screen.IsFullscreen);
    }

    /// <summary>Keypad bindings retain independent press, repeat, and release state from the number row.</summary>
    [TestMethod]
    public void Keyboard_KeypadTransitions_AreIndependentFromNumberRow()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);
        host.RaiseKeyDown(Keys.Keypad1);
        host.RaiseKeyDown(Keys.KeypadAdd);
        Assert.IsTrue(keyboard.IsKeyPressed(Keys.Keypad1));
        Assert.IsTrue(keyboard.IsKeyDown(Keys.KeypadAdd));
        Assert.IsFalse(keyboard.IsKeyDown(Keys.D1));
        host.RaiseUpdate();
        host.RaiseKeyDown(Keys.Keypad1, true);
        Assert.IsFalse(keyboard.IsKeyPressed(Keys.Keypad1));
        Assert.IsTrue(keyboard.IsKeyPressedRepeated(Keys.Keypad1));
        host.RaiseKeyUp(Keys.Keypad1);
        host.RaiseKeyUp(Keys.KeypadAdd);
        Assert.IsTrue(keyboard.IsKeyUp(Keys.Keypad1));
        Assert.IsTrue(keyboard.IsKeyUp(Keys.KeypadAdd));
    }

    [TestMethod]
    public void Keyboard_EmptyState_HasCorrectValues()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        Assert.IsFalse(keyboard.IsKeyDown(Keys.A));
        Assert.IsTrue(keyboard.IsKeyUp(Keys.A));
        Assert.IsFalse(keyboard.IsKeyPressed(Keys.A));
        Assert.IsFalse(keyboard.IsKeyPressedRepeated(Keys.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyDown_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(Keys.A);

        Assert.IsTrue(keyboard.IsKeyDown(Keys.A));
        host.RaiseUpdate();
        Assert.IsTrue(keyboard.IsKeyDown(Keys.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyPressed_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(Keys.A);

        Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
        host.RaiseUpdate();
        Assert.IsFalse(keyboard.IsKeyPressed(Keys.A));
        host.RaiseKeyDown(Keys.A);
        Assert.IsFalse(keyboard.IsKeyPressed(Keys.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyPressedRepeated_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(Keys.A);
        Assert.IsTrue(keyboard.IsKeyPressedRepeated(Keys.A));
        host.RaiseUpdate();
        host.RaiseKeyDown(Keys.A, true);

        Assert.IsTrue(keyboard.IsKeyPressedRepeated(Keys.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyUp_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(Keys.A);
        host.RaiseKeyUp(Keys.A);

        Assert.IsTrue(keyboard.IsKeyUp(Keys.A));
    }

    [TestMethod]
    public void Keyboard_UnknownKey_DoesNotCrash()
    {
        var (host, _) = WindowingTestFactory.Create();

        host.RaiseKeyDown(Keys.Unknown);
        host.RaiseKeyUp(Keys.Unknown);
    }

    [TestMethod]
    public void Keyboard_BadKey_ThrowsException()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        Assert.ThrowsException<InvalidOperationException>(() => keyboard.IsKeyDown(Keys.Unknown));
    }

    [TestMethod]
    public void Keyboard_Text_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseText(new('a'));
        host.RaiseText(new('z'));

        Assert.AreEqual(2, keyboard.Text.Count);
        Assert.AreEqual(new Rune('a'), keyboard.Text[0]);
        Assert.AreEqual(new Rune('z'), keyboard.Text[1]);

        host.RaiseUpdate();

        Assert.AreEqual(0, keyboard.Text.Count);
    }
}
