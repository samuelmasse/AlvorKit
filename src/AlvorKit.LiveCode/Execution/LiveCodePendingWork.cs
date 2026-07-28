namespace AlvorKit.LiveCode;

/// <summary>Base for one authenticated operation waiting to cross onto the game thread.</summary>
internal abstract class LiveCodePendingWork
{
    /// <summary>Completes work that can no longer reach the game thread.</summary>
    internal abstract void Cancel(string error);
}
