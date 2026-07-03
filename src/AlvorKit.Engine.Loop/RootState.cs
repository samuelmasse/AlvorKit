namespace AlvorKit.Engine.Loop;

/// <summary>Stores the currently active game state for the root loop.</summary>
[Root]
public class RootState
{
    private State current = new();

    /// <summary>Gets or sets the state that receives root loop callbacks.</summary>
    public State Current
    {
        get => current;
        set
        {
            current.Unload();
            current = value;
            current.Load();
        }
    }
}
