namespace AlvorKit.ECS;

public interface IEntMut : IEnt
{
    bool IsAlive { get; }

    void Set<T, N>(in T value);

    bool Unset<T, N>();
}
