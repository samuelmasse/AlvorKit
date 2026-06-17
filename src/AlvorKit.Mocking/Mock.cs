namespace AlvorKit.Mocking;

/// <summary>Creates and configures runtime mocks for interfaces, classes, sealed classes, and existing instances.</summary>
public static partial class Mock
{
    /// <summary>Tracks mocked sealed instances and partially mocked real instances that cannot carry <see cref="IMock"/> state.</summary>
    internal static ConditionalWeakTable<object, Mocked> Sealed = [];

    /// <summary>Returns the mock state attached to an object, or <see langword="null"/> when the object is not mocked.</summary>
    internal static Mocked? GetMocked(object obj)
    {
        if (obj is IMock mock)
            return mock.__Mocked_cc6d2cf7;
        else if (Sealed.TryGetValue(obj, out var val))
            return val;
        else return null;
    }
}
