namespace AlvorKit;

[Root]
public class RootUiUpdate
{
    private double time;

    internal void Advance(double delta) => time += delta;

    internal void Refresh(EntMut root)
    {
        // A refresh can request another pass on a node visited earlier in the tree.
        while (RunRefresh(root))
        {
        }
    }

    internal void Update(EntMut n)
    {
        if (!UiTree.IsActive(n))
            return;

        n.OnUpdateFV.Resolve()?.Invoke();

        foreach (var child in n.NodesR.Span)
            Update(child);
    }

    private bool RunRefresh(EntMut n)
    {
        if (!UiTree.IsActive(n))
        {
            Deactivate(n);
            return false;
        }

        var interval = n.RefreshIntervalFV.Resolve().TotalSeconds;
        var due = !n.UiRefreshActive || (interval > 0 && time - n.UiRefreshTime >= interval);
        n.UiRefreshActive = true;
        var refreshed = false;

        while (due || n.RefreshCountFV.Resolve() > 0)
        {
            var remaining = n.RefreshCountFV.Resolve();
            if (remaining > 0)
                n.RefreshCountFV = remaining - 1;

            n.UiRefreshTime = time;
            n.OnRefreshFV.Resolve()?.Invoke();
            refreshed = true;
            due = false;

            if (!UiTree.IsActive(n))
            {
                Deactivate(n);
                return refreshed;
            }
        }

        refreshed |= RefreshChildren(n);
        refreshed |= RefreshStack(n);
        return refreshed;
    }

    private bool RefreshChildren(EntMut n)
    {
        var refreshed = false;
        var index = 0;

        while (UiTree.IsActive(n) && index < NodesCount(n))
        {
            var child = Nodes(n)[index];
            refreshed |= RunRefresh(child);

            // Callbacks may replace child storage or remove the current node and its siblings.
            if (index < NodesCount(n) && Nodes(n)[index] == child)
                index++;
        }

        return refreshed;
    }

    private bool RefreshStack(EntMut n)
    {
        var refreshed = false;
        var index = 0;

        while (UiTree.IsActive(n) && index < NodeStackCount(n))
        {
            var entry = NodeStack(n)[index];
            var companion = UiTree.Companion(n, entry);
            if (companion != default)
                refreshed |= RunRefresh(companion);

            if (!NodeStackTryPeek(n, out var top) || top != entry)
                Deactivate(entry);

            if (index < NodeStackCount(n) && NodeStack(n)[index] == entry)
                index++;
        }

        if (UiTree.IsActive(n) && NodeStackTryPeek(n, out var current))
            refreshed |= RunRefresh(current);

        return refreshed;
    }

    private void Deactivate(EntMut n)
    {
        if (!n.UiRefreshActive)
            return;

        n.UiRefreshActive = false;
        foreach (var child in Nodes(n))
            Deactivate(child);

        foreach (var entry in NodeStack(n))
        {
            var companion = entry.CompanionFV.Resolve();
            if (companion != default)
                Deactivate(companion);

            Deactivate(entry);
        }
    }
}
