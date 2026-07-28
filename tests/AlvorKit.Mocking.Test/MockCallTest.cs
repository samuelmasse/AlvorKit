namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockCallTest
{
    private static readonly MethodInfo Method =
        typeof(MockCallTest).GetMethod(
            nameof(MixedTarget),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>Declared indices read mixed ordinary and ref inputs independently of carrier grouping.</summary>
    [TestMethod]
    public void Argument_MixedCarrierOrder_UsesDeclaredParameterIndices()
    {
        var (call, _) = CreateCall();

        Assert.AreEqual(11, call.Argument<int>(0));
        Assert.AreEqual(33L, call.Argument<long>(1));
        Assert.AreEqual("two", call.Argument<string>(2));
    }

    /// <summary>Reference writes use declared ref and out indices to mutate the invocation-owned carrier.</summary>
    [TestMethod]
    public void SetReference_RefAndOutParameters_MutatesCarrier()
    {
        var (call, carrier) = CreateCall();

        call.SetReference(1, 44L);
        call.SetReference(3, 55);

        Assert.AreEqual(44L, carrier[1]);
        Assert.AreEqual(55, carrier[3]);
        Assert.AreEqual(11, carrier[0]);
        Assert.AreEqual("two", carrier[2]);
    }

    /// <summary>Reference writes remain isolated to the active call's argument carrier.</summary>
    [TestMethod]
    public void SetReference_SeparateCalls_MutatesOnlyActiveCarrier()
    {
        var (firstCall, firstCarrier) = CreateCall();
        var (secondCall, secondCarrier) = CreateCall();

        firstCall.SetReference(1, 44L);
        secondCall.SetReference(1, 55L);

        Assert.AreEqual(44L, firstCarrier[1]);
        Assert.AreEqual(55L, secondCarrier[1]);
    }

    /// <summary>Argument reads and reference writes reject indices outside the declared signature.</summary>
    [TestMethod]
    public void Access_InvalidIndices_Throws()
    {
        var (call, _) = CreateCall();

        Assert.Throws<MockException>(() => call.Argument<int>(-1));
        Assert.Throws<MockException>(() => call.Argument<int>(6));
        Assert.Throws<MockException>(() => call.SetReference(6, 1));
    }

    /// <summary>Argument reads and reference writes reject mismatched generic value types.</summary>
    [TestMethod]
    public void Access_MismatchedGenericType_Throws()
    {
        var (call, _) = CreateCall();

        Assert.Throws<MockException>(() => call.Argument<long>(0));
        Assert.Throws<MockException>(() => call.SetReference(1, 44));
    }

    /// <summary>Reference writeback rejects an ordinary by-value parameter.</summary>
    [TestMethod]
    public void SetReference_NonReferenceParameter_Throws()
    {
        var (call, _) = CreateCall();

        Assert.Throws<MockException>(() => call.SetReference(0, 44));
    }

    /// <summary>Reading an out parameter is rejected because it has no entry value.</summary>
    [TestMethod]
    public void Argument_OutParameter_Throws()
    {
        var (call, _) = CreateCall();

        var error = Assert.Throws<MockException>(
            () => call.Argument<int>(3));

        StringAssert.Contains(error.Message, "has no entry value");
    }

    /// <summary>Ordinary call contexts reject reading a byref-like parameter.</summary>
    [TestMethod]
    public void Argument_ByRefLikeParameter_ThrowsTargetedError()
    {
        var (call, _) = CreateCall();

        var error = Assert.Throws<MockException>(
            () => call.Argument<object>(4));

        StringAssert.Contains(error.Message, "byref-like");
    }

    /// <summary>Ordinary call contexts reject writing a byref-like ref parameter.</summary>
    [TestMethod]
    public void SetReference_ByRefLikeParameter_ThrowsTargetedError()
    {
        var (call, _) = CreateCall();

        var error = Assert.Throws<MockException>(
            () => call.SetReference(5, new object()));

        StringAssert.Contains(error.Message, "byref-like");
    }

    private static (MockCall Call, object?[] Carrier) CreateCall()
    {
        object?[] carrier = [11, 33L, "two", null, null, null];
        var mocked = new Mocked(
            MockFallbackBehavior.Strict,
            new TypeCache(typeof(MockCallTest)));

        return (new(new object(), mocked, Method, carrier), carrier);
    }

    private static void MixedTarget(
        int first,
        ref long second,
        string third,
        out int fourth,
        ReadOnlySpan<byte> fifth,
        ref Span<byte> sixth)
    {
        _ = first;
        _ = second;
        _ = third;
        _ = fifth.Length;
        _ = sixth.Length;
        fourth = default;
    }
}
