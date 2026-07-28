namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockOrdinaryMatcherTest
{
    /// <summary>Null string and reference neighbors do not hide matcher positions.</summary>
    [TestMethod]
    public void ReferenceMatchers_MapRepeatedDefaultNeighbors()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();
        Mock.When(
                () => target.References(
                    null,
                    Arg.Any<string?>(),
                    null))
            .Return(11);
        Mock.When(
                () => target.Objects(
                    null,
                    Arg.Any<object?>(),
                    null))
            .Return(13);

        Assert.AreEqual(
            11,
            target.References(null, "matched", null));
        Assert.AreEqual(
            13,
            target.Objects(null, new object(), null));
    }

    /// <summary>A structure containing a reference maps safely without user equality.</summary>
    [TestMethod]
    public void StructContainingReference_MapsMatcherPosition()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();
        Mock.When(
                () => target.Structures(
                    default,
                    Arg.Any<ReferenceBearingValue>(),
                    default))
            .Return(17);

        Assert.AreEqual(
            17,
            target.Structures(
                default,
                new("matched", 19),
                default));
    }

    /// <summary>Exact default values surrounding repeated same-type matchers remain exact.</summary>
    [TestMethod]
    public void ValueMatchers_MapRepeatedDefaultNeighbors()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();
        Mock.When(
                () => target.Values(
                    0,
                    Arg.Any<int>(),
                    0,
                    Arg.Any<int>()))
            .Return(23);

        Assert.AreEqual(23, target.Values(0, 5, 0, 7));
        Assert.AreEqual(0, target.Values(1, 5, 0, 7));
    }

    /// <summary>Nullable and enum matchers use valid alternate values during capture.</summary>
    [TestMethod]
    public void NullableAndEnumMatchers_MapCorrectly()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();
        Mock.When(
                () => target.NullableAndEnum(
                    null,
                    Arg.Any<int?>(),
                    OrdinaryMatcherKind.None,
                    Arg.Any<OrdinaryMatcherKind>()))
            .Return(29);

        Assert.AreEqual(
            29,
            target.NullableAndEnum(
                null,
                31,
                OrdinaryMatcherKind.None,
                OrdinaryMatcherKind.Second));
    }

    /// <summary>Capture comparison never invokes a user-defined equality or hash implementation.</summary>
    [TestMethod]
    public void UserEquality_IsNeverUsedToPlaceMatcher()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();

        Mock.When(
                () => target.Dangerous(
                    Arg.Any<DangerousEqualityValue>()))
            .Return(37);

        Assert.AreEqual(
            37,
            target.Dangerous(new DangerousEqualityValue(41)));
    }

    /// <summary>A compacting collection between capture passes does not affect positional mapping.</summary>
    [TestMethod]
    public void ForcedCollectionBetweenPasses_PreservesMapping()
    {
        var target = Mock.CreateLoose<IOrdinaryMatcherTarget>();
        var pass = 0;
        Mock.When(
                () =>
                {
                    pass++;
                    if (pass == 2)
                        ForceCollection();

                    return target.References(
                        null,
                        Arg.Any<string?>(),
                        null);
                })
            .Return(43);

        Assert.AreEqual(2, pass);
        Assert.AreEqual(
            43,
            target.References(null, "after GC", null));
    }

    private static void ForceCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

internal interface IOrdinaryMatcherTarget
{
    int References(string? first, string? second, string? third);

    int Objects(object? first, object? second, object? third);

    int Structures(
        ReferenceBearingValue first,
        ReferenceBearingValue second,
        ReferenceBearingValue third);

    int Values(int first, int second, int third, int fourth);

    int NullableAndEnum(
        int? first,
        int? second,
        OrdinaryMatcherKind third,
        OrdinaryMatcherKind fourth);

    int Dangerous(DangerousEqualityValue value);
}

internal readonly record struct ReferenceBearingValue(
    string? Text,
    int Number);

internal enum OrdinaryMatcherKind
{
    None,
    First,
    Second
}

internal readonly struct DangerousEqualityValue(int value)
{
    internal int Value { get; } = value;

    public override bool Equals(object? obj) =>
        throw new InvalidOperationException(
            "User equality must not run during matcher capture.");

    public override int GetHashCode() =>
        throw new InvalidOperationException(
            "User hash code must not run during matcher capture.");
}
