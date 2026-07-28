namespace AlvorKit.Mocking.Test.Contracts;

// This compiling source-shape fixture is the ordinary public API contract.
// Ref-safe and receiver-free shapes live in dedicated contract fixtures.
internal static class MockingApiContract
{
    public static void Construction(Worker existing)
    {
        IWorker strict = Mock.Create<IWorker>();
        IWorker loose = Mock.CreateLoose<IWorker>();

        object runtimeStrict = Mock.Create(
            typeof(IWorker),
            MockBehavior.Strict);
        object runtimeLoose = Mock.Create(
            typeof(IWorker),
            MockBehavior.Loose);

        Worker partial = Mock.Partial(existing);

        _ = strict;
        _ = loose;
        _ = runtimeStrict;
        _ = runtimeLoose;
        _ = partial;
    }

    public static void OrdinaryBehavior(
        IWorker worker,
        Job expectedJob,
        List<Notice> observed)
    {
        Mock.When(() => worker.CurrentJob)
            .Return(expectedJob);

        Mock.When(() => worker.TryWork(Arg.Any<Job>()))
            .ReturnSequence(false, false, true);

        Mock.When(() => worker.Score(Arg.Any<Job>()))
            .Answer(call => call.Argument<Job>(0).Priority * 10);

        Mock.When(() => worker.Publish(Arg.Any<Notice>()))
            .Do(call => observed.Add(call.Argument<Notice>(0)));

        Mock.When(() => worker.Load(Arg.Any<string>()))
            .Throw(new IOException("unavailable"));

        Mock.When(() => worker.Reset())
            .Throw(new InvalidOperationException("unavailable"));

        Mock.When(() => worker.TryGetJob(Arg.Any<string>(), out _))
            .Answer(call =>
            {
                call.SetReference(1, expectedJob);
                return true;
            });
    }

    public static void Verification(
        IWorker worker,
        Notice expected)
    {
        Mock.Verify(() => worker.TryWork(Arg.Any<Job>()))
            .Exactly(3);

        Mock.Verify(() => worker.Publish(
                Arg.Match<Notice>(notice => notice.Kind == expected.Kind)))
            .Once();

        Mock.Verify(() => worker.Cancel(Arg.Any<Job>()))
            .Never();

        Mock.Verify(() => worker.Load(Arg.Any<string>()))
            .AtLeast(1);

        Mock.Verify(() => worker.Score(Arg.Any<Job>()))
            .AtMost(4);

        Mock.VerifyNoOtherCalls(worker);
        Mock.ClearInvocations(worker);
    }

    public static void Events(IWorker worker)
    {
        static void handler(object? _1, EventArgs _2) { }
        worker.Completed += handler;

        Mock.Raise(
            () => worker.Completed += null!,
            worker,
            EventArgs.Empty);

        worker.Completed -= handler;
    }

    public static void Sessions(
        IWorker worker,
        IJobs jobs,
        IMovement movement,
        ITerrain terrain)
    {
        using var session = Mock.Session();

        var beforeWork = session.Checkpoint();
        worker.Update();
        var afterWork = session.Checkpoint();

        Mock.Verify(() => jobs.Claim(Arg.Any<Job>()))
            .Between(beforeWork, afterWork)
            .Once();

        session.VerifySequence(
            () => jobs.Claim(Arg.Any<Job>()),
            () => movement.Begin(Arg.Any<Cell>()),
            () => terrain.Mine(Arg.Any<Cell>()));
    }

    public static void ConstructedGenericWithoutPreparation(
        IWorker worker)
    {
        Mock.When(() => worker.Convert<int, string>(42))
            .Return("forty-two");
    }
}

// Dedicated compiling fixtures cover typed and ref-safe callbacks, managed and
// ref-struct returns, receiver-free interception, and struct interception.

internal interface IWorker
{
    event EventHandler Completed;

    Job CurrentJob { get; }

    bool TryWork(Job job);

    int Score(Job job);

    void Publish(Notice notice);

    object Load(string key);

    void Reset();

    bool TryGetJob(string name, out Job job);

    void Cancel(Job job);

    void Update();

    TResult Convert<TSource, TResult>(TSource value);
}

internal interface IJobs
{
    void Claim(Job job);
}

internal interface IMovement
{
    void Begin(Cell target);
}

internal interface ITerrain
{
    void Mine(Cell target);
}

internal sealed class Worker
{
}

internal sealed record Job(int Priority);

internal sealed record Notice(string Kind);

internal readonly record struct Cell(int X, int Y, int Z);
