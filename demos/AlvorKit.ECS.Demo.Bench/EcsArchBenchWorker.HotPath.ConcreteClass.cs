namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassScalarGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, Afr24ClassArch>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassScalarGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<int, F00, Afr24ClassArch>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassScalarSetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, Afr24ClassArch>(i);
        longSink = ent.GetArchetypal<int, F00, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassScalarSetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<int, F00, Afr24ClassArch>(i);
        int last = (operations - 1) & Afr24RotatingMask;
        longSink = ents[last].GetArchetypal<int, F00, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassWideGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassWideGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassWideSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>(value);
        wideSink = ent.GetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassWideSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        wideSink = ents[last].GetArchetypal<EcsBenchWideValue, FWide, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassReferenceGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassReferenceGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassReferenceSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>(value);
        referenceSink = ent.GetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassReferenceSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        referenceSink = ents[last].GetArchetypal<EcsBenchReference, FReference, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassRefStructGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassRefStructGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassRefStructSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>(value);
        refStructSink = ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassRefStructSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        refStructSink = ents[last].GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24ClassArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassLocRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = ents[i & Afr24RotatingMask].Get<EntArchLoc, Afr24ClassArch>();
            sum += loc.RowSetId + loc.ArchId + loc.Row;
        }
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteClassDirectoryRotating()
    {
        int operations = afr24Operations;
        EntArchLoc[] locs = afr24State!.Locs;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = locs[i & Afr24RotatingMask];
            sum += EntArchColumn<int, F00, Afr24ClassArch>.ValuesAt(loc.RowSetId)!.Length;
        }
        longSink = sum;
    }
}
