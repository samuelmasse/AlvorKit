namespace AlvorKit.Windowing.Test;

[TestClass]
public class KeyboardTest
{
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
        keyboard.Tick();
        Assert.IsTrue(keyboard.IsKeyDown(Keys.A));
    }

    [TestMethod]
    public void Keyboard_IsKeyPressed_Works()
    {
        var (host, loop) = WindowingTestFactory.Create();
        var keyboard = new Keyboard(loop);

        host.RaiseKeyDown(Keys.A);

        Assert.IsTrue(keyboard.IsKeyPressed(Keys.A));
        keyboard.Tick();
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
        keyboard.Tick();
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
