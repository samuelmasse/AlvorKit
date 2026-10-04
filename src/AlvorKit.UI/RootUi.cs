namespace AlvorKit;

[Root]
public class RootUi : IEntMut, IDisposable
{
    public static implicit operator Ent(RootUi value) => value.ent;
    public static implicit operator EntMut(RootUi value) => value.ent;

    private readonly NodeArrayAllocator allocator = new();
    private readonly EntArena arena = new();
    private readonly EntMut ent;
    private long nextId = 1;
    private long alive = 1;
    private List<EntPtr> ents = [];
    private List<EntPtr> buffer = [];
    private TraversalState traversal = new()
    {
        Nodes = new EntMut[16],
        OrderKeys = new EntMut[16],
        OrderValues = new float[16],
    };

    internal NodeArrayAllocator Allocator => allocator;
    internal ref TraversalState Traversal => ref traversal;

    public EntHandle Handle => ent.Handle;
    public bool IsAlive => ent.IsAlive;
    public override string ToString() => ent.ToString();

    public RootUi()
    {
        ent = arena.Alloc();
        this.UiId = nextId++;
        this.UiRoot = this;
    }

    public T? Get<T, N>() => ent.Get<T, N>();
    public bool Has<T, N>() => ent.Has<T, N>();
    public void Set<T, N>(in T value) => ent.Set<T, N>(value);
    public bool Unset<T, N>() => ent.Unset<T, N>();
    public T? GetArchetypal<T, N, A>() => ent.GetArchetypal<T, N, A>();
    public bool HasArchetypal<T, N, A>() => ent.HasArchetypal<T, N, A>();
    public void SetArchetypal<T, N, A>(in T value) => ent.SetArchetypal<T, N, A>(value);
    public bool UnsetArchetypal<T, N, A>() => ent.UnsetArchetypal<T, N, A>();

    /// <summary>Invalidates the root and every child, releasing components, callbacks, and retained layout storage.</summary>
    public void Dispose()
    {
        arena.Dispose();
        ents.Clear();
        buffer.Clear();
        allocator.Clear();
        traversal = new() { Nodes = [], OrderKeys = [], OrderValues = [] };
    }

    public EntMut Alloc()
    {
        var ent = arena.Alloc();
        ent.UiId = nextId++;
        ent.UiRoot = this;

        ents.Add(ent);

        return ent;
    }

    public void Cleanup()
    {
        alive++;

        Mark((EntMut)this);

        foreach (var ent in ents)
        {
            if (ent.UiToken != alive)
            {
                allocator.Free(ent.UiNodes);
                allocator.Free(ent.UiNodeStack);
                ent.Dispose();
            }
            else buffer.Add(ent);
        }

        (ents, buffer) = (buffer, ents);
        buffer.Clear();
    }

    private void Mark(EntMut ent)
    {
        ent.UiToken = alive;

        var companion = ent.CompanionFV.Resolve();
        if (companion != default)
            Mark(companion);

        foreach (var child in Nodes(ent))
            Mark(child);

        foreach (var child in NodeStack(ent))
            Mark(child);
    }

    internal struct TraversalState
    {
        public EntMut[] Nodes;
        public int Index;
        public EntMut[] OrderKeys;
        public float[] OrderValues;
    }
}
