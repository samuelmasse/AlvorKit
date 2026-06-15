namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests the single-value strict state tracker.
/// </summary>
[TestClass]
public class GlStateSlotTest
{
    [TestMethod]
    public void NewSlot_IsUnset()
    {
        var slot = new GlStateSlot<int>();

        Assert.IsFalse(slot.IsSet);
        Assert.IsNull(slot.Value);
    }

    [TestMethod]
    public void Set_FromUnset_StoresValue()
    {
        var slot = new GlStateSlot<int>();

        slot.Set("Fn", 42);

        Assert.IsTrue(slot.IsSet);
        Assert.AreEqual(42, slot.Value);
    }

    [TestMethod]
    public void Set_WhenAlreadySet_Throws()
    {
        var slot = new GlStateSlot<int>();
        slot.Set("Fn", 1);

        Assert.Throws<GlAlreadySetException>(() => slot.Set("Fn", 2));
    }

    [TestMethod]
    public void Reset_AfterSet_ClearsValue()
    {
        var slot = new GlStateSlot<int>();
        slot.Set("Fn", 1);

        slot.Reset("Fn");

        Assert.IsFalse(slot.IsSet);
        Assert.IsNull(slot.Value);
    }

    [TestMethod]
    public void Reset_WhenUnset_Throws()
    {
        var slot = new GlStateSlot<int>();

        Assert.Throws<GlAlreadyUnsetException>(() => slot.Reset("Fn"));
    }
}
