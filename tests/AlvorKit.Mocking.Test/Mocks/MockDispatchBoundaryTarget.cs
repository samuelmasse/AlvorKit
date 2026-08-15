namespace AlvorKit;

/// <summary>Exposes exact-dispatch signatures beyond the retired grouped-carrier widths.</summary>
public abstract class MockDispatchBoundaryTarget
{
    /// <summary>Accepts more than sixteen ordinary values.</summary>
    public abstract int Ordinary(
        int v0, int v1, int v2, int v3, int v4, int v5,
        int v6, int v7, int v8, int v9, int v10, int v11,
        int v12, int v13, int v14, int v15, int v16);

    /// <summary>Accepts more than sixteen managed references.</summary>
    public abstract void References(
        ref int r0, ref int r1, ref int r2, ref int r3, ref int r4,
        ref int r5, ref int r6, ref int r7, ref int r8, ref int r9,
        ref int r10, ref int r11, ref int r12, ref int r13, ref int r14,
        ref int r15, ref int r16);

    /// <summary>Accepts more than sixteen ref-struct arguments across all direction shapes.</summary>
    public abstract void RefStructs(
        Span<int> s0, Span<int> s1, Span<int> s2, Span<int> s3,
        Span<int> s4, Span<int> s5, Span<int> s6, Span<int> s7,
        Span<int> s8, Span<int> s9, Span<int> s10, Span<int> s11,
        Span<int> s12, Span<int> s13, Span<int> s14, Span<int> s15,
        Span<int> s16,
        in ReadOnlySpan<int> input,
        ref Span<int> reference,
        out Span<int> output);

    /// <summary>
    /// Interleaves more than sixteen ordinary, managed-reference, and
    /// ref-struct arguments with explicit input and output references.
    /// </summary>
    public abstract void Mixed(
        int v0, Span<int> s0, ref int r0, in int input,
        int v1, Span<int> s1, ref int r1,
        int v2, Span<int> s2, ref int r2,
        int v3, Span<int> s3, ref int r3,
        int v4, Span<int> s4, ref int r4,
        int v5, Span<int> s5, ref int r5,
        int v6, Span<int> s6, ref int r6,
        int v7, Span<int> s7, ref int r7,
        int v8, Span<int> s8, ref int r8,
        int v9, Span<int> s9, ref int r9,
        int v10, Span<int> s10, ref int r10,
        int v11, Span<int> s11, ref int r11,
        int v12, Span<int> s12, ref int r12,
        int v13, Span<int> s13, ref int r13,
        int v14, Span<int> s14, ref int r14,
        int v15, Span<int> s15, ref int r15,
        int v16, Span<int> s16, ref int r16,
        out int output);
}
