namespace AlvorKit;

internal class QueryFixture : IDisposable
{
    private readonly EntArena arena = new();
    private readonly EntMut[] ents;
    private readonly EntMut[] shuffledEnts;
    internal EntArena Arena => arena;
    internal EntMut[] Ents => ents;
    internal EntMut[] ShuffledEnts => shuffledEnts;

    internal QueryFixture(int entCount)
    {
        ents = new EntMut[entCount];

        for (int i = 0; i < ents.Length; i++)
        {
            EntMut ent = arena.Alloc();
            int value = i + 1;
            ent.Set<int, SparseValue>(value);
            ent.SetArchetypal<int, ArchValue, QueryArch>(value);
            ent.SetArchetypal<int, W0, WideQueryArch>(value);
            ent.SetArchetypal<int, W1, WideQueryArch>(value);
            ent.SetArchetypal<int, W2, WideQueryArch>(value);
            ent.SetArchetypal<int, W3, WideQueryArch>(value);
            ent.SetArchetypal<int, W4, WideQueryArch>(value);
            ent.SetArchetypal<int, W5, WideQueryArch>(value);
            ent.SetArchetypal<int, W6, WideQueryArch>(value);
            ent.SetArchetypal<int, W7, WideQueryArch>(value);
            ents[i] = ent;
        }

        shuffledEnts = [.. ents];
        var random = new Random(0x5EED);
        random.Shuffle(shuffledEnts);
    }

    public void Dispose() => arena.Dispose();
}
