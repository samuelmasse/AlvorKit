MockDynamic.Enable();

Console.WriteLine("AlvorKit.Mocking walkthrough");

Console.WriteLine("1. Strict and loose mocks");

var strictRenderer = Mock.Create<IRenderer>();
var observedSprite = string.Empty;
var observedLayer = -1;
Mock.When(() => strictRenderer.Draw(
        "player",
        Arg.Match<int>(layer => layer >= 0)))
    .Answer(call =>
    {
        observedSprite = call.Argument<string>(0);
        observedLayer = call.Argument<int>(1);
        return true;
    });

Expect(
    strictRenderer.Draw("player", 3),
    "the configured strict call should use its answer");
Expect(
    observedSprite == "player" && observedLayer == 3,
    "the callback should receive declared arguments in order");
ExpectThrows<MockException>(
    () => strictRenderer.Draw("enemy", 3),
    "an unmatched strict call should fail");

Mock.Verify(() => strictRenderer.Draw("player", 3))
    .Once();
Mock.Verify(() => strictRenderer.Draw("enemy", 3))
    .Once();
Mock.VerifyNoOtherCalls(strictRenderer);

var looseRenderer = Mock.CreateLoose<IRenderer>();
Expect(
    !looseRenderer.Draw("unconfigured", 0),
    "an unmatched loose call should return the bool default");
Mock.Verify(() => looseRenderer.Draw("unconfigured", 0))
    .Once();
Mock.VerifyNoOtherCalls(looseRenderer);

Console.WriteLine("   strict match: player at layer 3");
Console.WriteLine("   loose fallback: false");

Console.WriteLine("2. Session checkpoints and cross-mock order");

var input = Mock.CreateLoose<IFrameInput>();
var frameRenderer = Mock.Create<IRenderer>();
var audio = Mock.CreateLoose<IAudioMixer>();
Mock.When(() => frameRenderer.Draw("player", 3))
    .Return(true);

using (var session = Mock.Session())
{
    MockCheckpoint beforeFrame = session.Checkpoint();

    input.Poll();
    Expect(
        frameRenderer.Draw("player", 3),
        "the ordered render step should use its setup");
    audio.Mix();

    MockCheckpoint throughFrame = session.Checkpoint();

    Mock.Verify(input.Poll)
        .Between(beforeFrame, throughFrame)
        .Once();
    session.VerifySequence(
        input.Poll,
        () => frameRenderer.Draw("player", 3),
        audio.Mix);
}

Mock.VerifyNoOtherCalls(input);
Mock.VerifyNoOtherCalls(frameRenderer);
Mock.VerifyNoOtherCalls(audio);

Console.WriteLine("   order: input -> render -> audio");

Console.WriteLine("3. Ref-safe callbacks and stable snapshots");

var analyzer = Mock.Create<ISampleAnalyzer>();
Mock.When(() => analyzer.Sum(
        Arg.Any<ReadOnlySpan<int>>(0)))
    .SnapshotArgument(
        0,
        (ReadOnlySpan<int> values) => values.ToArray())
    .Answer((ReadOnlySpan<int> values) => Sum(values));

int[] samples = [2, 3, 5, 7];
Expect(
    analyzer.Sum(samples) == 17,
    "the typed callback should consume the live span");
samples.AsSpan().Fill(-1);

Mock.Verify(() => analyzer.Sum(
        Arg.ReadOnlySpanEqual<int>(
            0,
            [2, 3, 5, 7])))
    .Once();
Mock.VerifyNoOtherCalls(analyzer);

Console.WriteLine("   stable snapshot: [2, 3, 5, 7]");

Console.WriteLine("4. Partial behavior");

var counter = new Counter();
Counter partialCounter = Mock.Partial(counter);

Expect(
    ReferenceEquals(counter, partialCounter),
    "a partial mock should preserve the supplied object");
Expect(
    partialCounter.Next() == 1 && counter.Current == 1,
    "an existing partial should preserve original behavior");
Expect(
    partialCounter.Add(3) == 4 && counter.Current == 4,
    "an unmatched partial call should execute the original");

Console.WriteLine("   same object; original value advanced to 4");

Console.WriteLine("5. Exceptions and return sequences");

var catalog = Mock.Create<IResourceCatalog>();
var missing =
    new IOException("The requested asset is unavailable.");
Mock.When(() => catalog.Load("missing"))
    .Throw(missing);
Mock.When(catalog.NextRetryDelay)
    .ReturnSequence(1, 2);

ExpectThrowsSame(
    missing,
    () => catalog.Load("missing"),
    "a configured exception should retain its identity");
Expect(
    catalog.NextRetryDelay() == 1
    && catalog.NextRetryDelay() == 2
    && catalog.NextRetryDelay() == 2,
    "a return sequence should repeat its final value");

Mock.Verify(() => catalog.Load("missing"))
    .Once();
Mock.Verify(catalog.NextRetryDelay)
    .Exactly(3);
Mock.VerifyNoOtherCalls(catalog);

Console.WriteLine("   sequence: 1, 2, 2; configured IOException observed");

Console.WriteLine("6. Events and ordinary ref/out values");

