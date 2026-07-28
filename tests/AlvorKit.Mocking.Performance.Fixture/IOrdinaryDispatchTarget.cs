namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Provides an ordinary boxed-dispatch target for the isolated fixture.</summary>
public interface IOrdinaryDispatchTarget
{
    /// <summary>Returns a value derived from one ordinary argument.</summary>
    int Invoke(int value);
}
