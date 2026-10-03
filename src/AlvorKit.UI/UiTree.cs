namespace AlvorKit;

/// <summary>Checks live attachment and visibility independently of the last layout's compiled child lists.</summary>
internal static class UiTree
{
    internal static bool IsActive(EntMut node)
    {
        while (node != default && node.IsAlive)
        {
            if (node.IsDeletedFV.Resolve() || node.IsDisabledFV.Resolve())
                return false;

            var parent = node.UiParent;
            if (parent == default)
                return node == (EntMut)node.UiRoot;

            var entry = node.UiStackEntry;
            if (entry == node)
            {
                if (!NodeStackTryPeek(parent, out var top) || top != node)
                    return false;
            }
            else if (entry != default &&
                (entry.UiParent != parent || entry.CompanionFV.Resolve() != node))
                return false;

            node = parent;
        }

        return false;
    }

    internal static EntMut Companion(EntMut parent, EntMut entry)
    {
        var companion = entry.CompanionFV.Resolve();
        if (companion != default)
        {
            companion.UiParent = parent;
            companion.UiStackEntry = entry;
        }

        return companion;
    }
}
