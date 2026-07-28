namespace AlvorKit.Mocking.Test;

public abstract class MockMethodTest<T> where T : class, IMockTarget
{
    /// <summary>Verifies Method GetValue ReturnsMocked.</summary>
    [TestMethod]
    public void Method_GetValue_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(mock.GetValue).Return(456);

        Assert.AreEqual(456, mock.GetValue());
    }

    /// <summary>Verifies Method GetPtrValue ReturnsMocked.</summary>
    [TestMethod]
    public unsafe void Method_GetPtrValue_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => (nint)mock.GetPtrValue()).Return(456);

        Assert.AreEqual(456, (nint)mock.GetPtrValue());
    }

    /// <summary>Verifies Method GetRef ReturnsMocked.</summary>
    [TestMethod]
    public void Method_GetRef_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.GetRef()).Return(789);

        ref var result = ref mock.GetRef();

        Assert.AreEqual(789, result);
    }

    /// <summary>Verifies Method GetRefPtr ReturnsMocked.</summary>
    [TestMethod]
    public unsafe void Method_GetRefPtr_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => (nint)mock.GetRefPtr()).Return(789);

        ref var result = ref mock.GetRefPtr();

        Assert.AreEqual(789, (nint)result);
    }

    /// <summary>Verifies Method Read OutParameter ReturnsMocked.</summary>
    [TestMethod]
    public void Method_Read_OutParameter_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(0, 100));

        mock.Read(out var value);

        Assert.AreEqual(100, value);
    }

    /// <summary>Verifies Method ReadPtr OutParameter ReturnsMocked.</summary>
    [TestMethod]
    public unsafe void Method_ReadPtr_OutParameter_ReturnsMocked()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.ReadPtr(out _))
            .Do(call => call.SetReference(0, (nint)100));

        mock.ReadPtr(out var value);

        Assert.AreEqual(100, (nint)value);
    }

    /// <summary>Verifies Method Read OutParameterWrongCount Throws.</summary>
    [TestMethod]
    public void Method_Read_OutParameterWrongCount_Throws()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.Read(out _))
            .Do(call => call.SetReference(1, 1));
        Assert.Throws<MockException>(() => mock.Read(out _));
    }

    /// <summary>Verifies Method Read OutParameterMatchesAnyValue.</summary>
    [TestMethod]
    public void Method_Read_OutParameterMatchesAnyValue()
    {
        var mock = Mock.Create<T>();
        int match = 10;
        Mock.When(() => mock.Read(out match))
            .Do(call => call.SetReference(0, 22));

        mock.Read(out var result);

        Assert.AreEqual(22, result);
    }

    /// <summary>Verifies Method Write RefParameter UpdatesValue.</summary>
    [TestMethod]
    public void Method_Write_RefParameter_UpdatesValue()
    {
        var mock = Mock.Create<T>();
        int input = 88;
        Mock.When(() => mock.Write(ref input))
            .Do(call => call.SetReference(0, 44));

        mock.Write(ref input);

        Assert.AreEqual(44, input);
    }

    /// <summary>Verifies Method WritePtr RefParameter UpdatesValue.</summary>
    [TestMethod]
    public unsafe void Method_WritePtr_RefParameter_UpdatesValue()
    {
        var mock = Mock.Create<T>();
        int* ptr = (int*)88;
        Mock.When(() => mock.WritePtr(ref ptr))
            .Do(call => call.SetReference(0, (nint)44));

        mock.WritePtr(ref ptr);

        Assert.AreEqual(44, (nint)ptr);
    }

    /// <summary>Verifies Method Write RefParameterWrongCount Throws.</summary>
    [TestMethod]
    public void Method_Write_RefParameterWrongCount_Throws()
    {
        var mock = Mock.Create<T>();
        int val = 0;

        Mock.When(() => mock.Write(ref val))
            .Do(call => call.SetReference(1, 1));
        Assert.Throws<MockException>(() => mock.Write(ref val));
    }

    /// <summary>Verifies Method Write RefParameterNoMatch DoesNotUpdate.</summary>
    [TestMethod]
    public void Method_Write_RefParameterNoMatch_DoesNotUpdate()
    {
        var mock = Mock.CreateLoose<T>();
        int val = 42;
        Mock.When(() => mock.Write(ref val))
            .Do(call => call.SetReference(0, 35));

        int other = 43;
        mock.Write(ref other);

        Assert.AreEqual(43, other);
    }

    /// <summary>Verifies Method WithArgs ReturnsSpecified.</summary>
    [TestMethod]
    public void Method_WithArgs_ReturnsSpecified()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.ComputeSum(1, 2)).Return(3);

        Assert.AreEqual(3, mock.ComputeSum(1, 2));
    }

    /// <summary>Verifies Method WithArgsMultipleTimes UsesLatest.</summary>
    [TestMethod]
    public void Method_WithArgsMultipleTimes_UsesLatest()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.ComputeSum(1, 2)).Return(3);
        Mock.When(() => mock.ComputeSum(1, 2)).Return(99);

        Assert.AreEqual(99, mock.ComputeSum(1, 2));
    }

    /// <summary>Verifies Method WithArgsMultipleTimesWithSpan UsesLatest.</summary>
    [TestMethod]
    public void Method_WithArgsMultipleTimesWithSpan_UsesLatest()
    {
        int[] ints3 = [3];

        var mock = Mock.Create<T>();
        Mock.When(
                () => mock.ComputeSumWithSpan(
                    1,
                    2,
                    Arg.Any<Span<int>>(2)))
            .Return(3);
        Mock.When(
                () => mock.ComputeSumWithSpan(
                    1,
                    2,
                    Arg.Any<Span<int>>(2)))
            .Return(99);

        Assert.AreEqual(99, mock.ComputeSumWithSpan(1, 2, ints3));
    }

    /// <summary>Verifies Method WithArgsMultipleTimesWithSpanOut UsesLatest.</summary>
    [TestMethod]
    public void Method_WithArgsMultipleTimesWithSpanOut_UsesLatest()
    {
        var mock = Mock.Create<T>();
        Mock.When(() => mock.ComputeSumWithSpanOut(1, 2, out _)).Return(3);
        Mock.When(() => mock.ComputeSumWithSpanOut(1, 2, out _)).Return(99);

        Assert.AreEqual(99, mock.ComputeSumWithSpanOut(1, 2, out _)); // spans get ignored
    }

    /// <summary>Verifies Method WithSpanRef CannotBeMocked.</summary>
    [TestMethod]
    public void Method_WithSpanRef_CannotBeMocked()
    {
        var mock = Mock.CreateLoose<T>();
        Span<int> ints = [234];
        mock.ComputeSumWithSpanRef(1, 2, ref ints);
    }

    /// <summary>Confirms supported ref-struct returns use strict fallback when no factory is configured.</summary>
    [TestMethod]
    public void Method_SpanReturn_StrictFallbackThrows()
    {
        var mock = Mock.Create<T>();
        Assert.Throws<MockException>(
            () => mock.ComputeSumWithSpanReturn(1, 2));
    }

    /// <summary>Direct overrides reject ref-struct aliases while concrete interception preserves the original throw.</summary>
    [TestMethod]
    public void Method_WithSpanRefReturn_CannotBeMocked()
    {
        var mock = Mock.Create<T>();
        MethodInfo method = typeof(T).GetMethod(
            nameof(IMockTarget.ComputeSumWithSpanRefReturn))!;
        if (!typeof(T).IsSealed &&
            (method.IsAbstract || method.IsVirtual && !method.IsFinal))
        {
            Assert.Throws<MockException>(
                () => mock.ComputeSumWithSpanRefReturn(1, 2));
        }
        else
        {
            Assert.Throws<NotImplementedException>(
                () => mock.ComputeSumWithSpanRefReturn(1, 2));
        }
    }

    /// <summary>Verifies Method WithArgs NoMatch ReturnsDefault.</summary>
    [TestMethod]
    public void Method_WithArgs_NoMatch_ReturnsDefault()
    {
        var mock = Mock.CreateLoose<T>();
        Mock.When(() => mock.ComputeSum(2, 2)).Return(123);

        Assert.AreEqual(0, mock.ComputeSum(1, 1));
    }

}

[TestClass]
public class IMockTargetMethodTest : MockMethodTest<IMockTarget>;

[TestClass]
public class AbstractMockMethodTest : MockMethodTest<AbstractMock>;

[TestClass]
public class PartialMockMethodTest : MockMethodTest<PartialMock>;

[TestClass]
public class VirtualMockMethodTest : MockMethodTest<VirtualMock>;
