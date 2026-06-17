namespace AlvorKit.Mocking;

/// <summary>Generic argument carrier used by dynamic Harmony prefix methods.</summary>
internal ref partial struct Rewire<
    T0, T1, T2, T3, T4, T5, T6, T7,
    T8, T9, T10, T11, T12, T13, T14, T15,
    RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
    RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
    ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
    ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15>
    where ST0 : allows ref struct
    where ST1 : allows ref struct
    where ST2 : allows ref struct
    where ST3 : allows ref struct
    where ST4 : allows ref struct
    where ST5 : allows ref struct
    where ST6 : allows ref struct
    where ST7 : allows ref struct
    where ST8 : allows ref struct
    where ST9 : allows ref struct
    where ST10 : allows ref struct
    where ST11 : allows ref struct
    where ST12 : allows ref struct
    where ST13 : allows ref struct
    where ST14 : allows ref struct
    where ST15 : allows ref struct
{
    public T0 V0;
    public T1 V1;
    public T2 V2;
    public T3 V3;
    public T4 V4;
    public T5 V5;
    public T6 V6;
    public T7 V7;
    public T8 V8;
    public T9 V9;
    public T10 V10;
    public T11 V11;
    public T12 V12;
    public T13 V13;
    public T14 V14;
    public T15 V15;

    public ref RT0 R0;
    public ref RT1 R1;
    public ref RT2 R2;
    public ref RT3 R3;
    public ref RT4 R4;
    public ref RT5 R5;
    public ref RT6 R6;
    public ref RT7 R7;
    public ref RT8 R8;
    public ref RT9 R9;
    public ref RT10 R10;
    public ref RT11 R11;
    public ref RT12 R12;
    public ref RT13 R13;
    public ref RT14 R14;
    public ref RT15 R15;

    public ST0 S0;
    public ST1 S1;
    public ST2 S2;
    public ST3 S3;
    public ST4 S4;
    public ST5 S5;
    public ST6 S6;
    public ST7 S7;
    public ST8 S8;
    public ST9 S9;
    public ST10 S10;
    public ST11 S11;
    public ST12 S12;
    public ST13 S13;
    public ST14 S14;
    public ST15 S15;

    /// <summary>Per-thread object buffer reused while boxing intercepted arguments.</summary>
    private static readonly ThreadLocal<object?[]> argBuffer = new(() => new object?[64]);
}

/// <summary>Placeholder generic argument used to pad fixed-width rewire carriers.</summary>
internal readonly struct RewireEmpty { }
