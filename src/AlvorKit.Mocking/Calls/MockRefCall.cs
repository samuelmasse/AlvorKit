namespace AlvorKit;

/// <summary>Returns a mutable managed reference to caller-owned stable storage.</summary>
/// <typeparam name="T">The referenced element type.</typeparam>
/// <returns>A mutable reference whose storage outlives the mocked call.</returns>
public delegate ref T MockRefCall<T>();
