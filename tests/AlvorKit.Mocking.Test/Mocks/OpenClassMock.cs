namespace AlvorKit.Mocking.Test;

public abstract class OpenClassMock
{
    public abstract int MethodUnmanaged<T>() where T : unmanaged;
    public abstract int MethodInterface<T>() where T : IList<T>;
}
