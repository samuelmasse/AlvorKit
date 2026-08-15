namespace AlvorKit;

/// <summary>Sealed target for the 48-parameter concrete behavior row.</summary>
public sealed class ProfiledWideTarget
{
    /// <summary>Gets the number of original wide bodies entered.</summary>
    public int OriginalCalls { get; private set; }

    /// <summary>Consumes sixteen value, ref, and span triplets in declared order.</summary>
    public int Wide(
        int v0, ref int r0, Span<int> s0,
        int v1, ref int r1, Span<int> s1,
        int v2, ref int r2, Span<int> s2,
        int v3, ref int r3, Span<int> s3,
        int v4, ref int r4, Span<int> s4,
        int v5, ref int r5, Span<int> s5,
        int v6, ref int r6, Span<int> s6,
        int v7, ref int r7, Span<int> s7,
        int v8, ref int r8, Span<int> s8,
        int v9, ref int r9, Span<int> s9,
        int v10, ref int r10, Span<int> s10,
        int v11, ref int r11, Span<int> s11,
        int v12, ref int r12, Span<int> s12,
        int v13, ref int r13, Span<int> s13,
        int v14, ref int r14, Span<int> s14,
        int v15, ref int r15, Span<int> s15)
    {
        OriginalCalls++;
        return v0 + r0 + s0.Length +
            v1 + r1 + s1.Length +
            v2 + r2 + s2.Length +
            v3 + r3 + s3.Length +
            v4 + r4 + s4.Length +
            v5 + r5 + s5.Length +
            v6 + r6 + s6.Length +
            v7 + r7 + s7.Length +
            v8 + r8 + s8.Length +
            v9 + r9 + s9.Length +
            v10 + r10 + s10.Length +
            v11 + r11 + s11.Length +
            v12 + r12 + s12.Length +
            v13 + r13 + s13.Length +
            v14 + r14 + s14.Length +
            v15 + r15 + s15.Length;
    }
}

/// <summary>Exact delegate for the 48-parameter concrete operation.</summary>
public delegate int ProfiledWideOperation(
    ProfiledWideTarget target,
    int v0, ref int r0, Span<int> s0,
    int v1, ref int r1, Span<int> s1,
    int v2, ref int r2, Span<int> s2,
    int v3, ref int r3, Span<int> s3,
    int v4, ref int r4, Span<int> s4,
    int v5, ref int r5, Span<int> s5,
    int v6, ref int r6, Span<int> s6,
    int v7, ref int r7, Span<int> s7,
    int v8, ref int r8, Span<int> s8,
    int v9, ref int r9, Span<int> s9,
    int v10, ref int r10, Span<int> s10,
    int v11, ref int r11, Span<int> s11,
    int v12, ref int r12, Span<int> s12,
    int v13, ref int r13, Span<int> s13,
    int v14, ref int r14, Span<int> s14,
    int v15, ref int r15, Span<int> s15);

/// <summary>Preserves the untouched 48-parameter operation for fallback.</summary>
internal static class ProfiledWideOriginal
{
    /// <summary>Invokes the untouched wide operation with exact declared positions.</summary>
    internal static int Invoke(
        ProfiledWideTarget target,
        int v0, ref int r0, Span<int> s0,
        int v1, ref int r1, Span<int> s1,
        int v2, ref int r2, Span<int> s2,
        int v3, ref int r3, Span<int> s3,
        int v4, ref int r4, Span<int> s4,
        int v5, ref int r5, Span<int> s5,
        int v6, ref int r6, Span<int> s6,
        int v7, ref int r7, Span<int> s7,
        int v8, ref int r8, Span<int> s8,
        int v9, ref int r9, Span<int> s9,
        int v10, ref int r10, Span<int> s10,
        int v11, ref int r11, Span<int> s11,
        int v12, ref int r12, Span<int> s12,
        int v13, ref int r13, Span<int> s13,
        int v14, ref int r14, Span<int> s14,
        int v15, ref int r15, Span<int> s15) =>
        target.Wide(
            v0, ref r0, s0, v1, ref r1, s1,
            v2, ref r2, s2, v3, ref r3, s3,
            v4, ref r4, s4, v5, ref r5, s5,
            v6, ref r6, s6, v7, ref r7, s7,
            v8, ref r8, s8, v9, ref r9, s9,
            v10, ref r10, s10, v11, ref r11, s11,
            v12, ref r12, s12, v13, ref r13, s13,
            v14, ref r14, s14, v15, ref r15, s15);
}

/// <summary>Exposes the exact wide Mocking wrapper as a profiler handler.</summary>
public sealed class ProfiledWideHandler(ProfiledWideOperation wrapper)
{
    /// <summary>Invokes the bound wide Mocking wrapper.</summary>
    public int Invoke(
        ProfiledWideTarget target,
        int v0, ref int r0, Span<int> s0,
        int v1, ref int r1, Span<int> s1,
        int v2, ref int r2, Span<int> s2,
        int v3, ref int r3, Span<int> s3,
        int v4, ref int r4, Span<int> s4,
        int v5, ref int r5, Span<int> s5,
        int v6, ref int r6, Span<int> s6,
        int v7, ref int r7, Span<int> s7,
        int v8, ref int r8, Span<int> s8,
        int v9, ref int r9, Span<int> s9,
        int v10, ref int r10, Span<int> s10,
        int v11, ref int r11, Span<int> s11,
        int v12, ref int r12, Span<int> s12,
        int v13, ref int r13, Span<int> s13,
        int v14, ref int r14, Span<int> s14,
        int v15, ref int r15, Span<int> s15) =>
        wrapper(
            target,
            v0, ref r0, s0, v1, ref r1, s1,
            v2, ref r2, s2, v3, ref r3, s3,
            v4, ref r4, s4, v5, ref r5, s5,
            v6, ref r6, s6, v7, ref r7, s7,
            v8, ref r8, s8, v9, ref r9, s9,
            v10, ref r10, s10, v11, ref r11, s11,
            v12, ref r12, s12, v13, ref r13, s13,
            v14, ref r14, s14, v15, ref r15, s15);
}
