namespace AlvorKit.Mocking.Performance.Fixture;

/// <summary>Provides a zero-argument configured-dispatch target.</summary>
public interface IZeroArgumentDispatchTarget
{
    /// <summary>Returns one configured value.</summary>
    int Invoke();
}
