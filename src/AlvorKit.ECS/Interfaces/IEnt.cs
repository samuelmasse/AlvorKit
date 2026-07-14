namespace AlvorKit.ECS;

public interface IEnt
{
    EntHandle Handle { get; }

    T? Get<T, N>();

    bool Has<T, N>();

    /// <summary>Gets an archetypal component or the default value when this Ent does not have it.</summary>
    T? GetArchetypal<T, N, A>()
    {
        var handle = Handle;
        return new EntMut(handle.Index, handle.Generation).GetArchetypal<T, N, A>();
    }

    /// <summary>Returns whether this Ent has an archetypal component in group <typeparamref name="A"/>.</summary>
    bool HasArchetypal<T, N, A>()
    {
        var handle = Handle;
        return new EntMut(handle.Index, handle.Generation).HasArchetypal<T, N, A>();
    }
}
