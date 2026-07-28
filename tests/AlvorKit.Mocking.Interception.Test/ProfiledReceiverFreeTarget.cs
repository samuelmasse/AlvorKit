namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Provides observable receiver-free operations for profiled no-session fallback.</summary>
public sealed class ProfiledReceiverFreeTarget
{
    private static int staticPropertyValue;

    /// <summary>Creates one target and records that the original constructor ran.</summary>
    internal ProfiledReceiverFreeTarget(int value)
    {
        ConstructorCalls++;
        InstanceField = value;
    }

    /// <summary>Counts original static method and property calls.</summary>
    internal static int StaticCalls { get; private set; }

    /// <summary>Counts original allocations.</summary>
    internal static int ConstructorCalls { get; private set; }

    /// <summary>Stores a directly accessed static field value.</summary>
    internal static int StaticField;

    /// <summary>Stores a directly accessed instance field value.</summary>
    internal int InstanceField;

    /// <summary>Stores a directly accessed reference field value.</summary>
    internal string? InstanceReferenceField;

    /// <summary>Gets or sets an observable static property.</summary>
    internal static int StaticNumber
    {
        get
        {
            StaticCalls++;
            return staticPropertyValue;
        }
        set
        {
            StaticCalls++;
            staticPropertyValue = value;
        }
    }

    /// <summary>Transforms a scalar and records original execution.</summary>
    internal static int Transform(int value)
    {
        StaticCalls++;
        return value + 10;
    }

    /// <summary>Returns a generic value and records original execution.</summary>
    internal static T Identity<T>(T value)
        where T : notnull
    {
        StaticCalls++;
        return value;
    }

    /// <summary>Restores deterministic original counters and storage.</summary>
    internal static void Reset()
    {
        StaticCalls = 0;
        ConstructorCalls = 0;
        staticPropertyValue = 0;
        StaticField = 0;
    }
}

public delegate int ProfiledReceiverFreeInt32Unary(int value);
public delegate string ProfiledReceiverFreeStringUnary(string value);
public delegate void ProfiledReceiverFreeInt32Write(int value);
public delegate int ProfiledReceiverFreeInt32Read();
public delegate ProfiledReceiverFreeTarget ProfiledReceiverFreeConstruction(
    int value);
public delegate int ProfiledReceiverFreeInstanceInt32Read(
    ProfiledReceiverFreeTarget target);
public delegate void ProfiledReceiverFreeInstanceInt32Write(
    ProfiledReceiverFreeTarget target,
    int value);
public delegate string? ProfiledReceiverFreeInstanceStringRead(
    ProfiledReceiverFreeTarget target);
public delegate void ProfiledReceiverFreeInstanceStringWrite(
    ProfiledReceiverFreeTarget target,
    string? value);
