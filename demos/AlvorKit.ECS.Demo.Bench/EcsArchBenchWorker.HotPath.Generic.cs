namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericScalarGetOne<A>()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, A>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericScalarGetRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<int, F00, A>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericScalarSetOne<A>()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, A>(i);
        longSink = ent.GetArchetypal<int, F00, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericScalarSetRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<int, F00, A>(i);
        int last = (operations - 1) & Afr24RotatingMask;
        longSink = ents[last].GetArchetypal<int, F00, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericWideGetOne<A>()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchWideValue, FWide, A>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericWideGetRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchWideValue, FWide, A>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericWideSetOne<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchWideValue, FWide, A>(value);
        wideSink = ent.GetArchetypal<EcsBenchWideValue, FWide, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericWideSetRotating<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchWideValue, FWide, A>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        wideSink = ents[last].GetArchetypal<EcsBenchWideValue, FWide, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericReferenceGetOne<A>()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchReference, FReference, A>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericReferenceGetRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchReference, FReference, A>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericReferenceSetOne<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, A>(value);
        referenceSink = ent.GetArchetypal<EcsBenchReference, FReference, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericReferenceSetRotating<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchReference, FReference, A>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        referenceSink = ents[last].GetArchetypal<EcsBenchReference, FReference, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericRefStructGetOne<A>()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, A>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericRefStructGetRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchRefStruct, FRefStruct, A>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericRefStructSetOne<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
        refStructSink = ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericRefStructSetRotating<A>()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        refStructSink = ents[last].GetArchetypal<EcsBenchRefStruct, FRefStruct, A>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericLocRotating<A>()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = ents[i & Afr24RotatingMask].Get<EntArchLoc, A>();
            sum += loc.RowSetId + loc.ArchId + loc.Row;
        }
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24GenericDirectoryRotating<A>()
    {
        int operations = afr24Operations;
        EntArchLoc[] locs = afr24State!.Locs;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = locs[i & Afr24RotatingMask];
            sum += EntArchColumn<int, F00, A>.ValuesAt(loc.RowSetId)!.Length;
        }
        longSink = sum;
    }
}
