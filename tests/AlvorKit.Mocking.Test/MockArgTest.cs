namespace AlvorKit.Mocking.Test;

public abstract class MockArgTest<T> where T : class, IMockTarget
{
    /// <summary>Verifies Arg MethodAnyAllArgs AlwaysMatches.</summary>
    [TestMethod]
    public void Arg_MethodAnyAllArgs_AlwaysMatches()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.ComputeSum(Arg.Any<int>(), Arg.Any<int>())).Return(64);

        Assert.AreEqual(64, mock.ComputeSum(34, 54));
        Assert.AreEqual(64, mock.ComputeSum(0, -1));
        Assert.AreEqual(64, mock.ComputeSum(12, -124));
    }

    /// <summary>Verifies Arg MethodAnySomeArgs MatchesCorrectArg.</summary>
    [TestMethod]
    public void Arg_MethodAnySomeArgs_MatchesCorrectArg()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.ComputeSum(0, Arg.Any<int>())).Return(64);

        Assert.AreEqual(0, mock.ComputeSum(34, 54));
        Assert.AreEqual(64, mock.ComputeSum(0, -1));
        Assert.AreEqual(0, mock.ComputeSum(12, -124));
    }

    /// <summary>Verifies Arg MethodMatch MatchesCorrectly.</summary>
    [TestMethod]
    public void Arg_MethodMatch_MatchesCorrectly()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.ComputeSum(Arg.Match<int>((x) => x < 14), Arg.Any<int>())).Return(64);

        Assert.AreEqual(0, mock.ComputeSum(34, 54));
        Assert.AreEqual(64, mock.ComputeSum(0, -1));
        Assert.AreEqual(64, mock.ComputeSum(12, -124));
    }

    /// <summary>Verifies Arg MethodMatchNothing MatchesNothing.</summary>
    [TestMethod]
    public void Arg_MethodMatchNothing_MatchesNothing()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.ComputeSum(Arg.Match<int>((x) => false), Arg.Any<int>())).Return(64);

        Assert.AreEqual(0, mock.ComputeSum(34, 54));
        Assert.AreEqual(0, mock.ComputeSum(0, -1));
        Assert.AreEqual(0, mock.ComputeSum(12, -124));
    }

    /// <summary>Verifies Arg MethodAnyRef MatchesCorrectly.</summary>
    [TestMethod]
    public void Arg_MethodAnyRef_MatchesCorrectly()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.Write(ref Arg<int>.Any())).Return([64]);

        int res = 34;
        mock.Write(ref res);
        Assert.AreEqual(64, res);

        res = 235;
        mock.Write(ref res);
        Assert.AreEqual(64, res);

        res = -123;
        mock.Write(ref res);
        Assert.AreEqual(64, res);
    }

    /// <summary>Verifies Arg MethodMatchRef MatchesCorrectly.</summary>
    [TestMethod]
    public void Arg_MethodMatchRef_MatchesCorrectly()
    {
        var mock = Mock.Create<T>();

        Mock.When(() => mock.Write(ref Arg<int>.Match((x) => x < 0))).Return([64]);

        int res = 34;
        mock.Write(ref res);
        Assert.AreEqual(34, res); // did not change

        res = 235;
        mock.Write(ref res);
        Assert.AreEqual(235, res); // did not change

        res = -123;
        mock.Write(ref res);
        Assert.AreEqual(64, res);
    }

    /// <summary>Verifies Arg MethodAnyInconsistentLess Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyInconsistentLess_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock.ComputeSum(Arg.Any<int>(), 0);
                else if (call == 1)
                    mock.ComputeSum(0, 0);
                call++;
            });
        });
    }

    /// <summary>Verifies Arg MethodAnyInconsistentChangedPosition IsAllowed.</summary>
    [TestMethod]
    public void Arg_MethodAnyInconsistentChangedPosition_IsAllowed()
    {
        var mock = Mock.Create<T>();

        int call = 0;
        Mock.When(() =>
        {
            if (call == 0)
                mock.ComputeSum(Arg.Any<int>(), 0);
            else if (call == 1)
                mock.ComputeSum(0, Arg.Any<int>()); // we can't detect this
            call++;
        });
    }

    /// <summary>Verifies Arg MethodAnyInconsistentNotProvidedToFunction Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyInconsistentNotProvidedToFunction_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock.ComputeSum(Arg.Any<int>(), 0);
                else if (call == 1)
                {
                    Arg.Any<int>();
                    mock.ComputeSum(0, 0);
                }
                call++;
            });
        });
    }

    /// <summary>Verifies Arg MethodAnyInconsistentOnlyCalledFirst Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyInconsistentOnlyCalledFirst_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock.ComputeSum(Arg.Any<int>(), Arg.Any<int>());
                else if (call == 1)
                    mock.ComputeSum(0, 0);
                call++;
            });
        });
    }

    /// <summary>Verifies Arg MethodAnyInconsistentOnlyCalledAfter Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyInconsistentOnlyCalledAfter_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock.ComputeSum(Arg.Any<int>(), 0); // need to have at least one Any otherwise it's skipped
                else if (call == 1)
                    mock.ComputeSum(Arg.Any<int>(), Arg.Any<int>());
                call++;
            });
        });
    }

    /// <summary>Verifies Arg MethodAnyChangedMethod Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyChangedMethod_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock.ComputeSum(Arg.Any<int>(), 0);
                else if (call == 1)
                    mock.ComputeSumWithSpan(Arg.Any<int>(), 0, default);
                call++;
            });
        });
    }

    /// <summary>Verifies Arg MethodAnyChangedInstance Throws.</summary>
    [TestMethod]
    public void Arg_MethodAnyChangedInstance_Throws()
    {
        var mock1 = Mock.Create<T>();
        var mock2 = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
        {
            int call = 0;
            Mock.When(() =>
            {
                if (call == 0)
                    mock1.ComputeSum(Arg.Any<int>(), 0);
                else if (call == 1)
                    mock2.ComputeSum(Arg.Any<int>(), 0);
                call++;
            });
        });
    }
}

[TestClass]
public class IMockTargetArgTest : MockArgTest<IMockTarget>;

[TestClass]
public class BasicMockArgTest : MockArgTest<BasicMock>;

[TestClass]
public class GenericMockArgTest : MockArgTest<GenericMock<List<int>>>;

[TestClass]
public class AbstractMockArgTest : MockArgTest<AbstractMock>;

[TestClass]
public class PartialMockArgTest : MockArgTest<PartialMock>;

[TestClass]
public class VirtualMockArgTest : MockArgTest<VirtualMock>;

[TestClass]
public class DerivedMockArgTest : MockArgTest<DerivedMock>;

[TestClass]
public class SealedMockArgTest : MockArgTest<SealedMock>;
