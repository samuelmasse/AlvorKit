namespace AlvorKit.Mocking;

internal ref partial struct Rewire<
    T0, T1, T2, T3, T4, T5, T6, T7,
    T8, T9, T10, T11, T12, T13, T14, T15,
    RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
    RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
    ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
    ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15>
{
    /// <summary>Harmony prefix for void methods.</summary>
    internal static bool Void(MethodInfo method, object instance,
        Rewire<T0, T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12, T13, T14, T15,
            RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
            RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
            ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
            ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15> args)
    {
        var mocked = Mock.GetMocked(instance);
        if (mocked == null)
            return true;

        if (Capture.Context.IsDisambiguating)
        {
            WriteFingerprints(ref args);
            Capture.WriteDisambiguate(instance, method);
            return false;
        }

        var callArgs = BuildArgs(ref args);
        if (Rewire.Method(method, instance, mocked, callArgs, out _))
        {
            if (!Capture.Context.IsActive)
                WriteBackRefArgs(callArgs, ref args);
            else WriteFingerprints(ref args);

            return false;
        }

        return true;
    }

    /// <summary>Harmony prefix for methods that return a value or reference.</summary>
    internal static bool Method<R>(MethodInfo method, object instance, ref R rval,
        Rewire<T0, T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12, T13, T14, T15,
            RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
            RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
            ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
            ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15> args)
    {
        var mocked = Mock.GetMocked(instance);
        if (mocked == null)
            return true;

        if (Capture.Context.IsDisambiguating)
        {
            WriteReturnValue(ReturnValues.GetDefault(mocked, method), ref rval);
            WriteFingerprints(ref args);
            Capture.WriteDisambiguate(instance, method);
            return false;
        }

        var callArgs = BuildArgs(ref args);
        if (Rewire.Method(method, instance, mocked, callArgs, out var robj))
        {
            if (!Capture.Context.IsActive)
                WriteBackRefArgs(callArgs, ref args);
            else WriteFingerprints(ref args);

            WriteReturnValue(robj, ref rval);
            return false;
        }

        WriteReturnValue(robj, ref rval);
        return true;
    }

    /// <summary>Copies an object return value into the strongly typed Harmony result slot.</summary>
    private static void WriteReturnValue<R>(object? value, ref R result)
    {
        if (value == null)
            result = default!;
        else result = (R)value;
    }
}
