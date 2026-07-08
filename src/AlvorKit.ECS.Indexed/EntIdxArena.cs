namespace AlvorKit.ECS.Indexed;

public class EntIdxArena(EntObj context) : IDisposable
{
    private readonly EntArena arena = new();

        public int Allocated => arena.Allocated;

        public bool IsAlive => arena.IsAlive;

        public virtual EntPtrIdx Alloc() => new(arena.Alloc(), (Ent)context);

        public virtual void Dispose() => arena.Dispose();
}
