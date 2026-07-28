namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Provides observable base-initializer state for constructor interception.</summary>
public class ProfiledConstructorBodyBaseTarget
{
    private static readonly List<string> Events = [];

    /// <summary>Runs the mandatory base initializer.</summary>
    protected ProfiledConstructorBodyBaseTarget(int value)
    {
        BaseCalls++;
        BaseValue = value;
        Record($"base:{value}");
    }

    /// <summary>Counts base-initializer executions.</summary>
    public static int BaseCalls { get; private set; }

    /// <summary>Gets state initialized before derived-body routing.</summary>
    public int BaseValue { get; }

    /// <summary>Gets a stable snapshot of constructor ordering.</summary>
    public static string[] EventSnapshot() => [.. Events];

    /// <summary>Records one ordered constructor event.</summary>
    public static void Record(string value) => Events.Add(value);

    /// <summary>Clears base-initializer evidence.</summary>
    protected static void ResetBase()
    {
        BaseCalls = 0;
        Events.Clear();
    }
}

/// <summary>Provides one definition-wide constructor remainder.</summary>
public sealed class ProfiledConstructorBodyTarget :
    ProfiledConstructorBodyBaseTarget
{
    /// <summary>Initializes the base, then runs the interceptable derived remainder.</summary>
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public ProfiledConstructorBodyTarget(int value)
        : base(value + 100)
    {
        BodyCalls++;
        Value = value;
        Record("body");
        if (value < 0)
        {
            throw new InvalidOperationException(
                value == -31
                    ? "original constructor body -31"
                    : "original constructor body");
        }
    }

    /// <summary>Counts original derived-remainder executions.</summary>
    public static int BodyCalls { get; private set; }

    /// <summary>Gets the value written only by the original derived remainder.</summary>
    public int Value { get; }

    /// <summary>Clears all constructor ordering and side-effect evidence.</summary>
    public static void Reset()
    {
        BodyCalls = 0;
        ResetBase();
    }

}

/// <summary>Exact receiver-and-arguments constructor remainder.</summary>
public delegate void ProfiledConstructorBodyRemainder(
    ProfiledConstructorBodyTarget target,
    int value);

/// <summary>Owns the ordinary allocation site used to execute the constructor.</summary>
internal static class ProfiledConstructorBodyFactory
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static ProfiledConstructorBodyTarget Create(int value) =>
        new(value);
}
