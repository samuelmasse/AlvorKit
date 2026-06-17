namespace AlvorKit.Mocking;

internal ref partial struct Rewire<
    T0, T1, T2, T3, T4, T5, T6, T7,
    T8, T9, T10, T11, T12, T13, T14, T15,
    RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
    RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
    ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
    ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15>
{
    /// <summary>Copies configured ref and out values back into intercepted by-reference arguments.</summary>
    private static void WriteBackRefArgs(
        object?[] callArgs,
        ref Rewire<T0, T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12, T13, T14, T15,
            RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
            RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
            ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
            ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15> args)
    {
        int offset = 0;

        if (typeof(T0) != typeof(RewireEmpty)) offset++;
        if (typeof(T1) != typeof(RewireEmpty)) offset++;
        if (typeof(T2) != typeof(RewireEmpty)) offset++;
        if (typeof(T3) != typeof(RewireEmpty)) offset++;
        if (typeof(T4) != typeof(RewireEmpty)) offset++;
        if (typeof(T5) != typeof(RewireEmpty)) offset++;
        if (typeof(T6) != typeof(RewireEmpty)) offset++;
        if (typeof(T7) != typeof(RewireEmpty)) offset++;
        if (typeof(T8) != typeof(RewireEmpty)) offset++;
        if (typeof(T9) != typeof(RewireEmpty)) offset++;
        if (typeof(T10) != typeof(RewireEmpty)) offset++;
        if (typeof(T11) != typeof(RewireEmpty)) offset++;
        if (typeof(T12) != typeof(RewireEmpty)) offset++;
        if (typeof(T13) != typeof(RewireEmpty)) offset++;
        if (typeof(T14) != typeof(RewireEmpty)) offset++;
        if (typeof(T15) != typeof(RewireEmpty)) offset++;

        int i = offset;

        if (typeof(RT0) != typeof(RewireEmpty)) args.R0 = (RT0)callArgs[i++]!;
        if (typeof(RT1) != typeof(RewireEmpty)) args.R1 = (RT1)callArgs[i++]!;
        if (typeof(RT2) != typeof(RewireEmpty)) args.R2 = (RT2)callArgs[i++]!;
        if (typeof(RT3) != typeof(RewireEmpty)) args.R3 = (RT3)callArgs[i++]!;
        if (typeof(RT4) != typeof(RewireEmpty)) args.R4 = (RT4)callArgs[i++]!;
        if (typeof(RT5) != typeof(RewireEmpty)) args.R5 = (RT5)callArgs[i++]!;
        if (typeof(RT6) != typeof(RewireEmpty)) args.R6 = (RT6)callArgs[i++]!;
        if (typeof(RT7) != typeof(RewireEmpty)) args.R7 = (RT7)callArgs[i++]!;
        if (typeof(RT8) != typeof(RewireEmpty)) args.R8 = (RT8)callArgs[i++]!;
        if (typeof(RT9) != typeof(RewireEmpty)) args.R9 = (RT9)callArgs[i++]!;
        if (typeof(RT10) != typeof(RewireEmpty)) args.R10 = (RT10)callArgs[i++]!;
        if (typeof(RT11) != typeof(RewireEmpty)) args.R11 = (RT11)callArgs[i++]!;
        if (typeof(RT12) != typeof(RewireEmpty)) args.R12 = (RT12)callArgs[i++]!;
        if (typeof(RT13) != typeof(RewireEmpty)) args.R13 = (RT13)callArgs[i++]!;
        if (typeof(RT14) != typeof(RewireEmpty)) args.R14 = (RT14)callArgs[i++]!;
        if (typeof(RT15) != typeof(RewireEmpty)) args.R15 = (RT15)callArgs[i++]!;
    }
}
