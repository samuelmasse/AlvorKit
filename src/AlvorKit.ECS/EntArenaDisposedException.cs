namespace AlvorKit.ECS;

/// <summary>Thrown when an allocation is requested from a disposed entity arena.</summary>
public class EntArenaDisposedException() :
    ObjectDisposedException("Attempted to perform a write operation on a disposed EntArena.", innerException: null);
