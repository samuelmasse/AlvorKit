namespace AlvorKit.Mocking.Test;

[TestClass]
public class ClassMockTests
{
    /// <summary>Verifies Create TwoMocksAndInstance ReturnsCorrectValues.</summary>
    [TestMethod]
    public void Create_TwoMocksAndInstance_ReturnsCorrectValues()
    {
        var mock1 = Mock.Create<ClassMock>();
        var mock2 = Mock.Create<ClassMock>();
        var instance = new ClassMock("Bob");

        Assert.AreEqual(string.Empty, mock1.Name);
        Assert.AreEqual(string.Empty, mock2.Name);
        Assert.AreEqual("Bob", instance.Name);

        Mock.When(() => mock1.Name).Return("Hello");

        Assert.AreEqual("Hello", mock1.Name);
        Assert.AreEqual(string.Empty, mock2.Name);
        Assert.AreEqual("Bob", instance.Name);

        Mock.When(() => mock2.Name).Return("There");

        Assert.AreEqual("Hello", mock1.Name);
        Assert.AreEqual("There", mock2.Name);
        Assert.AreEqual("Bob", instance.Name);
    }

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

    /// <summary>Verifies Create GenericMockWithValueType Works.</summary>
    [TestMethod]
    public void Create_GenericMockWithValueType_Works()
    {
        var mock = Mock.Create<GenericClassMock<int>>();

        Mock.When(() => mock.Value).Return(1);
        Mock.When(() => mock.Method()).Return(3434);

        Assert.AreEqual(1, mock.Value);
        Assert.AreEqual(3434, mock.Method());
    }

    /// <summary>Verifies Create GenericMockWithReferenceType Works.</summary>
    [TestMethod]
    public void Create_GenericMockWithReferenceType_Works()
    {
        var mock = Mock.Create<GenericClassMock<List<object>>>();

        var list1 = new List<object>();
        var list2 = new List<object>();

        Mock.When(() => mock.Value).Return(list1);
        Mock.When(() => mock.Method()).Return(list2);

        Assert.AreSame(list1, mock.Value);
        Assert.AreSame(list2, mock.Method());
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

    /// <summary>Verifies Create Long Works.</summary>
    [TestMethod]
    public void Create_Long_Works()
    {
        var mock = Mock.Create<LongClassMock>();

        int[] ints = [17, 18];
        uint[] uints = [19, 20];
        short[] shorts = [21, 22];
        ushort[] ushorts = [23, 24];
        long[] longs = [25, 26];
        ulong[] ulongs = [27, 28];
        sbyte[] sbytes = [29, 30];
        byte[] bytes = [31, 32];

        Mock.When(() => mock.Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            ref ints[0], ref uints[0], ref shorts[0], ref ushorts[0], ref longs[0], ref ulongs[0], ref sbytes[0], ref bytes[0],
            ref ints[1], ref uints[1], ref shorts[1], ref ushorts[1], ref longs[1], ref ulongs[1], ref sbytes[1], ref bytes[1],
            ints.AsSpan(), uints.AsSpan(), shorts.AsSpan(), ushorts.AsSpan(),
            longs.AsSpan(), ulongs.AsSpan(), sbytes.AsSpan(), bytes.AsSpan(),
            ints.AsSpan()[1..], uints.AsSpan()[1..], shorts.AsSpan()[1..], ushorts.AsSpan()[1..],
            longs.AsSpan()[1..], ulongs.AsSpan()[1..], sbytes.AsSpan()[1..], bytes.AsSpan()[1..])).Return(72);

        Assert.AreEqual(72, mock.Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            ref ints[0], ref uints[0], ref shorts[0], ref ushorts[0], ref longs[0], ref ulongs[0], ref sbytes[0], ref bytes[0],
            ref ints[1], ref uints[1], ref shorts[1], ref ushorts[1], ref longs[1], ref ulongs[1], ref sbytes[1], ref bytes[1],
            ints.AsSpan(), uints.AsSpan(), shorts.AsSpan(), ushorts.AsSpan(),
            longs.AsSpan(), ulongs.AsSpan(), sbytes.AsSpan(), bytes.AsSpan(),
            ints.AsSpan()[1..], uints.AsSpan()[1..], shorts.AsSpan()[1..], ushorts.AsSpan()[1..],
            longs.AsSpan()[1..], ulongs.AsSpan()[1..], sbytes.AsSpan()[1..], bytes.AsSpan()[1..]));

        // The last ref is different
        Assert.AreNotEqual(72, mock.Long(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
            ref ints[0], ref uints[0], ref shorts[0], ref ushorts[0], ref longs[0], ref ulongs[0], ref sbytes[0], ref bytes[0],
            ref ints[1], ref uints[1], ref shorts[1], ref ushorts[1], ref longs[1], ref ulongs[1], ref sbytes[1], ref bytes[0], // here
            ints.AsSpan(), uints.AsSpan(), shorts.AsSpan(), ushorts.AsSpan(),
            longs.AsSpan(), ulongs.AsSpan(), sbytes.AsSpan(), bytes.AsSpan(),
            ints.AsSpan()[1..], uints.AsSpan()[1..], shorts.AsSpan()[1..], ushorts.AsSpan()[1..],
            longs.AsSpan()[1..], ulongs.AsSpan()[1..], sbytes.AsSpan()[1..], bytes.AsSpan()[1..]));
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
        var mock = Mock.Create<InParamMock>();

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

    /// <summary>Verifies Instance UnmockedObject ThrowsOnMockCall.</summary>
    [TestMethod]
    public void Instance_UnmockedObject_ThrowsOnMockCall()
    {
        var instance = new ClassMock(string.Empty);

        Assert.Throws<MockException>(() => Mock.When(() => instance.Name).Return("Hello"));
        Assert.Throws<MockException>(() => Mock.Raise(() => instance.Event += null));
    }

    /// <summary>Verifies Instance UnmockedBecomesPartiallyMocked.</summary>
    [TestMethod]
    public void Instance_UnmockedBecomesPartiallyMocked()
    {
        var instance = new ClassMock(string.Empty);

        Mock.Instance(instance);
        Mock.When(() => instance.Name).Return("Partial");

        Assert.AreEqual("Partial", instance.Name);
        Assert.AreEqual("Roger", instance.LastName); // Unmocked
    }

    /// <summary>Verifies Instance PartialMockWithArgs CallsRealIfNoMatch.</summary>
    [TestMethod]
    public void Instance_PartialMockWithArgs_CallsRealIfNoMatch()
    {
        var instance = new ClassMock(string.Empty);

        Mock.Instance(instance);
        Mock.When(() => instance.ReturnDouble(35)).Return(1);

        Assert.AreEqual(1, instance.ReturnDouble(35));
        Assert.AreEqual(64, instance.ReturnDouble(32)); // Original logic
    }

    /// <summary>Verifies Instance SameInstanceTwice ThrowsException.</summary>
    [TestMethod]
    public void Instance_SameInstanceTwice_ThrowsException()
    {
        var instance = new ClassMock(string.Empty);

        Mock.Instance(instance);

        Assert.Throws<MockException>(() => Mock.Instance(instance));
    }

    /// <summary>Verifies Instance MockObject ThrowsException.</summary>
    [TestMethod]
    public void Instance_MockObject_ThrowsException()
    {
        var mock = Mock.Create<ClassMock>();

        Assert.Throws<MockException>(() => Mock.Instance(mock));
    }
}
