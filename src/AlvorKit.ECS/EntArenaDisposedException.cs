namespace AlvorKit.ECS;

public class EntArenaDisposedException() :
    ObjectDisposedException("Attempted to perform a write operation on a disposed EntArena.", innerException: null);
