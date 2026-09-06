namespace AlvorKit;

/// <summary>Runs registered shutdown actions in reverse order after the root state and scripts unload.</summary>
[Root]
public class RootUnload
{
    private readonly List<Action> actions = [];

    /// <summary>Registers an action to run during root shutdown.</summary>
    public void Add(Action action) => actions.Add(action);

    /// <summary>Runs pending actions in reverse registration order and clears them.</summary>
    public void Run()
    {
        for (var i = actions.Count - 1; i >= 0; i--)
            actions[i]();

        actions.Clear();
    }
}
