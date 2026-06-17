namespace AlvorKit.Mocking.Test;

public abstract class MockPropertyTest<T> where T : class, IMockTarget
{
    /// <summary>Verifies Property Get ReturnsMockedValue.</summary>
    [TestMethod]
    public void Property_Get_ReturnsMockedValue()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.Property).Return(123);

        var result = mock.Property;

        Assert.AreEqual(123, result);
    }

    /// <summary>Verifies PtrProperty Get ReturnsMockedValue.</summary>
    [TestMethod]
    public unsafe void PtrProperty_Get_ReturnsMockedValue()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => (nint)mock.PtrProperty).Return(123);

        Assert.AreEqual(123, (nint)mock.PtrProperty);
    }

    /// <summary>Verifies RefProperty Get ReturnsMockedValue.</summary>
    [TestMethod]
    public void RefProperty_Get_ReturnsMockedValue()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.RefProperty).Return(321);

        ref var result = ref mock.RefProperty;

        Assert.AreEqual(321, result);
    }

    /// <summary>Verifies RefPtrProperty Get ReturnsMockedValue.</summary>
    [TestMethod]
    public unsafe void RefPtrProperty_Get_ReturnsMockedValue()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => (nint)mock.RefPtrProperty).Return(321);

        ref var result = ref mock.RefPtrProperty;

        Assert.AreEqual(321, (nint)result);
    }

    /// <summary>Verifies Indexer Get ReturnsMockedValue.</summary>
    [TestMethod]
    public void Indexer_Get_ReturnsMockedValue()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock["key"]).Return(764);

        var result = mock["key"];

        Assert.AreEqual(764, result);
    }
}

[TestClass]
public class IMockTargetPropertyTest : MockPropertyTest<IMockTarget>;

[TestClass]
public class BasicMockPropertyTest : MockPropertyTest<BasicMock>;

[TestClass]
public class GenericMockPropertyTest : MockPropertyTest<GenericMock<List<int>>>;

[TestClass]
public class AbstractMockPropertyTest : MockPropertyTest<AbstractMock>;

[TestClass]
public class PartialMockPropertyTest : MockPropertyTest<PartialMock>;

[TestClass]
public class VirtualMockPropertyTest : MockPropertyTest<VirtualMock>;

[TestClass]
public class DerivedMockPropertyTest : MockPropertyTest<DerivedMock>;

[TestClass]
public class SealedMockPropertyTest : MockPropertyTest<SealedMock>;
