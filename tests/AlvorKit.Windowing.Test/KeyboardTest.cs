namespace AlvorKit.Windowing.Test;

[TestClass]
public class KeyboardTest
{
    [TestMethod]
    public void Keyboard_EmptyState_HasCorrectValues()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        Assert.IsFalse(keyboard.IsKeyDown(WindowKey.A));
        Assert.IsTrue(keyboard.IsKeyUp(WindowKey.A));
        Assert.IsFalse(keyboard.IsKeyPressed(WindowKey.A));
        Assert.IsFalse(keyboard.IsKeyPressedRepeated(WindowKey.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyDown_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(WindowKey.A);

        Assert.IsTrue(keyboard.IsKeyDown(WindowKey.A));
        keyboard.Tick();
        Assert.IsTrue(keyboard.IsKeyDown(WindowKey.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyPressed_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(WindowKey.A);

        Assert.IsTrue(keyboard.IsKeyPressed(WindowKey.A));
        keyboard.Tick();
        Assert.IsFalse(keyboard.IsKeyPressed(WindowKey.A));
        host.RaiseKeyDown(WindowKey.A);
        Assert.IsFalse(keyboard.IsKeyPressed(WindowKey.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyPressedRepeated_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(WindowKey.A);
        Assert.IsTrue(keyboard.IsKeyPressedRepeated(WindowKey.A));
        keyboard.Tick();
        host.RaiseKeyDown(WindowKey.A, true);

        Assert.IsTrue(keyboard.IsKeyPressedRepeated(WindowKey.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyUp_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(WindowKey.A);
        host.RaiseKeyUp(WindowKey.A);

        Assert.IsTrue(keyboard.IsKeyUp(WindowKey.A));
    }

    [TestMethod]
    public void Keyboard_UnknownKey_DoesNotCrash()
    {
        var (host, _) = WindowingTestFactory.Create();

        host.RaiseKeyDown(WindowKey.Unknown);
        host.RaiseKeyUp(WindowKey.Unknown);
    }

    [TestMethod]
    public void Keyboard_BadKey_ThrowsException()
    {
        var (_, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        Assert.ThrowsException<InvalidOperationException>(() => keyboard.IsKeyDown(WindowKey.Unknown));
    }

    [TestMethod]
    public void Keyboard_TextAndClipboard_Work()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.Clipboard = "bob";
        host.RaiseText(new('a'));
        host.RaiseText(new('z'));

        Assert.AreEqual("bob", keyboard.Clipboard);
        Assert.AreEqual(2, keyboard.Text.Count);
        Assert.AreEqual(new Rune('a'), keyboard.Text[0]);
        Assert.AreEqual(new Rune('z'), keyboard.Text[1]);

        keyboard.Clipboard = "alice";
        keyboard.Tick();

        Assert.AreEqual("alice", host.Clipboard);
        Assert.AreEqual(0, keyboard.Text.Count);
    }
}
