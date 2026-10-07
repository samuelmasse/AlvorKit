namespace AlvorKit;

internal class ArchHotFixture(
    EntArena[] allocs,
    EntMut[] ents,
    EntArchLoc[] locs,
    int[][] scalarColumns,
    int[] rows,
    EcsBenchWideValue wideValue,
    EcsBenchReference? referenceValue,
    EcsBenchRefStruct refStructValue) : IDisposable
{
    internal EntMut[] Ents => ents;
    internal EntArchLoc[] Locs => locs;
    internal int[][] ScalarColumns => scalarColumns;
    internal int[] Rows => rows;
    internal EcsBenchWideValue WideValue => wideValue;
    internal EcsBenchReference? ReferenceValue => referenceValue;
    internal EcsBenchRefStruct RefStructValue => refStructValue;

    internal static ArchHotFixture Create<A>(Afr24Shape shape, Afr24WorkingSet workingSet, bool cacheScalarColumns)
    {
        int entCount = workingSet == Afr24WorkingSet.One ? 1 : 1024;
        int allocCount = workingSet == Afr24WorkingSet.One ? 1 : 4;
        var allocs = new EntArena[allocCount];

        for (int i = 0; i < allocs.Length; i++)
            allocs[i] = new EntArena();
        var ents = new EntMut[entCount];
        var locs = new EntArchLoc[entCount];
        int[][] scalarColumns = cacheScalarColumns ? new int[entCount][] : [];
        int[] rows = cacheScalarColumns ? new int[entCount] : [];

        for (int i = 0; i < ents.Length; i++)
        {
            int allocIndex = workingSet == Afr24WorkingSet.One ? 0 : i & (4 - 1);
            int signature = workingSet == Afr24WorkingSet.One ? 0 : (i >> 2) & (16 - 1);
            EntMut ent = allocs[allocIndex].Alloc();

            if (shape == Afr24Shape.Scalar)
            {
                ArchShapes.SetMask<A>(ent, 1u | (uint)(signature << 1));
                SetAfr24Target<A>(ent, shape, i + 17);
            }
            else
            {
                SetAfr24Target<A>(ent, shape, i + 17);
                ArchShapes.SetMask<A>(ent, 1u | (uint)(signature << 1));
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

    public void Dispose()
    {
        foreach (var alloc in allocs)
            alloc.Dispose();
    }
}
