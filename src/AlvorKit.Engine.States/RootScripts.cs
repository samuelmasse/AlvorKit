namespace AlvorKit;

/// <summary>Owns script registration and ordered callback dispatch on the game-loop thread.</summary>
[Root]
public class RootScripts
{
    /// <summary>Currently registered scripts in execution order, including additions made by callbacks.</summary>
    private readonly List<Script> scripts = [];

    /// <summary>Reusable pass order. Removing a script clears its slot without moving the remaining callbacks.</summary>
    private readonly List<Script?> pass = [];

    /// <summary>Tracks nested dispatch so an active pass's slots stay in place until its callbacks return.</summary>
    private int dispatchDepth;

    /// <summary>Indicates that the next outermost pass needs the latest registration order.</summary>
    private bool orderChanged;

    /// <summary>Gets the current registration order for inspection. Adding or removing a script invalidates the span.</summary>
    public ReadOnlySpan<Script> Span => CollectionsMarshal.AsSpan(scripts);

    /// <summary>Registers and loads a script immediately. During dispatch it joins the next outermost pass.</summary>
    public void Add(Script script)
    {
        scripts.Add(script);
        scripts.Sort((a, b) => a.Order.CompareTo(b.Order));
        orderChanged = true;
        script.Load();
    }

    /// <summary>Removes and unloads one registration of this instance. Active passes skip its slot immediately.</summary>
    public void Remove(Script script)
    {
        int index = Span.IndexOf(script, ReferenceEqualityComparer.Instance);
        if (index < 0)
            return;

        scripts.RemoveAt(index);
        var slots = CollectionsMarshal.AsSpan(pass);
        int slot = slots.IndexOf(script, ReferenceEqualityComparer.Instance);
        if (slot >= 0)
            slots[slot] = null;

        orderChanged = true;
        script.Unload();
    }

    /// <summary>Visits scripts in execution order, skipping registrations removed before their turn.</summary>
    public void Dispatch(Action<Script> visit) => Dispatch(visit, static (script, callback) => callback(script));

    /// <summary>Visits scripts with caller-supplied context so frame callbacks can run without a closure.</summary>
    public void Dispatch<T>(T context, Action<Script, T> visit)
    {
        if (dispatchDepth == 0 && orderChanged)
        {
            pass.Clear();
            pass.AddRange(scripts);
            orderChanged = false;
        }

        dispatchDepth++;
        try
        {
            foreach (var script in CollectionsMarshal.AsSpan(pass))
            {
                if (script is not null)
                    visit(script, context);
            }
        }
        finally
        {
            dispatchDepth--;
        }
    }
}
