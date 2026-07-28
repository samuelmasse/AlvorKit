namespace AlvorKit.Mocking.Test;

[TestClass]
public class ArgTest
{
    /// <summary>Verifies Arg Any ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_Any_ReturnsEmpty()
    {
        Assert.AreEqual(0, Arg.Any<int>());
        Assert.IsNull(Arg.Any<string>());
        Assert.IsNull(Arg.Any<List<string>>());
    }

    /// <summary>Verifies Arg Match ReturnsEmpty.</summary>
    [TestMethod]
    public void Arg_Match_ReturnsEmpty()
    {
        Assert.AreEqual(0, Arg.Match<int>((x) => true));
        Assert.IsNull(Arg.Match<string>((x) => true));
        Assert.IsNull(Arg.Match<List<string>>((x) => true));
    }

    /// <summary>Indexed by-reference matchers reject use outside active capture.</summary>
    [TestMethod]
    public void Arg_IndexedRefMatchers_RequireActiveCapture()
    {
        Assert.Throws<MockException>(
            () => Arg.Any<int>(0));
        Assert.Throws<MockException>(
            () => Arg.Match<int>(
                0,
                static value => value != 0));
        Assert.Throws<MockException>(
            () => Arg.ReadOnlySpanEqual<int>(
                0,
                [1]));
        Assert.Throws<MockException>(
            () => Arg.AnyRef<int>(0));
        Assert.Throws<MockException>(
            () => Arg.Match<int>(
                0,
                (scoped in _) => true));
    }

    /// <summary>Active indexed by-value matchers return defaults without evaluating predicates.</summary>
    [TestMethod]
    public void Arg_IndexedValueMatchers_ReturnDefaults()
    {
        var predicateCalls = 0;
        Capture.Start(CaptureOperation.Setup);
        try
        {
            int any = Arg.Any<int>(0);
            ReadOnlySpan<int> matched =
                Arg.Match<ReadOnlySpan<int>>(
                    1,
                    _ =>
                    {
                        predicateCalls++;
                        return true;
                    });

            Assert.AreEqual(0, any);
            Assert.IsTrue(matched.IsEmpty);
            Assert.AreEqual(0, predicateCalls);
            Assert.AreEqual(2, Capture.FirstIndexedMatchers.Count);
        }
        finally
        {
            Capture.End();
        }
    }

    /// <summary>Active indexed by-reference matchers return only null-reference placeholders.</summary>
    [TestMethod]
    public void Arg_IndexedRefMatchers_ReturnNullReferences()
    {
        Capture.Start(CaptureOperation.Setup);
        try
        {
            ref int any = ref Arg.AnyRef<int>(0);
            ref string matched = ref Arg.Match<string>(
                1,
                (scoped in value) => value.Length != 0);

            Assert.IsTrue(
                System.Runtime.CompilerServices.Unsafe
                    .IsNullRef(ref any));
            Assert.IsTrue(
                System.Runtime.CompilerServices.Unsafe
                    .IsNullRef(ref matched));
            Assert.AreEqual(2, Capture.FirstIndexedMatchers.Count);
        }
        finally
        {
            Capture.End();
        }
    }
}
