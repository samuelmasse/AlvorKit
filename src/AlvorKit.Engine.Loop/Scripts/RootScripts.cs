namespace AlvorKit.Engine.Loop;

/// <summary>Owns root-loop scripts and maintains their priority order.</summary>
[Root]
public sealed class RootScripts
{
    private readonly List<Script> scripts = [];

    /// <summary>Gets the current script list without copying.</summary>
    public ReadOnlySpan<Script> Span => CollectionsMarshal.AsSpan(scripts);

    /// <summary>Adds, loads, and sorts a script.</summary>
    public T Add<T>(T script) where T : Script
    {
        scripts.Add(script);
        script.Load();
        scripts.Sort(static (a, b) => a.Priority.CompareTo(b.Priority));
        return script;
    }

    /// <summary>Unloads and removes a script when it is present.</summary>
    public void Remove(Script script)
    {
        var index = scripts.IndexOf(script);
        if (index < 0)
            return;

        script.Unload();
        scripts.RemoveAt(index);
    }

    /// <summary>Unloads and removes every script from last to first.</summary>
    internal void RemoveAllReverse()
    {
        for (var i = scripts.Count - 1; i >= 0; i--)
        {
            scripts[i].Unload();
            scripts.RemoveAt(i);
        }
    }
}
