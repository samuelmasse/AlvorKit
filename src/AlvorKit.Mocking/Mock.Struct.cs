namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>
    /// Creates a type-wide struct scope for the current session. The scope
    /// retains neither a receiver value nor a storage address.
    /// </summary>
    public static MockStructScope<T> Struct<T>()
        where T : struct =>
        new();
}
