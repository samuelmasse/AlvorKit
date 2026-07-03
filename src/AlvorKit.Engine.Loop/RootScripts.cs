namespace AlvorKit.Engine.Loop;

/// <summary>Owns root-loop scripts and maintains their execution order.</summary>
[Root]
public class RootScripts
{
    private readonly List<Script> scripts = [];

    /// <summary>Gets the current script list without copying.</summary>
    public ReadOnlySpan<Script> Span => CollectionsMarshal.AsSpan(scripts);

    /// <summary>Adds and loads a script after sorting by <see cref="Script.Order"/>.</summary>
    public void Add(Script script)
    {
        scripts.Add(script);
        scripts.Sort((a, b) => a.Order.CompareTo(b.Order));
        script.Load();
    }

    /// <summary>Removes and unloads a script.</summary>
    public void Remove(Script script)
    {
        scripts.Remove(script);
        script.Unload();
    }
}
