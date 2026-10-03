namespace AlvorKit;

[Root]
public class RootUiUpdate
{
    internal void Ahead(EntMut root)
    {
        // A callback may request another update on a node visited earlier in the tree.
        while (RunAhead(root))
        {
        }
    }

    internal void Update(EntMut n)
    {
        n.OnUpdateFV.Resolve()?.Invoke();

        foreach (var c in n.NodesR.Span)
            Update(c);
    }

    private bool RunAhead(EntMut n)
    {
        if (n.IsDeletedFV.Resolve() || n.IsDisabledFV.Resolve())
            return false;

        var updated = false;
        var remaining = n.AheadUpdateCountFV.Resolve();

        while (remaining > 0)
        {
            n.AheadUpdateCountFV = remaining - 1;
            n.OnUpdateFV.Resolve()?.Invoke();
            updated = true;

            if (n.IsDeletedFV.Resolve() || n.IsDisabledFV.Resolve())
                return updated;

            remaining = n.AheadUpdateCountFV.Resolve();
        }

        updated |= AheadChildren(n);
        updated |= AheadStack(n);
        return updated;
    }

    private bool AheadChildren(EntMut n)
    {
        var updated = false;
        var index = 0;

        while (index < NodesCount(n))
        {
            var child = Nodes(n)[index];
            updated |= RunAhead(child);

            // Read live storage again: the callback may have added or removed siblings.
            if (index < NodesCount(n) && Nodes(n)[index] == child)
                index++;
        }

        return updated;
    }

    private bool AheadStack(EntMut n)
    {
        var updated = false;
        var index = 0;

        while (index < NodeStackCount(n))
        {
            var entry = NodeStack(n)[index];
            var companion = entry.CompanionFV.Resolve();

            if (companion != default)
                updated |= RunAhead(companion);

            if (index < NodeStackCount(n) && NodeStack(n)[index] == entry)
                index++;
        }

        if (NodeStackTryPeek(n, out var top))
            updated |= RunAhead(top);

        return updated;
    }
}
