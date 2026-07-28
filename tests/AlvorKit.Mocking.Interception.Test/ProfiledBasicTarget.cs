namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Sealed target for the concrete basic behavior matrix.</summary>
public sealed class ProfiledBasicTarget
{
    private EventHandler? changed;
    private int number;

    /// <summary>Gets the number of original method and property bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Gets the number of original event-accessor bodies entered.</summary>
    public int EventAccessorCalls { get; private set; }

    /// <summary>Adds two values in the original implementation.</summary>
    public int Add(int left, int right)
    {
        OriginalCalls++;
        return left + right;
    }

    /// <summary>Mutates reference and output arguments in the original implementation.</summary>
    public int Mutate(ref int value, out int doubled)
    {
        OriginalCalls++;
        value += 3;
        doubled = value * 2;
        return value;
    }

    /// <summary>Gets or sets the original property value.</summary>
    public int Number
    {
        get
        {
            OriginalCalls++;
            return number;
        }
        set
        {
            OriginalCalls++;
            number = value;
        }
    }

    /// <summary>Provides an event with observable original accessors.</summary>
    internal event EventHandler? Changed
    {
        add
        {
            EventAccessorCalls++;
            changed += value;
        }
        remove
        {
            EventAccessorCalls++;
            changed -= value;
        }
    }
}

/// <summary>Exact delegate for concrete addition.</summary>
public delegate int ProfiledBasicAddOperation(
    ProfiledBasicTarget target,
    int left,
    int right);

/// <summary>Exact delegate for the concrete property getter.</summary>
public delegate int ProfiledBasicGetNumberOperation(
    ProfiledBasicTarget target);

/// <summary>Exact delegate for the concrete property setter.</summary>
public delegate void ProfiledBasicSetNumberOperation(
    ProfiledBasicTarget target,
    int value);

/// <summary>Exact delegate for the concrete ref/out operation.</summary>
public delegate int ProfiledBasicMutateOperation(
    ProfiledBasicTarget target,
    ref int value,
    out int doubled);

/// <summary>Exact delegate for a concrete event accessor.</summary>
public delegate void ProfiledBasicEventOperation(
    ProfiledBasicTarget target,
    EventHandler? handler);
