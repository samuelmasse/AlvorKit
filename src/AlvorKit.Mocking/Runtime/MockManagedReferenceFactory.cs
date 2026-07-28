namespace AlvorKit.Mocking;

/// <summary>Returns one mutable alias through the internal exact dispatch ABI.</summary>
/// <typeparam name="T">The referenced element type.</typeparam>
internal delegate ref T MockManagedReferenceFactory<T>();
