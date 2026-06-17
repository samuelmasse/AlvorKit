namespace AlvorKit.Mocking;

internal ref partial struct Rewire<
    T0, T1, T2, T3, T4, T5, T6, T7,
    T8, T9, T10, T11, T12, T13, T14, T15,
    RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
    RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
    ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
    ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15>
{
    /// <summary>Writes FNV-1a fingerprints for matcher disambiguation without boxing by-ref-like arguments.</summary>
    private unsafe static void WriteFingerprints(
        ref Rewire<T0, T1, T2, T3, T4, T5, T6, T7,
            T8, T9, T10, T11, T12, T13, T14, T15,
            RT0, RT1, RT2, RT3, RT4, RT5, RT6, RT7,
            RT8, RT9, RT10, RT11, RT12, RT13, RT14, RT15,
            ST0, ST1, ST2, ST3, ST4, ST5, ST6, ST7,
            ST8, ST9, ST10, ST11, ST12, ST13, ST14, ST15> args)
    {
        var fingerprints = Capture.Context.IsDisambiguating ? Capture.SecondFingerprints : Capture.FirstFingerprints;
        int index = 0;

        if (typeof(T0) != typeof(RewireEmpty)) Fingerprint(ref args.V0);
        if (typeof(T1) != typeof(RewireEmpty)) Fingerprint(ref args.V1);
        if (typeof(T2) != typeof(RewireEmpty)) Fingerprint(ref args.V2);
        if (typeof(T3) != typeof(RewireEmpty)) Fingerprint(ref args.V3);
        if (typeof(T4) != typeof(RewireEmpty)) Fingerprint(ref args.V4);
        if (typeof(T5) != typeof(RewireEmpty)) Fingerprint(ref args.V5);
        if (typeof(T6) != typeof(RewireEmpty)) Fingerprint(ref args.V6);
        if (typeof(T7) != typeof(RewireEmpty)) Fingerprint(ref args.V7);
        if (typeof(T8) != typeof(RewireEmpty)) Fingerprint(ref args.V8);
        if (typeof(T9) != typeof(RewireEmpty)) Fingerprint(ref args.V9);
        if (typeof(T10) != typeof(RewireEmpty)) Fingerprint(ref args.V10);
        if (typeof(T11) != typeof(RewireEmpty)) Fingerprint(ref args.V11);
        if (typeof(T12) != typeof(RewireEmpty)) Fingerprint(ref args.V12);
        if (typeof(T13) != typeof(RewireEmpty)) Fingerprint(ref args.V13);
        if (typeof(T14) != typeof(RewireEmpty)) Fingerprint(ref args.V14);
        if (typeof(T15) != typeof(RewireEmpty)) Fingerprint(ref args.V15);

        if (typeof(RT0) != typeof(RewireEmpty)) Fingerprint(ref args.R0);
        if (typeof(RT1) != typeof(RewireEmpty)) Fingerprint(ref args.R1);
        if (typeof(RT2) != typeof(RewireEmpty)) Fingerprint(ref args.R2);
        if (typeof(RT3) != typeof(RewireEmpty)) Fingerprint(ref args.R3);
        if (typeof(RT4) != typeof(RewireEmpty)) Fingerprint(ref args.R4);
        if (typeof(RT5) != typeof(RewireEmpty)) Fingerprint(ref args.R5);
        if (typeof(RT6) != typeof(RewireEmpty)) Fingerprint(ref args.R6);
        if (typeof(RT7) != typeof(RewireEmpty)) Fingerprint(ref args.R7);
        if (typeof(RT8) != typeof(RewireEmpty)) Fingerprint(ref args.R8);
        if (typeof(RT9) != typeof(RewireEmpty)) Fingerprint(ref args.R9);
        if (typeof(RT10) != typeof(RewireEmpty)) Fingerprint(ref args.R10);
        if (typeof(RT11) != typeof(RewireEmpty)) Fingerprint(ref args.R11);
        if (typeof(RT12) != typeof(RewireEmpty)) Fingerprint(ref args.R12);
        if (typeof(RT13) != typeof(RewireEmpty)) Fingerprint(ref args.R13);
        if (typeof(RT14) != typeof(RewireEmpty)) Fingerprint(ref args.R14);
        if (typeof(RT15) != typeof(RewireEmpty)) Fingerprint(ref args.R15);

        if (typeof(ST0) != typeof(RewireEmpty)) Fingerprint(ref args.S0);
        if (typeof(ST1) != typeof(RewireEmpty)) Fingerprint(ref args.S1);
        if (typeof(ST2) != typeof(RewireEmpty)) Fingerprint(ref args.S2);
        if (typeof(ST3) != typeof(RewireEmpty)) Fingerprint(ref args.S3);
        if (typeof(ST4) != typeof(RewireEmpty)) Fingerprint(ref args.S4);
        if (typeof(ST5) != typeof(RewireEmpty)) Fingerprint(ref args.S5);
        if (typeof(ST6) != typeof(RewireEmpty)) Fingerprint(ref args.S6);
        if (typeof(ST7) != typeof(RewireEmpty)) Fingerprint(ref args.S7);
        if (typeof(ST8) != typeof(RewireEmpty)) Fingerprint(ref args.S8);
        if (typeof(ST9) != typeof(RewireEmpty)) Fingerprint(ref args.S9);
        if (typeof(ST10) != typeof(RewireEmpty)) Fingerprint(ref args.S10);
        if (typeof(ST11) != typeof(RewireEmpty)) Fingerprint(ref args.S11);
        if (typeof(ST12) != typeof(RewireEmpty)) Fingerprint(ref args.S12);
        if (typeof(ST13) != typeof(RewireEmpty)) Fingerprint(ref args.S13);
        if (typeof(ST14) != typeof(RewireEmpty)) Fingerprint(ref args.S14);
        if (typeof(ST15) != typeof(RewireEmpty)) Fingerprint(ref args.S15);

        void Fingerprint<T>(ref T val) where T : allows ref struct
        {
            void* ptr = Unsafe.AsPointer(ref val);
            int size = typeof(T).IsValueType ? Unsafe.SizeOf<T>() : sizeof(IntPtr);

            ulong hash = 0xcbf29ce484222325;

            for (int i = 0; i < size; i++)
            {
                hash ^= ((byte*)ptr)[i];
                hash *= 0x100000001b3;
            }

            fingerprints[index++] = hash;
        }
    }
}
