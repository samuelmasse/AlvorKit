namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private const int Afr24RotatingEntCount = 1_024;
    private const int Afr24RotatingMask = Afr24RotatingEntCount - 1;
    private const int Afr24RotatingAllocCount = 4;
    private const int Afr24RotatingSignatureCount = 16;

    private static EcsArchAfr24State? afr24State;
    private static int afr24Operations;

    private EcsArchBenchSample RunHotPath(string scenarioId)
    {
        var spec = ParseAfr24Spec(scenarioId);
        if (spec.Stage == Afr24Stage.Row)
            return Measure<Afr24ClassArch>(scenarioId, () => CreateAfr24RowCase(spec));

        return spec.Group switch
        {
            Afr24Group.Class => Measure<Afr24ClassArch>(scenarioId, () => CreateAfr24ClassCase(spec)),
            Afr24Group.Struct => Measure<Afr24StructArch>(scenarioId, () => CreateAfr24StructCase(spec)),
            _ => throw new UnreachableException(),
        };
    }

    private EcsArchBenchCase CreateAfr24ClassCase(Afr24Spec spec)
    {
        var state = CreateAfr24State<Afr24ClassArch>(spec.Shape, spec.WorkingSet, false);
        afr24State = state;
        afr24Operations = options.Operations;
        Action body = spec.CallSite == Afr24CallSite.Concrete
            ? SelectAfr24ConcreteClass(spec)
            : SelectAfr24Generic<Afr24ClassArch>(spec);
        return new("op", afr24Operations, body, state, true);
    }

    private EcsArchBenchCase CreateAfr24StructCase(Afr24Spec spec)
    {
        var state = CreateAfr24State<Afr24StructArch>(spec.Shape, spec.WorkingSet, false);
        afr24State = state;
        afr24Operations = options.Operations;
        Action body = spec.CallSite == Afr24CallSite.Concrete
            ? SelectAfr24ConcreteStruct(spec)
            : SelectAfr24Generic<Afr24StructArch>(spec);
        return new("op", afr24Operations, body, state, true);
    }

    private EcsArchBenchCase CreateAfr24RowCase(Afr24Spec spec)
    {
        var state = CreateAfr24State<Afr24ClassArch>(Afr24Shape.Scalar, Afr24WorkingSet.Rotating, true);
        afr24State = state;
        afr24Operations = options.Operations;
        Action body = spec.Operation == Afr24Operation.Get
            ? Afr24RowGetRotating
            : Afr24RowSetRotating;
        return new("op", afr24Operations, body, state, true);
    }

    private static EcsArchAfr24State CreateAfr24State<A>(
        Afr24Shape shape,
        Afr24WorkingSet workingSet,
        bool cacheScalarColumns)
    {
        int entCount = workingSet == Afr24WorkingSet.One ? 1 : Afr24RotatingEntCount;
        int allocCount = workingSet == Afr24WorkingSet.One ? 1 : Afr24RotatingAllocCount;
        var allocs = new EntArena[allocCount];
        for (int i = 0; i < allocs.Length; i++)
            allocs[i] = new EntArena();

        var ents = new EntMut[entCount];
        var locs = new EntArchLoc[entCount];
        int[][] scalarColumns = cacheScalarColumns ? new int[entCount][] : [];
        int[] rows = cacheScalarColumns ? new int[entCount] : [];

        for (int i = 0; i < ents.Length; i++)
        {
            int allocIndex = workingSet == Afr24WorkingSet.One ? 0 : i & (Afr24RotatingAllocCount - 1);
            int signature = workingSet == Afr24WorkingSet.One ? 0 : (i >> 2) & (Afr24RotatingSignatureCount - 1);
            EntMut ent = Alloc(allocs[allocIndex]);
            if (shape == Afr24Shape.Scalar)
            {
                EcsArchBenchShapes.SetMask<A>(ent, 1u | (uint)(signature << 1));
                SetAfr24Target<A>(ent, shape, i + 17);
            }
            else
            {
                SetAfr24Target<A>(ent, shape, i + 17);
                EcsArchBenchShapes.SetMask<A>(ent, 1u | (uint)(signature << 1));
            }

            var loc = ent.Get<EntArchLoc, A>();
            ents[i] = ent;
            locs[i] = loc;
            if (cacheScalarColumns)
            {
                scalarColumns[i] = EntArchColumn<int, F00, A>.ValuesAt(loc.RowSetId)!;
                rows[i] = loc.Row;
            }
        }

        if (workingSet == Afr24WorkingSet.Rotating)
            ValidateAfr24RotatingFixture<A>(shape, ents, locs);

        return new(
            allocs,
            ents,
            locs,
            scalarColumns,
            rows,
            new(101, 102, 103, 104, 105, 106, 107, 108),
            shape == Afr24Shape.Reference ? new EcsBenchReference(109) : null,
            shape == Afr24Shape.RefStruct ? new("afr24", new object()) : default);
    }

    private static void SetAfr24Target<A>(EntMut ent, Afr24Shape shape, int seed)
    {
        switch (shape)
        {
            case Afr24Shape.Scalar:
                ent.SetArchetypal<int, F00, A>(seed);
                break;
            case Afr24Shape.Wide:
                ent.SetArchetypal<EcsBenchWideValue, FWide, A>(new(seed, 2, 3, 4, 5, 6, 7, 8));
                break;
            case Afr24Shape.Reference:
                ent.SetArchetypal<EcsBenchReference, FReference, A>(new(seed));
                break;
            case Afr24Shape.RefStruct:
                ent.SetArchetypal<EcsBenchRefStruct, FRefStruct, A>(new("afr24", new object()));
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void ValidateAfr24RotatingFixture<A>(
        Afr24Shape shape,
        ReadOnlySpan<EntMut> ents,
        ReadOnlySpan<EntArchLoc> locs)
    {
        var allocIds = new HashSet<int>();
        var pageIds = new HashSet<int>();
        var archIds = new HashSet<int>();
        var states = new HashSet<long>();
        for (int i = 0; i < ents.Length; i++)
        {
            var loc = locs[i];
            int allocId = EntReg.PageAllocators[ents[i].PageIndex];
            allocIds.Add(allocId);
            pageIds.Add(ents[i].PageIndex);
            archIds.Add(loc.ArchId);
            states.Add(((long)allocId << 32) | (uint)loc.ArchId);
            if (!HasNonzeroAfr24Target<A>(ents[i], shape))
                throw new InvalidOperationException("AFR-24 rotating fixtures require nonzero target values.");
        }

        if (allocIds.Count != Afr24RotatingAllocCount ||
            pageIds.Count != Afr24RotatingAllocCount ||
            archIds.Count != Afr24RotatingSignatureCount ||
            states.Count != Afr24RotatingAllocCount * Afr24RotatingSignatureCount)
        {
            throw new InvalidOperationException(
                "AFR-24 rotating fixtures require four alloc pages, sixteen arches, and sixty-four alloc/arch states.");
        }
    }

    private static bool HasNonzeroAfr24Target<A>(EntMut ent, Afr24Shape shape) => shape switch
    {
        Afr24Shape.Scalar => ent.GetArchetypal<int, F00, A>() != 0,
        Afr24Shape.Wide => ent.GetArchetypal<EcsBenchWideValue, FWide, A>().A != 0,
        Afr24Shape.Reference => ent.GetArchetypal<EcsBenchReference, FReference, A>()!.Value != 0,
        Afr24Shape.RefStruct => ent.GetArchetypal<EcsBenchRefStruct, FRefStruct, A>().Text.Length != 0,
        _ => throw new UnreachableException(),
    };

    private static Action SelectAfr24ConcreteClass(Afr24Spec spec) =>
        (spec.Stage, spec.Shape, spec.Operation, spec.WorkingSet) switch
        {
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteClassScalarGetOne,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteClassScalarGetRotating,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteClassScalarSetOne,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteClassScalarSetRotating,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteClassWideGetOne,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteClassWideGetRotating,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteClassWideSetOne,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteClassWideSetRotating,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteClassReferenceGetOne,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteClassReferenceGetRotating,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteClassReferenceSetOne,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteClassReferenceSetRotating,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteClassRefStructGetOne,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteClassRefStructGetRotating,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteClassRefStructSetOne,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteClassRefStructSetRotating,
            (Afr24Stage.Loc, _, _, Afr24WorkingSet.Rotating) => Afr24ConcreteClassLocRotating,
            (Afr24Stage.Directory, _, _, Afr24WorkingSet.Rotating) => Afr24ConcreteClassDirectoryRotating,
            _ => throw new UnreachableException(),
        };

    private static Action SelectAfr24ConcreteStruct(Afr24Spec spec) =>
        (spec.Stage, spec.Shape, spec.Operation, spec.WorkingSet) switch
        {
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteStructScalarGetOne,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteStructScalarGetRotating,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteStructScalarSetOne,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteStructScalarSetRotating,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteStructWideGetOne,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteStructWideGetRotating,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteStructWideSetOne,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteStructWideSetRotating,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteStructReferenceGetOne,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteStructReferenceGetRotating,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteStructReferenceSetOne,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteStructReferenceSetRotating,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24ConcreteStructRefStructGetOne,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24ConcreteStructRefStructGetRotating,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24ConcreteStructRefStructSetOne,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24ConcreteStructRefStructSetRotating,
            (Afr24Stage.Loc, _, _, Afr24WorkingSet.Rotating) => Afr24ConcreteStructLocRotating,
            (Afr24Stage.Directory, _, _, Afr24WorkingSet.Rotating) => Afr24ConcreteStructDirectoryRotating,
            _ => throw new UnreachableException(),
        };

    private static Action SelectAfr24Generic<A>(Afr24Spec spec) =>
        (spec.Stage, spec.Shape, spec.Operation, spec.WorkingSet) switch
        {
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24GenericScalarGetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24GenericScalarGetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24GenericScalarSetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Scalar, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24GenericScalarSetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24GenericWideGetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24GenericWideGetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24GenericWideSetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Wide, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24GenericWideSetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24GenericReferenceGetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24GenericReferenceGetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24GenericReferenceSetOne<A>,
            (Afr24Stage.Full, Afr24Shape.Reference, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24GenericReferenceSetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.One) => Afr24GenericRefStructGetOne<A>,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Get, Afr24WorkingSet.Rotating) => Afr24GenericRefStructGetRotating<A>,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.One) => Afr24GenericRefStructSetOne<A>,
            (Afr24Stage.Full, Afr24Shape.RefStruct, Afr24Operation.Set, Afr24WorkingSet.Rotating) => Afr24GenericRefStructSetRotating<A>,
            (Afr24Stage.Loc, _, _, Afr24WorkingSet.Rotating) => Afr24GenericLocRotating<A>,
            (Afr24Stage.Directory, _, _, Afr24WorkingSet.Rotating) => Afr24GenericDirectoryRotating<A>,
            _ => throw new UnreachableException(),
        };

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24RowGetRotating()
    {
        int operations = afr24Operations;
        int[][] columns = afr24State!.ScalarColumns;
        int[] rows = afr24State.Rows;
        long sum = 0;
        for (int i = 0; i < operations; i++)
        {
            int index = i & Afr24RotatingMask;
            sum += columns[index][rows[index]];
        }
        longSink = sum;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Afr24RowSetRotating()
    {
        int operations = afr24Operations;
        int[][] columns = afr24State!.ScalarColumns;
        int[] rows = afr24State.Rows;
        for (int i = 0; i < operations; i++)
        {
            int index = i & Afr24RotatingMask;
            columns[index][rows[index]] = i;
        }

        int last = (operations - 1) & Afr24RotatingMask;
        longSink = columns[last][rows[last]];
    }

    private static Afr24Spec ParseAfr24Spec(string scenarioId)
    {
        string[] parts = scenarioId.Split('-');
        if (parts[2] == "full")
        {
            return new(
                Afr24Stage.Full,
                ParseAfr24Shape(parts[3]),
                ParseAfr24Operation(parts[4]),
                ParseAfr24CallSite(parts[5]),
                ParseAfr24Group(parts[6]),
                ParseAfr24WorkingSet(parts[7]));
        }

        return parts[3] switch
        {
            "loc" => new(
                Afr24Stage.Loc,
                Afr24Shape.Scalar,
                Afr24Operation.Get,
                ParseAfr24CallSite(parts[4]),
                ParseAfr24Group(parts[5]),
                ParseAfr24WorkingSet(parts[6])),
            "directory" => new(
                Afr24Stage.Directory,
                Afr24Shape.Scalar,
                Afr24Operation.Get,
                ParseAfr24CallSite(parts[4]),
                ParseAfr24Group(parts[5]),
                ParseAfr24WorkingSet(parts[6])),
            "row" => new(
                Afr24Stage.Row,
                Afr24Shape.Scalar,
                ParseAfr24Operation(parts[4]),
                Afr24CallSite.Concrete,
                Afr24Group.Class,
                ParseAfr24WorkingSet(parts[5])),
            _ => throw new ArgumentOutOfRangeException("--worker-case", $"Unknown AFR-24 case '{scenarioId}'."),
        };
    }

    private static Afr24Shape ParseAfr24Shape(string value) => value switch
    {
        "scalar" => Afr24Shape.Scalar,
        "wide" => Afr24Shape.Wide,
        "reference" => Afr24Shape.Reference,
        "refstruct" => Afr24Shape.RefStruct,
        _ => throw new UnreachableException(),
    };

    private static Afr24Operation ParseAfr24Operation(string value) => value switch
    {
        "get" => Afr24Operation.Get,
        "set" => Afr24Operation.Set,
        _ => throw new UnreachableException(),
    };

    private static Afr24CallSite ParseAfr24CallSite(string value) => value switch
    {
        "concrete" => Afr24CallSite.Concrete,
        "generic" => Afr24CallSite.Generic,
        _ => throw new UnreachableException(),
    };

    private static Afr24Group ParseAfr24Group(string value) => value switch
    {
        "class" => Afr24Group.Class,
        "struct" => Afr24Group.Struct,
        _ => throw new UnreachableException(),
    };

    private static Afr24WorkingSet ParseAfr24WorkingSet(string value) => value switch
    {
        "one" => Afr24WorkingSet.One,
        "r1024" => Afr24WorkingSet.Rotating,
        _ => throw new UnreachableException(),
    };

    private readonly record struct Afr24Spec(
        Afr24Stage Stage,
        Afr24Shape Shape,
        Afr24Operation Operation,
        Afr24CallSite CallSite,
        Afr24Group Group,
        Afr24WorkingSet WorkingSet);

    private enum Afr24Stage { Full, Loc, Directory, Row }
    private enum Afr24Shape { Scalar, Wide, Reference, RefStruct }
    private enum Afr24Operation { Get, Set }
    private enum Afr24CallSite { Concrete, Generic }
    private enum Afr24Group { Class, Struct }
    private enum Afr24WorkingSet { One, Rotating }
}

internal sealed class Afr24ClassArch;
internal readonly struct Afr24StructArch;

internal sealed class EcsArchAfr24State(
    EntArena[] allocs,
    EntMut[] ents,
    EntArchLoc[] locs,
    int[][] scalarColumns,
    int[] rows,
    EcsBenchWideValue wideValue,
    EcsBenchReference? referenceValue,
    EcsBenchRefStruct refStructValue)
{
    internal readonly EntArena[] Allocs = allocs;
    internal readonly EntMut[] Ents = ents;
    internal readonly EntArchLoc[] Locs = locs;
    internal readonly int[][] ScalarColumns = scalarColumns;
    internal readonly int[] Rows = rows;
    internal readonly EcsBenchWideValue WideValue = wideValue;
    internal readonly EcsBenchReference? ReferenceValue = referenceValue;
    internal readonly EcsBenchRefStruct RefStructValue = refStructValue;
}
