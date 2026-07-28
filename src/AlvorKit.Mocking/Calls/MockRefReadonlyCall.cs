namespace AlvorKit.Mocking;

/// <summary>Returns a read-only managed reference to caller-owned stable storage.</summary>
/// <typeparam name="T">The referenced element type.</typeparam>
/// <returns>A read-only reference whose storage outlives the mocked call.</returns>
public delegate ref readonly T MockRefReadonlyCall<T>();
