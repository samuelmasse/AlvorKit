namespace AlvorKit;

[TestClass]
public sealed class MockStrictTest
{
    /// <summary>Verifies generic interface full mocks are strict by default.</summary>
    [TestMethod]
    public void Create_GenericFullMocks_DefaultToStrict()
    {
        var interfaceMock = Mock.Create<IMockTarget>();
        Assert.Throws<MockException>(() => interfaceMock.GetValue());
    }

    /// <summary>Verifies runtime-type full mocks are strict unless loose behavior is requested.</summary>
    [TestMethod]
    public void Create_RuntimeFullMock_DefaultsToStrict()
    {
        var mock = (IMockTarget)Mock.Create(typeof(IMockTarget));

        Assert.Throws<MockException>(() => mock.GetValue());
    }

    /// <summary>Verifies generic and runtime-type loose construction return loose defaults.</summary>
    [TestMethod]
    public void Create_ExplicitLoose_ReturnsDefaults()
    {
        var genericMock = Mock.CreateLoose<IMockTarget>();
        var runtimeMock = (IMockTarget)Mock.Create(
            typeof(IMockTarget),
            MockBehavior.Loose);

        Assert.AreEqual(0, genericMock.GetValue());
        Assert.AreEqual(0, runtimeMock.GetValue());
    }

    /// <summary>Verifies partial construction returns the existing instance and preserves passthrough.</summary>
    [TestMethod]
    public void Partial_ExistingInstance_ReturnsSameAndPassesThrough()
    {
        var instance = new ClassMock("original");

        var partial = Mock.Partial(instance);

        Assert.AreSame(instance, partial);
        Assert.AreEqual("original", partial.Name);
        Assert.AreEqual(12, partial.ReturnDouble(6));
    }

    /// <summary>Verifies partial construction rejects a second attachment.</summary>
    [TestMethod]
    public void Partial_AlreadyPartial_ThrowsMockException()
    {
        var instance = new ClassMock("original");
        Mock.Partial(instance);

        Assert.Throws<MockException>(() => Mock.Partial(instance));
    }

    /// <summary>Verifies partial construction rejects a full mock.</summary>
    [TestMethod]
    public void Partial_FullMock_ThrowsMockException()
    {
        var mock = Mock.Create<ClassMock>();

        Assert.Throws<MockException>(() => Mock.Partial(mock));
    }

    /// <summary>Verifies an unmatched value method fails on a strict mock.</summary>
    [TestMethod]
    public void Strict_UnmatchedValueMethod_ThrowsMockException()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<MockException>(() => mock.GetValue());
    }

    /// <summary>Verifies an unmatched void method fails on a strict mock.</summary>
    [TestMethod]
    public void Strict_UnmatchedVoidMethod_ThrowsMockException()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<MockException>(() => mock.RaiseEvent());
    }

    /// <summary>Verifies an unmatched property getter fails on a strict mock.</summary>
    [TestMethod]
    public void Strict_UnmatchedGetter_ThrowsMockException()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<MockException>(() => _ = mock.Property);
    }

    /// <summary>Verifies an unmatched property setter fails on a strict mock.</summary>
    [TestMethod]
    public void Strict_UnmatchedSetter_ThrowsMockException()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<MockException>(() => mock["key"] = 42);
    }
}
