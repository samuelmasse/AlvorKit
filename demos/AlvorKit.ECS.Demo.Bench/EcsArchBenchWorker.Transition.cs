namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    /// <summary>Measures cached structural-transition lookup at one requested arch width.</summary>
    private EcsArchBenchSample RunTransitionLookup(string scenarioId)
    {
        int width = int.Parse(scenarioId.AsSpan()[^2..], CultureInfo.InvariantCulture);
        return Measure<RunArch>(
            scenarioId,
            () => CreateTransitionLookupCase<RunArch>(width),
            () => CreateTransitionLookupCase<WarmArch>(width).Body());
    }

    /// <summary>Creates one warmed transition lookup loop and its owned fixture state.</summary>
    private EcsArchBenchCase CreateTransitionLookupCase<A>(int width)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetWidth<A>(ent, width);
        int centerArchId = ent.Get<EntArchLoc, A>().ArchId;
        int[] fieldIds = EntArchGraph<A>.FieldIds(centerArchId).ToArray();

        for (int field = 0; field < width; field++)
        {
            EcsArchBenchShapes.ToggleField<A>(ent, field, false);
            EcsArchBenchShapes.ToggleField<A>(ent, field, true);
        }

        var state = new EcsArchBenchState([alloc], [ent], fieldIds);
        return new("lookup", options.Operations, Body, state, true);

        void Body()
        {
            long sum = 0;
            for (int operation = 0; operation < options.Operations; operation++)
            {
                int fieldId = fieldIds[operation & (width - 1)];
                sum += EntArchGraph<A>.GetTransitionArchId(centerArchId, fieldId);
            }
            longSink = sum;
        }
    }
}
