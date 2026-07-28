namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockDispatchBoundaryTest
{
    /// <summary>More than sixteen ordinary arguments retain declared indices through configured dispatch.</summary>
    [TestMethod]
    public void Ordinary_MoreThanSixteen_PreservesDeclaredOrder()
    {
        var mock = Mock.Create<MockDispatchBoundaryTarget>();
        Mock.When(() => InvokeOrdinary(mock))
            .Answer(
                call =>
                {
                    for (int index = 0; index < 17; index++)
                        Assert.AreEqual(101 + index, call.Argument<int>(index));

                    return 701;
                });

        Assert.AreEqual(701, InvokeOrdinary(mock));
    }

    /// <summary>More than sixteen managed references preserve entry order and independent writeback.</summary>
    [TestMethod]
    public void References_MoreThanSixteen_PreservesDeclaredOrderAndWriteback()
    {
        var mock = Mock.Create<MockDispatchBoundaryTarget>();
        int[] setupReferences = Sequence(301);
        Mock.When(() => InvokeReferences(mock, setupReferences))
            .Do(
                call =>
                {
                    for (int index = 0; index < 17; index++)
                    {
                        Assert.AreEqual(
                            301 + index,
                            call.Argument<int>(index));
                        call.SetReference(index, 801 + index);
                    }
                });

        int[] references = Sequence(301);
        InvokeReferences(mock, references);

        CollectionAssert.AreEqual(Sequence(801), references);
    }

    /// <summary>More than sixteen ref-struct parameters match their declared positions without boxing.</summary>
    [TestMethod]
    public void RefStructs_MoreThanSixteen_PreservesDeclaredMatcherOrder()
    {
        var mock = Mock.Create<MockDispatchBoundaryTarget>();
        int[] storage = Sequence(401);
        int[] inputStorage = [601, 602];
        int[] referenceStorage = [701, 702, 703];

        Mock.When(
                () => ConfigureRefStructMatchers(
                    mock,
                    storage,
                    inputStorage))
            .Do(_ => { });

        var (ReferenceLength, ReferenceFirst, OutputLength) = InvokeRefStructs(
            mock,
            storage,
            inputStorage,
            referenceStorage);

        Assert.AreEqual(3, ReferenceLength);
        Assert.AreEqual(701, ReferenceFirst);
        Assert.AreEqual(0, OutputLength);
    }

    /// <summary>
    /// A mixed signature beyond every retired category width preserves declared
    /// value, input, reference, output, and ref-struct positions.
    /// </summary>
    [TestMethod]
    public void Mixed_BeyondGroupedWidths_PreservesDeclaredOrderAndWriteback()
    {
        var mock = Mock.Create<MockDispatchBoundaryTarget>();
        int[] spans = Sequence(401);
        int[] setupReferences = Sequence(301);
        Mock.When(
                () => ConfigureMixedMatchers(
                    mock,
                    setupReferences,
                    spans,
                    201))
            .Do(AssertMixedAndWriteReferences);

        int[] references = Sequence(301);
        InvokeMixed(mock, references, spans, 201, out int output);

        CollectionAssert.AreEqual(Sequence(901), references);
        Assert.AreEqual(1201, output);
    }

    private static void AssertMixedAndWriteReferences(MockCall call)
    {
        Assert.AreEqual(201, call.Argument<int>(3));

        for (int index = 0; index < 17; index++)
        {
            int valueIndex = index == 0 ? 0 : (3 * index) + 1;
            int referenceIndex = valueIndex + 2;
            Assert.AreEqual(
                101 + index,
                call.Argument<int>(valueIndex));
            Assert.AreEqual(
                301 + index,
                call.Argument<int>(referenceIndex));
            call.SetReference(referenceIndex, 901 + index);
        }

        call.SetReference(52, 1201);
    }

    private static int[] Sequence(int start)
    {
        var result = new int[17];
        for (int index = 0; index < result.Length; index++)
            result[index] = start + index;

        return result;
    }

    private static int InvokeOrdinary(MockDispatchBoundaryTarget target) =>
        target.Ordinary(
            101, 102, 103, 104, 105, 106,
            107, 108, 109, 110, 111, 112,
            113, 114, 115, 116, 117);

    private static void InvokeReferences(
        MockDispatchBoundaryTarget target,
        int[] references) =>
        target.References(
            ref references[0], ref references[1], ref references[2],
            ref references[3], ref references[4], ref references[5],
            ref references[6], ref references[7], ref references[8],
            ref references[9], ref references[10], ref references[11],
            ref references[12], ref references[13], ref references[14],
            ref references[15], ref references[16]);

    private static (
        int ReferenceLength,
        int ReferenceFirst,
        int OutputLength) InvokeRefStructs(
        MockDispatchBoundaryTarget target,
        int[] storage,
        int[] inputStorage,
        int[] referenceStorage)
    {
        ReadOnlySpan<int> input = inputStorage;
        Span<int> reference = referenceStorage;
        target.RefStructs(
            storage.AsSpan(0, 1), storage.AsSpan(1, 1),
            storage.AsSpan(2, 1), storage.AsSpan(3, 1),
            storage.AsSpan(4, 1), storage.AsSpan(5, 1),
            storage.AsSpan(6, 1), storage.AsSpan(7, 1),
            storage.AsSpan(8, 1), storage.AsSpan(9, 1),
            storage.AsSpan(10, 1), storage.AsSpan(11, 1),
            storage.AsSpan(12, 1), storage.AsSpan(13, 1),
            storage.AsSpan(14, 1), storage.AsSpan(15, 1),
            storage.AsSpan(16, 1),
            in input,
            ref reference,
            out Span<int> output);
        return (
            reference.Length,
            reference[0],
            output.Length);
    }

    private static void ConfigureRefStructMatchers(
        MockDispatchBoundaryTarget target,
        int[] storage,
        int[] inputStorage) =>
        target.RefStructs(
            Arg.SpanEqual<int>(0, storage.AsSpan(0, 1)),
            Arg.SpanEqual<int>(1, storage.AsSpan(1, 1)),
            Arg.SpanEqual<int>(2, storage.AsSpan(2, 1)),
            Arg.SpanEqual<int>(3, storage.AsSpan(3, 1)),
            Arg.SpanEqual<int>(4, storage.AsSpan(4, 1)),
            Arg.SpanEqual<int>(5, storage.AsSpan(5, 1)),
            Arg.SpanEqual<int>(6, storage.AsSpan(6, 1)),
            Arg.SpanEqual<int>(7, storage.AsSpan(7, 1)),
            Arg.SpanEqual<int>(8, storage.AsSpan(8, 1)),
            Arg.SpanEqual<int>(9, storage.AsSpan(9, 1)),
            Arg.SpanEqual<int>(10, storage.AsSpan(10, 1)),
            Arg.SpanEqual<int>(11, storage.AsSpan(11, 1)),
            Arg.SpanEqual<int>(12, storage.AsSpan(12, 1)),
            Arg.SpanEqual<int>(13, storage.AsSpan(13, 1)),
            Arg.SpanEqual<int>(14, storage.AsSpan(14, 1)),
            Arg.SpanEqual<int>(15, storage.AsSpan(15, 1)),
            Arg.SpanEqual<int>(16, storage.AsSpan(16, 1)),
            Arg.ReadOnlySpanEqual<int>(17, inputStorage),
            ref Arg.Match<Span<int>>(
                18,
                (
                    scoped in values) =>
                    values.Length == 3 &&
                    values[0] == 701),
            out _);

    private static void InvokeMixed(
        MockDispatchBoundaryTarget target,
        int[] references,
        int[] spans,
        int input,
        out int output) =>
        target.Mixed(
            101, spans.AsSpan(0, 1), ref references[0], in input,
            102, spans.AsSpan(1, 1), ref references[1],
            103, spans.AsSpan(2, 1), ref references[2],
            104, spans.AsSpan(3, 1), ref references[3],
            105, spans.AsSpan(4, 1), ref references[4],
            106, spans.AsSpan(5, 1), ref references[5],
            107, spans.AsSpan(6, 1), ref references[6],
            108, spans.AsSpan(7, 1), ref references[7],
            109, spans.AsSpan(8, 1), ref references[8],
            110, spans.AsSpan(9, 1), ref references[9],
            111, spans.AsSpan(10, 1), ref references[10],
            112, spans.AsSpan(11, 1), ref references[11],
            113, spans.AsSpan(12, 1), ref references[12],
            114, spans.AsSpan(13, 1), ref references[13],
            115, spans.AsSpan(14, 1), ref references[14],
            116, spans.AsSpan(15, 1), ref references[15],
            117, spans.AsSpan(16, 1), ref references[16],
            out output);

    private static void ConfigureMixedMatchers(
        MockDispatchBoundaryTarget target,
        int[] references,
        int[] spans,
        int input) =>
        target.Mixed(
            101, Arg.SpanEqual<int>(1, spans.AsSpan(0, 1)), ref references[0], in input,
            102, Arg.SpanEqual<int>(5, spans.AsSpan(1, 1)), ref references[1],
            103, Arg.SpanEqual<int>(8, spans.AsSpan(2, 1)), ref references[2],
            104, Arg.SpanEqual<int>(11, spans.AsSpan(3, 1)), ref references[3],
            105, Arg.SpanEqual<int>(14, spans.AsSpan(4, 1)), ref references[4],
            106, Arg.SpanEqual<int>(17, spans.AsSpan(5, 1)), ref references[5],
            107, Arg.SpanEqual<int>(20, spans.AsSpan(6, 1)), ref references[6],
            108, Arg.SpanEqual<int>(23, spans.AsSpan(7, 1)), ref references[7],
            109, Arg.SpanEqual<int>(26, spans.AsSpan(8, 1)), ref references[8],
            110, Arg.SpanEqual<int>(29, spans.AsSpan(9, 1)), ref references[9],
            111, Arg.SpanEqual<int>(32, spans.AsSpan(10, 1)), ref references[10],
            112, Arg.SpanEqual<int>(35, spans.AsSpan(11, 1)), ref references[11],
            113, Arg.SpanEqual<int>(38, spans.AsSpan(12, 1)), ref references[12],
            114, Arg.SpanEqual<int>(41, spans.AsSpan(13, 1)), ref references[13],
            115, Arg.SpanEqual<int>(44, spans.AsSpan(14, 1)), ref references[14],
            116, Arg.SpanEqual<int>(47, spans.AsSpan(15, 1)), ref references[15],
            117, Arg.SpanEqual<int>(50, spans.AsSpan(16, 1)), ref references[16],
            out _);
}
