namespace AlvorKit;

/// <summary>Tests the coordinate-axis and signed-axis-direction enum contracts.</summary>
[TestClass]
public class AxisTest
{
    /// <summary>Axis values match component indexes followed by the dimension count.</summary>
    [TestMethod]
    public void Axes_MatchComponentIndexesAndCounts()
    {
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2 }, Enum.GetValues<Axis2>().Select(value => (byte)value).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3 }, Enum.GetValues<Axis3>().Select(value => (byte)value).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4 },
            Enum.GetValues<Axis4>().Select(value => (byte)value).ToArray());
    }

    /// <summary>Direction values retain opposite pairs followed by the exclusive count sentinel.</summary>
    [TestMethod]
    public void AxisDirections_RetainOppositePairsAndCounts()
    {
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4 },
            Enum.GetValues<AxisDirection2>().Select(value => (byte)value).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6 },
            Enum.GetValues<AxisDirection3>().Select(value => (byte)value).ToArray());
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
            Enum.GetValues<AxisDirection4>().Select(value => (byte)value).ToArray());
    }
}
