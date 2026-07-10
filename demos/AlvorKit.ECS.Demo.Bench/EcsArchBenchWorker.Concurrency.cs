namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private EcsArchBenchSample RunConcurrentGet(string scenarioId, int allocCount) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateConcurrentGetCase<RunArch>(allocCount),
            () => CreatePointCase<WarmArch>(8, PointOperation.Get).Body());

    private EcsArchBenchCase CreateConcurrentGetCase<A>(int allocCount)
    {
        var threads = new Thread[allocCount];
        var allocs = new EntArena[allocCount];
        var ents = new EntMut[allocCount][];
        var ready = new CountdownEvent(allocCount);
        var start = new ManualResetEventSlim(false);
        var done = new CountdownEvent(allocCount);
        var state = new EcsArchBenchConcurrentState(threads, allocs, ents, ready, start, done);

        for (int allocId = 0; allocId < threads.Length; allocId++)
        {
            int owner = allocId;
            threads[allocId] = new Thread(() => ConcurrentGetOwner<A>(state, owner, options.Operations));
            threads[allocId].Start();
        }
        ready.Wait();

        return new("op", (long)options.Operations * allocCount, Body, state, Quiesce: state.Join);

        void Body()
        {
            start.Set();
            done.Wait();
        }
    }

    private static void ConcurrentGetOwner<A>(EcsArchBenchConcurrentState state, int owner, int operations)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetWidth<A>(ent, 8);
        state.Allocs[owner] = alloc;
        state.Ents[owner] = [ent];
        state.Ready.Signal();
        state.Start.Wait();

        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, A>();
        Interlocked.Add(ref longSink, sum);
        state.Done.Signal();
    }

    private EcsArchBenchSample RunConcurrentSet(string scenarioId, int allocCount) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateConcurrentSetCase<RunArch>(allocCount),
            () => CreatePointCase<WarmArch>(8, PointOperation.Set).Body());

    private EcsArchBenchCase CreateConcurrentSetCase<A>(int allocCount)
    {
        var threads = new Thread[allocCount];
        var allocs = new EntArena[allocCount];
        var ents = new EntMut[allocCount][];
        var ready = new CountdownEvent(allocCount);
        var start = new ManualResetEventSlim(false);
        var done = new CountdownEvent(allocCount);
        var state = new EcsArchBenchConcurrentState(threads, allocs, ents, ready, start, done);

        for (int allocId = 0; allocId < threads.Length; allocId++)
        {
            int owner = allocId;
            threads[allocId] = new Thread(() => ConcurrentSetOwner<A>(state, owner, options.Operations));
            threads[allocId].Start();
        }
        ready.Wait();

        return new("op", (long)options.Operations * allocCount, Body, state, Quiesce: state.Join);

        void Body()
        {
            start.Set();
            done.Wait();
        }
    }

    private static void ConcurrentSetOwner<A>(EcsArchBenchConcurrentState state, int owner, int operations)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetWidth<A>(ent, 8);
        state.Allocs[owner] = alloc;
        state.Ents[owner] = [ent];
        state.Ready.Signal();
        state.Start.Wait();

        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, A>(i);
        Interlocked.Add(ref longSink, ent.GetArchetypal<int, F00, A>());
        state.Done.Signal();
    }

    private EcsArchBenchSample RunConcurrentResolve(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateConcurrentResolveCase<RunArch>(options.Allocs, options.Rows),
            () => CreateAddUnknownCase<WarmArch>(Math.Min(options.Rows, 16)).Body());

    private static EcsArchBenchCase CreateConcurrentResolveCase<A>(int allocCount, int totalRows)
    {
        _ = EntArchColumn<int, FToggle, A>.FieldId;
        var threads = new Thread[allocCount];
        var allocs = new EntArena[allocCount];
        var ents = new EntMut[allocCount][];
        var ready = new CountdownEvent(allocCount);
        var start = new ManualResetEventSlim(false);
        var done = new CountdownEvent(allocCount);
        var state = new EcsArchBenchConcurrentState(threads, allocs, ents, ready, start, done);
        int rowsPerAlloc = Math.Max(1, (totalRows + allocCount - 1) / allocCount);

        for (int allocId = 0; allocId < threads.Length; allocId++)
        {
            int owner = allocId;
            threads[allocId] = new Thread(() => ConcurrentResolveOwner<A>(state, owner, rowsPerAlloc));
            threads[allocId].Start();
        }
        ready.Wait();

        long operations = (long)rowsPerAlloc * allocCount;
        return new("move", operations, Body, state, Quiesce: state.Join);

        void Body()
        {
            start.Set();
            done.Wait();
        }
    }

    private static void ConcurrentResolveOwner<A>(EcsArchBenchConcurrentState state, int owner, int rowsPerAlloc)
    {
        var alloc = new EntArena();
        var ents = new EntMut[rowsPerAlloc];
        uint fields = AdvanceCombination((1u << 8) - 1, owner * rowsPerAlloc);
        for (int i = 0; i < ents.Length; i++)
        {
            ents[i] = Alloc(alloc);
            EcsArchBenchShapes.SetMask<A>(ents[i], fields);
            fields = NextCombination(fields);
        }

        state.Allocs[owner] = alloc;
        state.Ents[owner] = ents;
        state.Ready.Signal();
        state.Start.Wait();

        for (int i = 0; i < ents.Length; i++)
            ents[i].SetArchetypal<int, FToggle, A>(i);
        Interlocked.Add(ref longSink, ents.Length);
        state.Done.Signal();
    }

    private static uint AdvanceCombination(uint fields, int count)
    {
        for (int i = 0; i < count; i++)
            fields = NextCombination(fields);
        return fields;
    }
}
