namespace AlvorKit.Mocking;

public static partial class Mock
{
    /// <summary>Creates and enters a logical mock session for the current execution context.</summary>
    public static MockSession Session() => new();
}
