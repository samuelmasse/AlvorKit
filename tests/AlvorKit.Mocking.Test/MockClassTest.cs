namespace AlvorKit.Mocking.Test;

[TestClass]
public class ClassMockTests
{
    /// <summary>Verifies Create SpanMock DoesNotThrow.</summary>
    [TestMethod]
    public void Create_SpanMock_DoesNotThrow()
    {
        Mock.Create<SpanMock>();
    }

    /// <summary>Verifies Create OpenClassMock DoesNotThrow.</summary>
    [TestMethod]
    public void Create_OpenClassMock_DoesNotThrow()
    {
        Mock.Create<OpenClassMock>();
    }

    /// <summary>Verifies Create InternalAbstractMock WorksCorrectly.</summary>
    [TestMethod]
    public void Create_InternalAbstractMock_WorksCorrectly()
    {
        var mock = Mock.Create<InternalClassMock>();

        Mock.When(() => mock.Name).Return("Bobby");
        Mock.When(() => mock.LastName).Return("Bob");

        Assert.AreEqual("Bobby", mock.Name);
        Assert.AreEqual("Bob", mock.LastName);

        Assert.IsFalse(mock.Equals(null));
        Assert.IsTrue(mock.Equals(mock));
        Assert.IsNotNull(mock.ToString());
        Assert.AreNotEqual(0, mock.GetHashCode());
        Assert.IsNotNull(mock.GetType());
    }

    /// <summary>Verifies Create InvalidTypes ThrowsMockException.</summary>
    [TestMethod]
    public void Create_InvalidTypes_ThrowsMockException()
    {
        Assert.Throws<MockException>(() => Mock.Create(typeof(int)));
        Assert.Throws<MockException>(() => Mock.Create(typeof((int, int))));
        Assert.Throws<MockException>(() => Mock.Create(typeof(int[])));
        Assert.Throws<MockException>(() => Mock.Create(typeof(FileAccess)));
        Assert.Throws<MockException>(() => Mock.Create(typeof(Func<string>)));
    }

    /// <summary>Verifies Create InParamMock DoesNotThrow.</summary>
    [TestMethod]
    public void Create_InParamMock_DoesNotThrow()
    {
        Mock.Create<InParamMock>();
    }

    /// <summary>Verifies Create InParamMock MethodsReturnDefaults.</summary>
    [TestMethod]
    public void Create_InParamMock_MethodsReturnDefaults()
    {
        var mock = Mock.CreateLoose<InParamMock>();

        int val = 42;
        Assert.AreEqual(0, mock.Transform(in val));
        Assert.AreEqual(0, mock.Add(1, in val));
    }

    /// <summary>Verifies Create InParamMock WhenReturn Works.</summary>
    [TestMethod]
    public void Create_InParamMock_WhenReturn_Works()
    {
        var mock = Mock.Create<InParamMock>();

        int val = 5;
        Mock.When(() => mock.Transform(in val)).Return(99);

        Assert.AreEqual(99, mock.Transform(in val));
    }

    /// <summary>Verifies an unmocked object rejects setup and event capture.</summary>
    [TestMethod]
    public void Partial_UnmockedObject_ThrowsOnMockCall()
    {
        var instance = new ClassMock(string.Empty);

        Assert.Throws<MockException>(() => Mock.When(() => instance.Name).Return("Hello"));
        Assert.Throws<MockException>(() => Mock.Raise(() => instance.Event += null));
    }

    /// <summary>Verifies partial construction rejects an already attached instance.</summary>
    [TestMethod]
    public void Partial_SameInstanceTwice_ThrowsException()
    {
        var instance = new ClassMock(string.Empty);

        Mock.Partial(instance);

        Assert.Throws<MockException>(() => Mock.Partial(instance));
    }

    /// <summary>Verifies partial construction rejects a full mock.</summary>
    [TestMethod]
    public void Partial_MockObject_ThrowsException()
    {
        var mock = Mock.Create<ClassMock>();

        Assert.Throws<MockException>(() => Mock.Partial(mock));
    }
}
