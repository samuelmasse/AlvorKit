namespace AlvorKit.LiveCode;

/// <summary>Contract implemented by a submitted command that is constructed through a selected injector scope.</summary>
public interface ILiveCodeCommand
{
    /// <summary>Runs synchronously at a safe point on the game thread and records structured output.</summary>
    void Run(LiveCodeContext output);
}
