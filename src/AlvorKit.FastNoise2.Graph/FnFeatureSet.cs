namespace AlvorKit;

/// <summary>Identifies a cumulative FastSIMD feature-set mask understood by FastNoise2 1.1.1.</summary>
/// <remarks>
/// Values describe CPU instruction support, not output quality. Except for <see cref="Maximum"/>, each value includes
/// the lower feature bits required by that implementation. The active value can vary by CPU, architecture, runtime
/// identifier, and native package build.
/// </remarks>
public enum FnFeatureSet : uint
{
    /// <summary>Uses scalar instructions. Native mask: <c>0x00000001</c>.</summary>
    Scalar = 1,

    /// <summary>Uses x86 SSE. Native cumulative mask: <c>0x00000006</c>.</summary>
    Sse = 6,

    /// <summary>Uses x86 SSE2. Native cumulative mask: <c>0x0000000E</c>.</summary>
    Sse2 = 14,

    /// <summary>Uses x86 SSE3. Native cumulative mask: <c>0x0000001E</c>.</summary>
    Sse3 = 30,

    /// <summary>Uses x86 SSSE3. Native cumulative mask: <c>0x0000003E</c>.</summary>
    Ssse3 = 62,

    /// <summary>Uses x86 SSE4.1. Native cumulative mask: <c>0x0000007E</c>.</summary>
    Sse41 = 126,

    /// <summary>Uses x86 SSE4.2. Native cumulative mask: <c>0x000000FE</c>.</summary>
    Sse42 = 254,

    /// <summary>Uses x86 AVX. Native cumulative mask: <c>0x000001FE</c>.</summary>
    Avx = 510,

    /// <summary>Uses x86 AVX2. Native cumulative mask: <c>0x000003FE</c>.</summary>
    Avx2 = 1022,

    /// <summary>Uses x86 AVX-512. Native cumulative mask: <c>0x00003FFE</c>.</summary>
    Avx512 = 16382,

    /// <summary>Uses 32-bit ARM NEON. Native cumulative mask: <c>0x0000C000</c>.</summary>
    Neon = 49152,

    /// <summary>Uses AArch64 SIMD. Native cumulative mask: <c>0x0001C000</c>.</summary>
    Aarch64 = 114688,

    /// <summary>Uses WebAssembly SIMD. Native mask: <c>0x00020000</c>.</summary>
    WasmSimd = 131072,

    /// <summary>Requests the fastest compiled implementation supported by the current CPU.</summary>
    Maximum = uint.MaxValue,
}
