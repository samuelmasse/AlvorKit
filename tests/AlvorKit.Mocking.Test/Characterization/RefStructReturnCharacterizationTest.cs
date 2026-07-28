namespace AlvorKit.Mocking.Test.Characterization;

[TestClass]
public class RefStructReturnCharacterizationTest
{
    /// <summary>Confirms strict proxy ref-struct returns use interception instead of an implicit default.</summary>
    [TestMethod]
    public void ProxyRefStructReturn_StrictFallbackThrows()
    {
        var mock = Mock.Create<ProxyRefStructReturnTarget>();

        Assert.Throws<MockException>(() => mock.Read());

        Assert.AreEqual(0, mock.Calls);
    }

}
