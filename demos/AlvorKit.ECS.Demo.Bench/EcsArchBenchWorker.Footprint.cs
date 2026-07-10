namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private EcsArchBenchSample RunUniqueSignatures(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateUniqueSignatureCase<RunArch>(options.Arches),
            () => CreateUniqueSignatureCase<WarmArch>(Math.Min(options.Arches, 32)).Body());

    private static EcsArchBenchCase CreateUniqueSignatureCase<A>(int archCount)
    {
        var alloc = new EntArena();
        EcsArchBenchShapes.RegisterFields<A>(32);
        EntMut ent = Alloc(alloc);
        var state = new EcsArchBenchState([alloc], [ent]);
        return new("arch", archCount, Body, state);

        void Body()
        {
            uint previous = 0;
            for (int i = 1; i <= archCount; i++)
            {
                uint signature = (uint)i ^ ((uint)i >> 1);
                uint changed = previous ^ signature;
                int field = BitOperations.TrailingZeroCount(changed);
                EcsArchBenchShapes.ToggleField<A>(ent, field, (signature & changed) != 0);
                previous = signature;
            }
            longSink = ent.GetArchetypal<int, F00, A>();
        }
    }

    private EcsArchBenchSample RunLowOccupancy(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateLowOccupancyCase<RunArch>(options.Arches),
            () => CreateLowOccupancyCase<WarmArch>(Math.Min(options.Arches, 16)).Body());

    private static EcsArchBenchCase CreateLowOccupancyCase<A>(int archCount)
    {
        var alloc = new EntArena();
        EcsArchBenchShapes.RegisterFields<A>(32);
        var ents = new EntMut[archCount];
        var state = new EcsArchBenchState([alloc], ents);
        return new("arch", archCount, Body, state);

        void Body()
        {
            for (int i = 0; i < ents.Length; i++)
            {
                ents[i] = Alloc(alloc);
                uint signature = (uint)(i + 1) ^ ((uint)(i + 1) >> 1);
                EcsArchBenchShapes.SetMask<A>(ents[i], signature);
            }
            longSink = ents.Length;
        }
    }

    private EcsArchBenchSample RunHighOccupancy(string scenarioId) =>
        Measure<RunArch>(
            scenarioId,
            () => CreateHighOccupancyCase<RunArch>(options.Rows),
            () => CreateHighOccupancyCase<WarmArch>(Math.Min(options.Rows, 16)).Body());

    private static EcsArchBenchCase CreateHighOccupancyCase<A>(int rowCount)
    {
        var alloc = new EntArena();
        EcsArchBenchShapes.RegisterFields<A>(16);
        var ents = new EntMut[rowCount];
        var state = new EcsArchBenchState([alloc], ents);
        return new("row", rowCount, Body, state);

        void Body()
        {
            for (int i = 0; i < ents.Length; i++)
            {
                ents[i] = Alloc(alloc);
                int width = (i & 3) switch
                {
                    0 => 1,
                    1 => 4,
                    2 => 8,
                    _ => 16,
                };
                EcsArchBenchShapes.SetWidth<A>(ents[i], width);
            }
            longSink = ents.Length;
        }
    }
}
