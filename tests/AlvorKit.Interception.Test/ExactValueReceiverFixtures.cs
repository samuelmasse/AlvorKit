namespace AlvorKit;

/// <summary>Provides mutable caller-owned storage.</summary>
public struct ExactMutableReceiver(int value)
{
    /// <summary>Gets the current live value.</summary>
    public int Value = value;

    /// <summary>Mutates and returns the live value.</summary>
    public int Add(int amount) =>
        Value += amount;

    /// <summary>Reads one readonly declared argument.</summary>
    public readonly int ReadIn(in int amount) =>
        Value + amount;
}

/// <summary>Provides readonly caller-owned storage.</summary>
public readonly struct ExactReadonlyReceiver(int value)
{
    /// <summary>Gets the current immutable value.</summary>
    public int Value { get; } = value;

    /// <summary>Reads the receiver.</summary>
    public int Read(int amount) =>
        Value + amount;
}

/// <summary>Provides readonly record storage.</summary>
public readonly record struct ExactRecordReceiver(int Value)
{
    /// <summary>Reads the receiver.</summary>
    public int Read(int amount) =>
        Value + amount;
}

/// <summary>Defines one constrained receiver operation.</summary>
public interface IExactValueMetric
{
    /// <summary>Mutates and measures the implementing receiver.</summary>
    int Measure(int amount);
}

/// <summary>Implements one constrained interface through live storage.</summary>
public struct ExactConstrainedReceiver(int value) : IExactValueMetric
{
    /// <summary>Gets the current live value.</summary>
    public int Value = value;

    /// <inheritdoc />
    public int Measure(int amount) =>
        Value += amount;
}

/// <summary>Supplies exact handlers for value receiver call shapes.</summary>
public sealed class ExactValueReceiverHandler
{
    /// <summary>Mutates mutable live storage.</summary>
    public int Add(ref ExactMutableReceiver receiver, int amount) =>
        receiver.Value += amount;

    /// <summary>Reads readonly live storage.</summary>
    public int Read(in ExactReadonlyReceiver receiver, int amount) =>
        receiver.Value + amount;

    /// <summary>Reads record live storage.</summary>
    public int ReadRecord(ref ExactRecordReceiver receiver, int amount) =>
        receiver.Value + amount;

    /// <summary>Mutates a concrete constrained receiver.</summary>
    public int Measure(ref ExactConstrainedReceiver receiver, int amount) =>
        receiver.Value += amount;

    /// <summary>Preserves a readonly declared argument.</summary>
    public int ReadIn(
        ref ExactMutableReceiver receiver,
        in int amount) =>
        receiver.Value + amount;

    /// <summary>Incorrectly changes a readonly declared argument to writable.</summary>
    public int ReadWritable(
        ref ExactMutableReceiver receiver,
        ref int amount) =>
        receiver.Value + amount;

    /// <summary>Incorrectly changes the declared return type.</summary>
    public long BadReturn(
        ref ExactMutableReceiver receiver,
        int amount) =>
        receiver.Value + amount;
}

/// <summary>Provides an intentionally open receiver shape.</summary>
public struct ExactOpenReceiver<T>
{
    /// <summary>Reads one default value.</summary>
    public readonly int Read() => 0;
}

/// <summary>Provides a nested construction that permits ref-struct arguments.</summary>
public struct ExactNestedReceiver<T>
    where T : allows ref struct
{
    /// <summary>Reads one default value without storing the construction argument.</summary>
    public readonly int Read() => 0;
}

/// <summary>Provides an unsupported varargs signature.</summary>
public static class ExactVarArgsTarget
{
    /// <summary>Declares one runtime varargs operation.</summary>
    public static void Observe(__arglist)
    {
    }

    /// <summary>Provides a deliberately irrelevant handler.</summary>
    public static void Handle()
    {
    }
}
