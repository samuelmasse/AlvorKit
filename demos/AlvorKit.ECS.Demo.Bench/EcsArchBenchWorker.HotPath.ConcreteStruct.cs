namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructScalarGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<int, F00, Afr24StructArch>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructScalarGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<int, F00, Afr24StructArch>();
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructScalarSetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<int, F00, Afr24StructArch>(i);
        longSink = ent.GetArchetypal<int, F00, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructScalarSetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<int, F00, Afr24StructArch>(i);
        int last = (operations - 1) & Afr24RotatingMask;
        longSink = ents[last].GetArchetypal<int, F00, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructWideGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructWideGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>().A;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructWideSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>(value);
        wideSink = ent.GetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructWideSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchWideValue value = state.WideValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        wideSink = ents[last].GetArchetypal<EcsBenchWideValue, FWide, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructReferenceGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchReference, FReference, Afr24StructArch>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructReferenceGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchReference, FReference, Afr24StructArch>()!.Value;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructReferenceSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchReference, FReference, Afr24StructArch>(value);
        referenceSink = ent.GetArchetypal<EcsBenchReference, FReference, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructReferenceSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchReference value = state.ReferenceValue!;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchReference, FReference, Afr24StructArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        referenceSink = ents[last].GetArchetypal<EcsBenchReference, FReference, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructRefStructGetOne()
    {
        int operations = afr24Operations;
        EntMut ent = afr24State!.Ents[0];
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructRefStructGetRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
            sum += ents[i & Afr24RotatingMask].GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>().Text.Length;
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructRefStructSetOne()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut ent = state.Ents[0];
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>(value);
        refStructSink = ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructRefStructSetRotating()
    {
        int operations = afr24Operations;
        var state = afr24State!;
        EntMut[] ents = state.Ents;
        EcsBenchRefStruct value = state.RefStructValue;
        for (int i = 0; i < operations; i++)
            ents[i & Afr24RotatingMask].SetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>(value);
        int last = (operations - 1) & Afr24RotatingMask;
        refStructSink = ents[last].GetArchetypal<EcsBenchRefStruct, FRefStruct, Afr24StructArch>();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructLocRotating()
    {
        int operations = afr24Operations;
        EntMut[] ents = afr24State!.Ents;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = ents[i & Afr24RotatingMask].Get<EntArchLoc, Afr24StructArch>();
            sum += loc.RowSetId + loc.ArchId + loc.Row;
        }
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24ConcreteStructDirectoryRotating()
    {
        int operations = afr24Operations;
        EntArchLoc[] locs = afr24State!.Locs;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            var loc = locs[i & Afr24RotatingMask];
            sum += EntArchColumn<int, F00, Afr24StructArch>.ValuesAt(loc.RowSetId)!.Length;
        }
        longSink = sum;
    }
}
