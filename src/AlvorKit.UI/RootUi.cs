namespace AlvorKit.UI;

using AlvorKit.UI.Root;

[Root]
public class RootUi : EntObj
{
    public static implicit operator Ent(RootUi value) => (Ent)(value as EntObj);
    public static implicit operator EntMut(RootUi value) => (EntMut)(value as EntObj);

    private readonly NodeArrayAllocator allocator = new();
    private readonly EntArena arena = new();
    private long nextId = 1;
    private long alive = 1;
    private List<EntPtr> ents = [];
    private List<EntPtr> buffer = [];

    internal NodeArrayAllocator Allocator => allocator;

    public RootUi()
    {
        this.UiId = nextId++;
        this.UiRoot = this;
    }

    ~RootUi() => arena.Dispose();

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
}
