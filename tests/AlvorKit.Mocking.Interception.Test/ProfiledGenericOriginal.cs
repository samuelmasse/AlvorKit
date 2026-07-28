namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Preserves untouched generic operations for wrapper fallbacks.</summary>
internal static class ProfiledGenericOriginal
{
    /// <summary>Invokes one closed generic target's untouched echo operation.</summary>
    internal static T ClosedEcho<T>(
        ProfiledGenericTarget<T> target,
        T value) =>
        target.Echo(value);

    /// <summary>Invokes one closed generic target's untouched property getter.</summary>
    internal static T ClosedValue<T>(
        ProfiledGenericTarget<T> target) =>
        target.Value;

    /// <summary>Invokes one untouched construction of the concrete generic method.</summary>
    internal static T ConstructedEcho<T>(
        ProfiledConstructedGenericTarget target,
        T value) =>
        target.Echo(value);
}
