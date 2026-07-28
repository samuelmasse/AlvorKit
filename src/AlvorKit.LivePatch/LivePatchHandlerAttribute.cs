namespace AlvorKit.LivePatch;

/// <summary>Marks the single exact-signature handler method in a submitted LivePatch class.</summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class LivePatchHandlerAttribute : Attribute;
