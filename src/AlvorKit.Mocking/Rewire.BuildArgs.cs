namespace AlvorKit.Mocking;

internal ref partial struct Rewire<
    T0, T1, T2, T3, T4, T5, T6, T7,
    T8, T9, T10, T11, T12, T13, T14, T15,
    RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
    RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
    ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
    ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15>
{
    /// <summary>Boxes intercepted arguments into the original method's logical argument order.</summary>
    private static object?[] BuildArgs(
        ref Rewire<T0, T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12, T13, T14, T15,
            RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
            RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
            ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
            ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15> args)
    {
        var buffer = argBuffer.Value!;
        int index = 0;

        void Add<T>(T value)
        {
            if (typeof(T) != typeof(RewireEmpty))
                buffer[index++] = value;
        }

        void AddEmpty() => Add<object?>(null);

        if (typeof(T0) != typeof(RewireEmpty)) Add(args.V0);
        if (typeof(T1) != typeof(RewireEmpty)) Add(args.V1);
        if (typeof(T2) != typeof(RewireEmpty)) Add(args.V2);
        if (typeof(T3) != typeof(RewireEmpty)) Add(args.V3);
        if (typeof(T4) != typeof(RewireEmpty)) Add(args.V4);
        if (typeof(T5) != typeof(RewireEmpty)) Add(args.V5);
        if (typeof(T6) != typeof(RewireEmpty)) Add(args.V6);
        if (typeof(T7) != typeof(RewireEmpty)) Add(args.V7);
        if (typeof(T8) != typeof(RewireEmpty)) Add(args.V8);
        if (typeof(T9) != typeof(RewireEmpty)) Add(args.V9);
        if (typeof(T10) != typeof(RewireEmpty)) Add(args.V10);
        if (typeof(T11) != typeof(RewireEmpty)) Add(args.V11);
        if (typeof(T12) != typeof(RewireEmpty)) Add(args.V12);
        if (typeof(T13) != typeof(RewireEmpty)) Add(args.V13);
        if (typeof(T14) != typeof(RewireEmpty)) Add(args.V14);
        if (typeof(T15) != typeof(RewireEmpty)) Add(args.V15);

        if (typeof(RT0) != typeof(RewireEmpty)) Add(args.R0);
        if (typeof(RT1) != typeof(RewireEmpty)) Add(args.R1);
        if (typeof(RT2) != typeof(RewireEmpty)) Add(args.R2);
        if (typeof(RT3) != typeof(RewireEmpty)) Add(args.R3);
        if (typeof(RT4) != typeof(RewireEmpty)) Add(args.R4);
        if (typeof(RT5) != typeof(RewireEmpty)) Add(args.R5);
        if (typeof(RT6) != typeof(RewireEmpty)) Add(args.R6);
        if (typeof(RT7) != typeof(RewireEmpty)) Add(args.R7);
        if (typeof(RT8) != typeof(RewireEmpty)) Add(args.R8);
        if (typeof(RT9) != typeof(RewireEmpty)) Add(args.R9);
        if (typeof(RT10) != typeof(RewireEmpty)) Add(args.R10);
        if (typeof(RT11) != typeof(RewireEmpty)) Add(args.R11);
        if (typeof(RT12) != typeof(RewireEmpty)) Add(args.R12);
        if (typeof(RT13) != typeof(RewireEmpty)) Add(args.R13);
        if (typeof(RT14) != typeof(RewireEmpty)) Add(args.R14);
        if (typeof(RT15) != typeof(RewireEmpty)) Add(args.R15);

        if (typeof(ST0) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST1) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST2) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST3) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST4) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST5) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST6) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST7) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST8) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST9) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST10) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST11) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST12) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST13) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST14) != typeof(RewireEmpty)) AddEmpty();
        if (typeof(ST15) != typeof(RewireEmpty)) AddEmpty();

        return buffer.AsSpan(0, index).ToArray();
    }
}
