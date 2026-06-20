namespace AlvorKit.Windowing.Test;

[TestClass]
public class WindowEventsTest
{
    [TestMethod]
    public void WindowPositionEvent_Position_IsExposed()
    {
        var e = new WindowPositionEvent(new(12, 34));

        Assert.AreEqual(new Vector2(12, 34), e.Position);
    }
}
