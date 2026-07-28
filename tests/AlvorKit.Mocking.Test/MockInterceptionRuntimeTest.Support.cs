namespace AlvorKit.Mocking.Test;

internal delegate int InterceptionIntCall(
    InterceptionRuntimeTarget target,
    int value);

internal delegate void InterceptionThrowCall(InterceptionRuntimeTarget target);

internal delegate int InterceptionRefOutCall(
    InterceptionRuntimeTarget target,
    ref int value,
    out int doubled);

internal delegate int InterceptionSpanCall(
    InterceptionRuntimeTarget target,
    ReadOnlySpan<int> values);

internal delegate ReadOnlySpan<int> InterceptionSpanReturnCall(
    InterceptionRuntimeTarget target);

internal delegate ref int InterceptionRefReturnCall(
    InterceptionRuntimeTarget target);

internal delegate ref readonly int InterceptionRefReadonlyReturnCall(
    InterceptionRuntimeTarget target);

internal delegate T InterceptionGenericCall<T>(
    InterceptionRuntimeTarget target,
    T value);

internal delegate void InterceptionConstructorBodyCall(
    InterceptionConstructorBodyTarget target,
    int value);

internal delegate void InterceptionConstructorSpanBodyCall(
    InterceptionConstructorBodyTarget target,
    ReadOnlySpan<int> values);

internal sealed class InterceptionConstructorBodyTarget
{
    internal int ObservedArgument;
    internal bool ReplacementRan;
    internal int Remainders;
    internal int Value;

    public InterceptionConstructorBodyTarget()
    {
    }

    public InterceptionConstructorBodyTarget(int value) =>
        ApplyRemainder(value);

    public InterceptionConstructorBodyTarget(ReadOnlySpan<int> values) =>
        ApplyRemainder(values);

    internal void ApplyRemainder(int value)
    {
        Remainders++;
        if (value < 0)
        {
            throw new InvalidOperationException(
                $"constructor remainder {value}");
        }

        Value = value;
    }

    internal void ApplyRemainder(ReadOnlySpan<int> values)
    {
        Remainders++;
        foreach (int value in values)
            Value += value;
    }
}

internal sealed class InterceptionRuntimeTarget
{
    private readonly Exception failure;
    private readonly int[] values;

    internal InterceptionRuntimeTarget(
        Exception? failure = null,
        int[]? values = null)
    {
        this.failure = failure ??
            new InvalidOperationException("original");
        this.values = values ?? [3, 5];
    }

    internal int Calls;

    public static int LastValue;

    public int Add(int value)
    {
        Calls++;
        return value + 10;
    }

    public void Throw()
    {
        Calls++;
        throw failure;
    }

    public int Mutate(
        ref int value,
        out int doubled)
    {
        Calls++;
        value += 3;
        doubled = value * 2;
        return value;
    }

    public int Sum(ReadOnlySpan<int> input)
    {
        Calls++;
        return input.ToArray().Sum();
    }

    public ReadOnlySpan<int> View()
    {
        Calls++;
        return values;
    }

    public ref int Mutable()
    {
        Calls++;
        return ref values[0];
    }

    public ref readonly int ReadOnly()
    {
        Calls++;
        return ref values[1];
    }

    public T Echo<T>(T value)
    {
        Calls++;
        return value;
    }

    public static int StaticDouble(int value) => value * 2;
}

internal static class InterceptionRuntimeOriginal
{
    internal static int Add(
        InterceptionRuntimeTarget target,
        int value) =>
        target.Add(value);

    internal static void Throw(InterceptionRuntimeTarget target) =>
        target.Throw();

    internal static int Mutate(
        InterceptionRuntimeTarget target,
        ref int value,
        out int doubled) =>
        target.Mutate(ref value, out doubled);

    internal static int Sum(
        InterceptionRuntimeTarget target,
        ReadOnlySpan<int> values) =>
        target.Sum(values);

    internal static ReadOnlySpan<int> View(
        InterceptionRuntimeTarget target) =>
        target.View();

    internal static ref int Mutable(
        InterceptionRuntimeTarget target) =>
        ref target.Mutable();

    internal static ref readonly int ReadOnly(
        InterceptionRuntimeTarget target) =>
        ref target.ReadOnly();

    internal static T Echo<T>(
        InterceptionRuntimeTarget target,
        T value) =>
        target.Echo(value);
}

internal sealed class InterceptionAliasOwner(int[] values)
{
    internal ref int Mutable() => ref values[0];

    internal ref readonly int ReadOnly() => ref values[1];
}

internal sealed class InterceptionLateRegistrationTarget
{
    public int Increment(int value) => value + 1;
}
