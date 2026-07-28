/// <summary>Ordinary unannotated game methods selected by exact MVID, token, and signature hash.</summary>
public static class LivePatchTarget
{
    /// <summary>Returns the visual mode consumed directly by the render loop.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int SceneMode() => 0;

    /// <summary>An aggressively inlineable method used to prove existing callers are repaired.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int InlineMode() => 10;

    /// <summary>Calls <see cref="InlineMode"/> after it has been warmed and normally inlined.</summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ReadInlineMode() => InlineMode() + 1;
}
