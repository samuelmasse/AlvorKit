namespace AlvorKit.Script.AlvorEye.Test;

/// <summary>Tests Windows key mapping used by AlvorEye input injection.</summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows6.1")]
[TestClass]
public sealed class WindowsAlvorEyePlatformInputTest
{
    /// <summary>Arrow keys carry the Win32 extended-key flag on both press and release events.</summary>
    [TestMethod]
    public void KeyInput_ArrowKeys_UseExtendedFlag()
    {
        AssertExtended("left", 0x25, 0x4B);
        AssertExtended("up", 0x26, 0x48);
        AssertExtended("right", 0x27, 0x4D);
        AssertExtended("down", 0x28, 0x50);
    }

    /// <summary>Letter keys remain normal virtual-key input without the extended-key flag.</summary>
    [TestMethod]
    public void KeyInput_LetterKeys_DoNotUseExtendedFlag()
    {
        var key = WindowsAlvorEyePlatform.KeyCode("D");
        var down = WindowsAlvorEyePlatform.KeyInput(key, false);
        var up = WindowsAlvorEyePlatform.KeyInput(key, true);

        Assert.AreEqual(0x44, key.VirtualKey);
        Assert.AreEqual(0, key.ScanCode);
        Assert.IsFalse(key.Extended);
        Assert.AreEqual(0x44, down.Data.Keyboard.Vk);
        Assert.AreEqual(0, down.Data.Keyboard.Scan);
        Assert.AreEqual(0u, down.Data.Keyboard.Flags);
        Assert.AreEqual(0x0002u, up.Data.Keyboard.Flags);
    }

    /// <summary>Asserts that one mapped key is extended and writes the expected scan-code keyboard flags.</summary>
    private static void AssertExtended(string name, ushort expectedVirtualKey, ushort expectedScanCode)
    {
        var key = WindowsAlvorEyePlatform.KeyCode(name);
        var down = WindowsAlvorEyePlatform.KeyInput(key, false);
        var up = WindowsAlvorEyePlatform.KeyInput(key, true);

        Assert.AreEqual(expectedVirtualKey, key.VirtualKey);
        Assert.AreEqual(expectedScanCode, key.ScanCode);
        Assert.IsTrue(key.Extended);
        Assert.AreEqual(0, down.Data.Keyboard.Vk);
        Assert.AreEqual(expectedScanCode, down.Data.Keyboard.Scan);
        Assert.AreEqual(0x0009u, down.Data.Keyboard.Flags);
        Assert.AreEqual(0x000Bu, up.Data.Keyboard.Flags);
    }
}
