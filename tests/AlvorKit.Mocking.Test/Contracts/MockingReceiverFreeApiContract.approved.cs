namespace AlvorKit;

// These methods compile the public receiver-free surface without executing setup
// before the owned-assembly runtime lane binds it.
internal static class MockingReceiverFreeApiContract
{
    private static readonly MockField<ReceiverFreeJob> CurrentJob =
        Mock.Field<ReceiverFreeWorker, ReceiverFreeJob>("currentJob");
    private static readonly MockField<int> GlobalVersion =
        Mock.Field<ReceiverFreeWorker, int>("globalVersion");

    internal static void StaticMethodsAndProperties(
        DateTimeOffset fixedTime)
    {
        MockCallSite clockSite =
            Mock.Site(() => ReceiverFreeSubject.ReadClock());
        MockCallSite resetSite =
            Mock.Site(() => ReceiverFreeSubject.Reset());

        Mock.When(() => ReceiverFreeSubject.UtcNow)
            .AtSite(clockSite)
            .Return(fixedTime);
        Mock.When(() => ReceiverFreeSubject.Reset())
            .AtSite(resetSite)
            .Passthrough();
        Mock.When(() => ReceiverFreeSubject.Fail())
            .Strict();

        Mock.Verify(() => ReceiverFreeSubject.UtcNow)
            .AtSite(clockSite)
            .Once();
    }

    internal static void Construction(
        Buffer substitute)
    {
        MockCallSite allocationSite =
            Mock.Site(() => ReceiverFreeSubject.CreateBuffer(16));

        Mock.WhenNew(() => new Buffer(Arg.Any<int>()))
            .AtSite(allocationSite)
            .Substitute(substitute);
        Mock.WhenNew(() => new Buffer(0))
            .Passthrough();
        Mock.WhenNew(() => new Buffer(-1))
            .Strict();

        BufferFactory factory =
            capacity => new(capacity + 1);
        Mock.WhenNew(() => new Buffer(Arg.Any<int>()))
            .SubstituteFactory(factory);

        Mock.VerifyNew(() => new Buffer(Arg.Any<int>()))
            .AtSite(allocationSite)
            .AtLeast(1);
    }

    internal static void ConstructorBodies()
    {
        BufferObserver observer =
            (buffer, capacity) => buffer.ObservedCapacity = capacity;
        BufferObserver replacement =
            (buffer, capacity) => buffer.ObservedCapacity = -capacity;

        Mock.WhenConstructorBody(
                () => new Buffer(Arg.Any<int>()))
            .Observe(observer);
        Mock.WhenConstructorBody(
                () => new Buffer(Arg.Any<int>()))
            .Replace(replacement);
        Mock.WhenConstructorBody(() => new Buffer(0))
            .Passthrough();

        Mock.VerifyConstructorBody(
                () => new Buffer(Arg.Any<int>()))
            .Exactly(2);
    }

    internal static void Fields(
        ReceiverFreeWorker worker,
        ReceiverFreeJob expected,
        MockCallSite readSite,
        MockCallSite writeSite)
    {
        MockField<int> reflected =
            Mock.Field<int>(GlobalVersion.Metadata);

        Mock.WhenFieldRead(worker, CurrentJob)
            .AtSite(readSite)
            .Return(expected);
        Mock.WhenFieldRead(GlobalVersion)
            .Transform(
                (scoped in value) => value + 1);

        Mock.WhenFieldWrite(
                worker,
                CurrentJob,
                () => Arg.Any<ReceiverFreeJob>())
            .AtSite(writeSite)
            .Observe(
                (scoped in value) =>
                    _ = value.Priority);
        Mock.WhenFieldWrite(
                GlobalVersion,
                () => Arg.Any<int>())
            .Transform(
                (scoped in value) => value + 1);

        Mock.VerifyFieldRead(worker, CurrentJob)
            .AtSite(readSite)
            .Once();
        Mock.VerifyFieldWrite(
                worker,
                CurrentJob,
                () => Arg.Any<ReceiverFreeJob>())
            .AtSite(writeSite)
            .Once();

        _ = reflected;
    }

    internal static void RefStructDelegates()
    {
        static void observer(scoped in ReadOnlySpan<int> value) =>
                _ = value.Length;
        static ReadOnlySpan<int> transform(scoped in ReadOnlySpan<int> value) =>
                value[1..];

        _ = (MockValueObserver<ReadOnlySpan<int>>)observer;
        _ = (MockValueTransform<ReadOnlySpan<int>>)transform;
    }
}

internal delegate Buffer BufferFactory(int capacity);

internal delegate void BufferObserver(
    Buffer buffer,
    int capacity);

internal static class ReceiverFreeSubject
{
    internal static DateTimeOffset UtcNow => default;

    internal static void Reset()
    {
    }

    internal static void Fail()
    {
    }

    internal static DateTimeOffset ReadClock() => UtcNow;

    internal static Buffer CreateBuffer(int capacity) =>
        new(capacity);
}

internal sealed class Buffer(int capacity)
{
    internal int Capacity { get; } = capacity;

    internal int ObservedCapacity { get; set; }
}

internal sealed class ReceiverFreeWorker
{
    private readonly ReceiverFreeJob? currentJob = new(0);
    private static readonly int globalVersion = 1;

    internal ReceiverFreeJob? ReadCurrentJob() => currentJob;

    internal static int ReadGlobalVersion() => globalVersion;
}

internal sealed record ReceiverFreeJob(int Priority);
