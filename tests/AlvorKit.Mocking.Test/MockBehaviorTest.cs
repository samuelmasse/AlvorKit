namespace AlvorKit;

[TestClass]
public sealed class MockBehaviorTest
{
    /// <summary>Verifies a configured value call throws the exact exception instance.</summary>
    [TestMethod]
    public void Throw_ValueCall_ThrowsConfiguredInstance()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("configured");
        Mock.When(mock.GetValue).Throw(expected);

        var actual = Assert.Throws<InvalidOperationException>(
            () => mock.GetValue());

        Assert.AreSame(expected, actual);
    }

    /// <summary>Verifies a configured void call throws the exact exception instance.</summary>
    [TestMethod]
    public void Throw_VoidCall_ThrowsConfiguredInstance()
    {
        var mock = Mock.Create<IMockTarget>();
        var expected = new InvalidOperationException("configured");
        var value = 12;
        Mock.When(() => mock.Write(ref value)).Throw(expected);

        var actual = Assert.Throws<InvalidOperationException>(
            () => mock.Write(ref value));

        Assert.AreSame(expected, actual);
        Assert.AreEqual(12, value);
    }

    /// <summary>Verifies a newer matching return supersedes an older configured exception.</summary>
    [TestMethod]
    public void Throw_OlderSetup_IsSupersededByNewerReturn()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).Throw(
            new InvalidOperationException("superseded"));
        Mock.When(mock.GetValue).Return(42);

        Assert.AreEqual(42, mock.GetValue());
    }

    /// <summary>Verifies a null configured exception is rejected during setup.</summary>
    [TestMethod]
    public void Throw_NullException_Throws()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<ArgumentNullException>(
            () => Mock.When(mock.GetValue).Throw(null!));
    }

    /// <summary>Verifies return sequences advance in order and repeat their final value.</summary>
    [TestMethod]
    public void ReturnSequence_CallsAdvanceThenRepeatFinal()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).ReturnSequence(10, 20, 30);

        Assert.AreEqual(10, mock.GetValue());
        Assert.AreEqual(20, mock.GetValue());
        Assert.AreEqual(30, mock.GetValue());
        Assert.AreEqual(30, mock.GetValue());
    }

    /// <summary>Verifies a one-value return sequence repeats its only configured value.</summary>
    [TestMethod]
    public void ReturnSequence_OneValue_RepeatsValue()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).ReturnSequence(10);

        Assert.AreEqual(10, mock.GetValue());
        Assert.AreEqual(10, mock.GetValue());
        Assert.AreEqual(10, mock.GetValue());
    }

    /// <summary>Verifies return-sequence setup copies the caller's source array.</summary>
    [TestMethod]
    public void ReturnSequence_SourceArrayMutated_KeepsConfiguredValues()
    {
        var mock = Mock.Create<IMockTarget>();
        int[] values = [10, 20, 30];
        Mock.When(mock.GetValue).ReturnSequence(values);
        values[0] = 100;
        values[1] = 200;
        values[2] = 300;

        Assert.AreEqual(10, mock.GetValue());
        Assert.AreEqual(20, mock.GetValue());
        Assert.AreEqual(30, mock.GetValue());
    }

    /// <summary>Verifies a newer matching return leaves an older sequence unreachable.</summary>
    [TestMethod]
    public void ReturnSequence_OlderSetup_IsSupersededByNewerReturn()
    {
        var mock = Mock.Create<IMockTarget>();
        Mock.When(mock.GetValue).ReturnSequence(10, 20);
        Mock.When(mock.GetValue).Return(99);

        Assert.AreEqual(99, mock.GetValue());
        Assert.AreEqual(99, mock.GetValue());
        Assert.AreEqual(99, mock.GetValue());
    }

    /// <summary>Verifies a nonmatching loose call does not consume a configured sequence.</summary>
    [TestMethod]
    public void ReturnSequence_NonmatchingLooseCall_DoesNotConsumeValue()
    {
        var mock = Mock.CreateLoose<IMockTarget>();
        Mock.When(() => mock.ComputeSum(1, 2))
            .ReturnSequence(10, 20);

        Assert.AreEqual(0, mock.ComputeSum(2, 1));
        Assert.AreEqual(10, mock.ComputeSum(1, 2));
        Assert.AreEqual(20, mock.ComputeSum(1, 2));
    }

    /// <summary>Verifies return sequences reject an empty configured value set.</summary>
    [TestMethod]
    public void ReturnSequence_Empty_Throws()
    {
        var mock = Mock.Create<IMockTarget>();

        Assert.Throws<ArgumentException>(
            () => Mock.When(mock.GetValue).ReturnSequence());
    }

    /// <summary>Verifies concurrent sequence calls claim every non-terminal value exactly once.</summary>
    [TestMethod]
    public void ReturnSequence_ConcurrentCalls_ClaimExactMultiset()
    {
        const int uniqueValueCount = 128;
        var mock = Mock.Create<IMockTarget>();
        int[] expected = [.. Enumerable.Range(0, uniqueValueCount)];
        Mock.When(mock.GetValue).ReturnSequence(expected);
        var values = new int[uniqueValueCount];

        Parallel.For(
            0,
            uniqueValueCount,
            index => values[index] = mock.GetValue());

        CollectionAssert.AreEquivalent(expected, values);
    }
}
