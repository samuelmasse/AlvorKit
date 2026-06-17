namespace AlvorKit.Mocking.Test;

public abstract class MockTest<T> where T : class, IMockTarget
{
    /// <summary>Verifies Create GenericMock ReturnsProxy.</summary>
    [TestMethod]
    public void Create_GenericMock_ReturnsProxy()
    {
        var mock = Mock.Create<T>();
        Assert.IsNotNull(mock);
        Assert.IsInstanceOfType<T>(mock);
    }

    /// <summary>Verifies Create TypeMock ReturnsProxy.</summary>
    [TestMethod]
    public void Create_TypeMock_ReturnsProxy()
    {
        var mock = Mock.Create(typeof(T));
        Assert.IsNotNull(mock);
        Assert.IsInstanceOfType<T>(mock);
    }

    /// <summary>Verifies Create GenericAndTypeMock ReturnsSameType.</summary>
    [TestMethod]
    public void Create_GenericAndTypeMock_ReturnsSameType()
    {
        var mock1 = Mock.Create<T>();
        var mock2 = Mock.Create(typeof(T));
        Assert.AreSame(mock1.GetType(), mock2.GetType());
    }

    /// <summary>Verifies When Empty ThrowsMockException.</summary>
    [TestMethod]
    public void When_Empty_ThrowsMockException()
    {
        var mock = Mock.Create<T>();
        Assert.Throws<MockException>(() =>
            Mock.When(() => 1).Return(42));
    }

    /// <summary>Verifies When EmptyVoid ThrowsMockException.</summary>
    [TestMethod]
    public void When_EmptyVoid_ThrowsMockException()
    {
        var mock = Mock.Create<T>();
        Assert.Throws<MockException>(() =>
            Mock.When(() => { }).Return([]));
    }

    /// <summary>Verifies When MultipleCalls MocksLast.</summary>
    [TestMethod]
    public void When_MultipleCalls_MocksLast()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => { mock.GetValue(); return mock.ComputeSum(1, 2); }).Return(42);

        Assert.AreEqual(0, mock.GetValue());
        Assert.AreEqual(42, mock.ComputeSum(1, 2));
    }

    /// <summary>Verifies Mock DefaultValues AreConsistent.</summary>
    [TestMethod]
    public unsafe void Mock_DefaultValues_AreConsistent()
    {
        var mock = Mock.Create<T>();

        Assert.IsNull(mock.Action);
        Assert.IsEmpty(mock.Values);
        Assert.IsEmpty(mock.Numbers);

        Assert.AreEqual(0, mock.Property);
        Assert.AreEqual(0, (nint)mock.PtrProperty);
        Assert.AreEqual(0, mock.RefProperty);
        Assert.AreEqual(0, (nint)mock.RefPtrProperty);

        Assert.IsNotNull(mock.ChildTarget);
        Assert.IsNotNull(mock.ChildTarget.ChildTarget);
        Assert.IsNotNull(mock.ChildTarget.ChildTarget.ChildTarget);
        Assert.IsNotNull(mock.Model);
    }

    /// <summary>Verifies Mock ReferenceValues ArePersistent.</summary>
    [TestMethod]
    public void Mock_ReferenceValues_ArePersistent()
    {
        var mock = Mock.Create<T>();

        Assert.AreSame(mock.Model, mock.Model);
        Assert.AreSame(mock.ChildTarget, mock.ChildTarget);
        Assert.AreSame(mock.ChildTarget.ChildTarget, mock.ChildTarget.ChildTarget);
    }
}

[TestClass]
public class IMockTargetTest : MockTest<IMockTarget>;

[TestClass]
public class IPartialMockTargetTest : MockTest<IPartialMockTarget>;

[TestClass]
public class BasicMockTest : MockTest<BasicMock>;

[TestClass]
public class GenericMockTest : MockTest<GenericMock<List<int>>>;

[TestClass]
public class AbstractMockTest : MockTest<AbstractMock>;

[TestClass]
public class PartialMockTest : MockTest<PartialMock>;

[TestClass]
public class VirtualMockTest : MockTest<VirtualMock>;

[TestClass]
public class DerivedMockTest : MockTest<DerivedMock>;

[TestClass]
public class SealedMockTest : MockTest<SealedMock>;
