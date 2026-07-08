namespace AlvorKit.ECS;

public interface IEnt
{
    EntHandle Handle { get; }

    T? Get<T, N>();

    bool Has<T, N>();
}
