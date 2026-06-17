namespace AlvorKit.Mocking.Test;

internal abstract class InternalClassMock
{
    internal abstract event Action<string> Event;

    internal abstract string Name { get; }
    internal abstract string LastName { get; }
    protected abstract int Get { get; }
}