var signals = Mock.CreateLoose<IFrameSignals>();
var raisedFrame = -1;
signals.FrameReady += frame => raisedFrame = frame;
Mock.Raise(
    () => signals.FrameReady += null!,
    12);
Expect(
    raisedFrame == 12,
    "a raised event should pass its configured arguments");

var setupOffset = 0;
Mock.When(() => signals.TryRead(
        "fps",
        ref setupOffset,
        out _))
    .Answer(call =>
    {
        int offset = call.Argument<int>(1);
        call.SetReference(1, offset + 1);
        call.SetReference(2, "60");
        return true;
    });

var offset = 0;
Expect(
    signals.TryRead(
        "fps",
        ref offset,
        out string value),
    "the configured ref/out call should succeed");
Expect(
    offset == 1 && value == "60",
    "the callback should publish ordinary ref/out values");

var verificationOffset = 0;
Mock.Verify(() => signals.TryRead(
        "fps",
        ref verificationOffset,
        out _))
    .Once();

Console.WriteLine("   event frame: 12; ref offset: 1; out value: 60");

Console.WriteLine("7. Output spans and borrowed returns");

var buffers = Mock.Create<IBufferOperations>();
Mock.When(() => buffers.Fill(
        Arg.Any<Span<int>>(0)))
    .Answer((Span<int> destination) =>
    {
        ReadOnlySpan<int> source = [8, 13, 21];
        source.CopyTo(destination);
        return source.Length;
    });
var bufferOwner =
    new BufferOwner([34, 55]);
Mock.When(buffers.Borrow)
    .ReturnFactory(bufferOwner.Borrow);

{
    Span<int> destination = stackalloc int[3];
    Expect(
        buffers.Fill(destination) == 3,
        "the typed answer should return the filled count");
    Expect(
        destination.SequenceEqual([8, 13, 21]),
        "the typed answer should fill caller-owned output");

    ReadOnlySpan<int> borrowed = buffers.Borrow();
    Expect(
        borrowed.SequenceEqual([34, 55]),
        "the return factory should preserve borrowed storage");
}

Mock.Verify(() => buffers.Fill(
        Arg.Any<Span<int>>(0)))
    .Once();
Mock.Verify(buffers.Borrow)
    .Once();
Mock.VerifyNoOtherCalls(buffers);

Console.WriteLine("   output: [8, 13, 21]; borrowed: [34, 55]");

Console.WriteLine("8. Async answers copy borrowed input");

var asyncAnalyzer = Mock.Create<IAsyncSampleAnalyzer>();
Mock.When(() => asyncAnalyzer.SumAsync(
        Arg.Any<ReadOnlySpan<int>>(0)))
    .SnapshotArgument(
        0,
        (ReadOnlySpan<int> values) => values.ToArray())
    .Answer((ReadOnlySpan<int> values) =>
        SumCopiedAsync(values.ToArray()));

int[] asyncSamples = [1, 4, 9];
Task<int> asyncSum =
    asyncAnalyzer.SumAsync(asyncSamples);
asyncSamples.AsSpan().Fill(-1);

Expect(
    await asyncSum == 14,
    "the asynchronous work should consume the copied input");
Mock.Verify(() => asyncAnalyzer.SumAsync(
        Arg.ReadOnlySpanEqual<int>(
            0,
            [1, 4, 9])))
    .Once();
Mock.VerifyNoOtherCalls(asyncAnalyzer);

Console.WriteLine("   copied async sum: 14");

Console.WriteLine("9. Constructed generic calls");

var formatter = Mock.CreateLoose<GenericFormatter>();
Mock.When(() => formatter.Format(5))
    .Return("five");

Expect(
    formatter.Format(5) == "five",
    "a constructed generic call should configure automatically");
Expect(
    formatter.Format("unconfigured") == string.Empty,
    "another generic construction should keep loose fallback");
Mock.Verify(() => formatter.Format(5))
    .Once();
Mock.Verify(() => formatter.Format("unconfigured"))
    .Once();
Mock.VerifyNoOtherCalls(formatter);

Console.WriteLine("   Format<int>(5): five");

// Receiver-free and struct-receiver interception require Interception, so this
// Dynamic-only project deliberately omits those examples.
Console.WriteLine("All checks passed.");

// Adds one borrowed input without allocating or retaining the span.
static int Sum(ReadOnlySpan<int> values)
{
    var total = 0;
    foreach (int value in values)
        total += value;
    return total;
}

// Consumes only the owned copy after yielding asynchronously.
static async Task<int> SumCopiedAsync(int[] values)
{
    await Task.Yield();
    return Sum(values);
}

// Fails fast so the walkthrough doubles as an executable smoke check.
static void Expect(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}

// Confirms one expected failure while preserving the demo's linear narrative.
static void ExpectThrows<TException>(
    Action action,
    string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException(message);
}

// Confirms that a configured failure preserves the supplied exception object.
static void ExpectThrowsSame<TException>(
    TException expected,
    Action action,
    string message)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException actual)
        when (ReferenceEquals(actual, expected))
    {
        return;
    }

    throw new InvalidOperationException(message);
}
