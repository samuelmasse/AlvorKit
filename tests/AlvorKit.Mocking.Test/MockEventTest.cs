namespace AlvorKit.Mocking.Test;

public abstract class MockEventTest<T> where T : class, IMockTarget
{
    /// <summary>Verifies Event OnEvent RaisesSuccessfully.</summary>
    [TestMethod]
    public void Event_OnEvent_RaisesSuccessfully()
    {
        var mock = Mock.Create<T>();
        bool raised = false;

        mock.OnEvent += (s, e) => raised = true;

        Mock.Raise(() => mock.OnEvent += null, mock, EventArgs.Empty);

        Assert.IsTrue(raised);
    }

    /// <summary>Verifies Event OnEventWithMinus RaisesSuccessfully.</summary>
    [TestMethod]
    public void Event_OnEventWithMinus_RaisesSuccessfully()
    {
        var mock = Mock.Create<T>();
        bool raised = false;

        mock.OnEvent += (s, e) => raised = true;

        Mock.Raise(() => mock.OnEvent -= null, mock, EventArgs.Empty);

        Assert.IsTrue(raised);
    }

    /// <summary>Verifies Event OnEventDoubleAddThenMinus RunsOnce.</summary>
    [TestMethod]
    public void Event_OnEventDoubleAddThenMinus_RunsOnce()
    {
        var mock = Mock.Create<T>();
        int raised = 0;

        mock.OnActionEvent += Event;
        mock.OnActionEvent += Event;
        mock.OnActionEvent -= Event;

        Mock.Raise(() => mock.OnActionEvent += null, 3);

        void Event(int x) => raised++;

        Assert.AreEqual(1, raised);
    }

    /// <summary>Verifies Event RaiseRealFunction Throws.</summary>
    [TestMethod]
    public void Event_RaiseRealFunction_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
            Mock.Raise(() => mock.OnEvent += (s, e) => { }, mock, EventArgs.Empty));
    }

    /// <summary>Verifies Event RaiseEmpty Throws.</summary>
    [TestMethod]
    public void Event_RaiseEmpty_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
            Mock.Raise(() => { }, mock, EventArgs.Empty));
    }

    /// <summary>Verifies Event RaiseMethodCall Throws.</summary>
    [TestMethod]
    public void Event_RaiseMethodCall_Throws()
    {
        var mock = Mock.Create<T>();

        Assert.Throws<MockException>(() =>
            Mock.Raise(() => mock.ComputeSum(1, 2), mock, EventArgs.Empty));
    }

    /// <summary>Verifies ActionEvent RaisesSuccessfully.</summary>
    [TestMethod]
    public void ActionEvent_RaisesSuccessfully()
    {
        var mock = Mock.Create<T>();
        bool raised = false;

        mock.OnActionEvent += (e) => raised = true;

        Mock.Raise(() => mock.OnActionEvent += null, 123);

        Assert.IsTrue(raised);
    }

    /// <summary>Verifies Events BehaveSameInMockAndReal.</summary>
    [TestMethod]
    public void Events_BehaveSameInMockAndReal()
    {
        var mock = Mock.Create<T>();
        var real = new BasicMock();

        int realCount = 0;
        int mockCount = 0;

        void RealHandler(int _) => realCount++;
        void MockHandler(int _) => mockCount++;

        void AssertDelta(int expected)
        {
            int beforeReal = realCount;
            int beforeMock = mockCount;

            real.RaiseEvent();
            Mock.Raise(() => mock.OnActionEvent += null, 123);

            Assert.AreEqual(beforeReal + expected, realCount);
            Assert.AreEqual(beforeMock + expected, mockCount);
        }

        real.OnActionEvent += RealHandler;
        mock.OnActionEvent += MockHandler;

        AssertDelta(1);

        real.OnActionEvent -= RealHandler;
        mock.OnActionEvent -= MockHandler;

        AssertDelta(0);

        real.OnActionEvent += RealHandler;
        real.OnActionEvent += RealHandler;
        mock.OnActionEvent += MockHandler;
        mock.OnActionEvent += MockHandler;

        AssertDelta(2);
    }
}

[TestClass]
public class IMockTargetEventTest : MockEventTest<IMockTarget>;

[TestClass]
public class BasicMockEventTest : MockEventTest<BasicMock>;

[TestClass]
public class GenericMockEventTest : MockEventTest<GenericMock<List<int>>>;

[TestClass]
public class AbstractMockEventTest : MockEventTest<AbstractMock>;

[TestClass]
public class PartialMockEventTest : MockEventTest<PartialMock>;

[TestClass]
public class VirtualMockEventTest : MockEventTest<VirtualMock>;

[TestClass]
public class DerivedMockEventTest : MockEventTest<DerivedMock>;

[TestClass]
public class SealedMockEventTest : MockEventTest<SealedMock>;
