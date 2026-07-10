namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private EcsArchBenchSample RunPoint(string scenarioId, PointOperation operation)
    {
        int width = int.Parse(scenarioId.AsSpan(^2), CultureInfo.InvariantCulture);
        return Measure<RunArch>(scenarioId, () => CreatePointCase<RunArch>(width, operation));
    }

    private EcsArchBenchCase CreatePointCase<A>(int width, PointOperation operation)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetWidth<A>(ent, width);
        if (operation is PointOperation.GetAbsent or PointOperation.HasAbsent)
            _ = EntArchColumn<int, F32, A>.FieldId;
        var state = new EcsArchBenchState([alloc], [ent]);

        return operation switch
        {
            PointOperation.Get => new("op", options.Operations, GetBody, state, true),
            PointOperation.GetAbsent => new("op", options.Operations, GetAbsentBody, state, true),
            PointOperation.HasPresent => new("op", options.Operations, HasPresentBody, state, true),
            PointOperation.HasAbsent => new("op", options.Operations, HasAbsentBody, state, true),
            PointOperation.Set => new("op", options.Operations, SetBody, state, true),
            _ => throw new UnreachableException(),
        };

        void GetBody()
        {
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += ent.GetArchetypal<int, F00, A>();
            longSink = sum;
        }

        void HasPresentBody()
        {
            long count = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                if (ent.HasArchetypal<int, F00, A>())
                    count++;
            }
            longSink = count;
        }

        void GetAbsentBody()
        {
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += ent.GetArchetypal<int, F32, A>();
            longSink = sum;
        }

        void HasAbsentBody()
        {
            long count = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                if (ent.HasArchetypal<int, F32, A>())
                    count++;
            }
            longSink = count;
        }

        void SetBody()
        {
            for (int i = 0; i < options.Operations; i++)
                ent.SetArchetypal<int, F00, A>(i);
            longSink = ent.GetArchetypal<int, F00, A>();
        }
    }

    private EcsArchBenchSample RunWide(string scenarioId, bool set) =>
        Measure<RunArch>(scenarioId, () => CreateWideCase<RunArch>(set));

    private EcsArchBenchCase CreateWideCase<A>(bool set)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetSevenFillers<A>(ent);
        var value = new EcsBenchWideValue(1, 2, 3, 4, 5, 6, 7, 8);
        ent.SetArchetypal<EcsBenchWideValue, FWide, A>(value);
        var state = new EcsArchBenchState([alloc], [ent]);
        return new("op", options.Operations, set ? SetBody : GetBody, state, true);

        void GetBody()
        {
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += ent.GetArchetypal<EcsBenchWideValue, FWide, A>().A;
            longSink = sum;
        }

        void SetBody()
        {
            for (int i = 0; i < options.Operations; i++)
                ent.SetArchetypal<EcsBenchWideValue, FWide, A>(value);
            wideSink = ent.GetArchetypal<EcsBenchWideValue, FWide, A>();
        }
    }

    private EcsArchBenchSample RunReference(string scenarioId, bool set) =>
        Measure<RunArch>(scenarioId, () => CreateReferenceCase<RunArch>(set));

    private EcsArchBenchCase CreateReferenceCase<A>(bool set)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetSevenFillers<A>(ent);
        var value = new EcsBenchReference(17);
        ent.SetArchetypal<EcsBenchReference, FReference, A>(value);
        var state = new EcsArchBenchState([alloc], [ent], value);
        return new("op", options.Operations, set ? SetBody : GetBody, state, true);

        void GetBody()
        {
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += ent.GetArchetypal<EcsBenchReference, FReference, A>()!.Value;
            longSink = sum;
        }

        void SetBody()
        {
            for (int i = 0; i < options.Operations; i++)
                ent.SetArchetypal<EcsBenchReference, FReference, A>(value);
            referenceSink = ent.GetArchetypal<EcsBenchReference, FReference, A>();
        }
    }

    private EcsArchBenchSample RunRefStruct(string scenarioId, bool set) =>
        Measure<RunArch>(scenarioId, () => CreateRefStructCase<RunArch>(set));

    private EcsArchBenchCase CreateRefStructCase<A>(bool set)
    {
        var alloc = new EntArena();
        EntMut ent = Alloc(alloc);
        EcsArchBenchShapes.SetSevenFillers<A>(ent);
        var token = new object();
        var value = new EcsBenchRefStruct("bench", token);
        ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
        var state = new EcsArchBenchState([alloc], [ent], token);
        return new("op", options.Operations, set ? SetBody : GetBody, state, true);

        void GetBody()
        {
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, A>().Text.Length;
            longSink = sum;
        }

        void SetBody()
        {
            for (int i = 0; i < options.Operations; i++)
                ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
            refStructSink = ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, A>();
        }
    }
}
