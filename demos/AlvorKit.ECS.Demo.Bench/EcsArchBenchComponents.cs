namespace AlvorKit.ECS.Demo.Bench;

internal sealed class WarmArch;
internal sealed class RunArch;

internal readonly struct F00;
internal readonly struct F01;
internal readonly struct F02;
internal readonly struct F03;
internal readonly struct F04;
internal readonly struct F05;
internal readonly struct F06;
internal readonly struct F07;
internal readonly struct F08;
internal readonly struct F09;
internal readonly struct F10;
internal readonly struct F11;
internal readonly struct F12;
internal readonly struct F13;
internal readonly struct F14;
internal readonly struct F15;
internal readonly struct F16;
internal readonly struct F17;
internal readonly struct F18;
internal readonly struct F19;
internal readonly struct F20;
internal readonly struct F21;
internal readonly struct F22;
internal readonly struct F23;
internal readonly struct F24;
internal readonly struct F25;
internal readonly struct F26;
internal readonly struct F27;
internal readonly struct F28;
internal readonly struct F29;
internal readonly struct F30;
internal readonly struct F31;
internal readonly struct F32;
internal readonly struct FToggle;
internal readonly struct FWide;
internal readonly struct FReference;
internal readonly struct FRefStruct;

/// <summary>Provides a larger reference-free value shape for point access measurements.</summary>
public readonly record struct EcsBenchWideValue(long A, long B, long C, long D, long E, long F, long G, long H);

/// <summary>Provides a stable reference value reused without allocation by timed loops.</summary>
public sealed class EcsBenchReference(int value)
{
    public int Value { get; } = value;
}

/// <summary>Provides a value type whose managed layout contains references.</summary>
public readonly record struct EcsBenchRefStruct(string Text, object Token);
