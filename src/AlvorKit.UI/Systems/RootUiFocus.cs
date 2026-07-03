namespace AlvorKit.UI;

[Root]
public class RootUiFocus(RootKeyboard keyboard)
{
    private readonly List<EntMut> focusables = [];
    private HashSet<EntMut> inits = [];
    private HashSet<EntMut> newInits = [];
    private EntMut focused;
    private bool tabMode;

    public void Focus(EntMut ent, bool tab)
    {
        tabMode = tab;

        var defer = ent.DeferFocusFV.Resolve();
        if (defer != default && DeferFocus(defer))
            return;

        focused.IsFocusedR = false;
        ent.IsFocusedR = true;
        focused = ent;

        var focusGroup = ent.FocusGroupFV.Resolve();
        if (focusGroup != default)
        {
            RemoveSelect(focusGroup);
            focusGroup.SelectedR = ent;
        }
        else RemoveSelect(ent);

        ent.IsSelectedR = true;
        ent.OnFocusFV.Resolve()?.Invoke();
    }

    private bool DeferFocus(EntMut ent)
    {
        if (ent == focused)
            return true;

        foreach (var c in ent.NodesR.Span)
        {
            if (DeferFocus(c))
                return true;
        }

        return false;
    }

    private void RemoveSelect(EntMut ent)
    {
        if (ent.IsSelectedR)
        {
            ent.IsSelectedR = false;
            ent.OnUnselectFV.Resolve()?.Invoke();
        }

        foreach (var c in ent.NodesR.Span)
            RemoveSelect(c);
    }

    internal void Update(EntMut n)
    {
        (inits, newInits) = (newInits, inits);
        focusables.Clear();
        newInits.Clear();
        CollectFocusables(n);
        UnselectUnselectables(n);

        int index = focusables.IndexOf(focused);
        if (index < 0)
        {
            EntMut target = default;

            foreach (var ent in newInits)
            {
                if (!inits.Contains(ent))
                {
                    target = ent;
                    break;
                }
            }

            if (target == default && tabMode && focusables.Count > 0)
                target = focusables[0];

            Focus(target, tabMode);
        }

        var focusGroup = focused.FocusGroupFV.Resolve();

        if (focusables.Count > 0 && keyboard.IsKeyPressedRepeated(Keys.Tab))
        {
            int offset = 1;

            while (offset < focusables.Count)
            {
                var focusable = focusables[(index + offset) % focusables.Count];
                var nextFocuseGroup = focusable.FocusGroupFV.Resolve();

                if (nextFocuseGroup == default || nextFocuseGroup != focusGroup)
                    break;

                offset++;
            }

            if (offset < focusables.Count)
            {
                var focusable = focusables[(index + offset) % focusables.Count];
                var nextFocuseGroup = focusable.FocusGroupFV.Resolve();

                if (nextFocuseGroup != default)
                {
                    var selected = FindSelected(nextFocuseGroup);
                    if (selected != default)
                        focusable = selected;
                }

                Focus(focusable, true);
            }
        }
    }

    private void CollectFocusables(EntMut n)
    {
        var isFocusable = n.IsFocusableFV.Resolve();
        var isInputDisabled = n.IsInputDisabledFV.Resolve();

        if (isFocusable && !isInputDisabled)
        {
            focusables.Add(n);

            if (n.IsInitialFocusFV.Resolve())
                newInits.Add(n);
        }

        foreach (var c in n.NodesR.Span)
            CollectFocusables(c);
    }

    private void UnselectUnselectables(EntMut n)
    {
        if (n.SelectedR != default)
        {
            var selected = n.SelectedR;
            if (!HasChild(n, selected))
            {
                selected.IsSelectedR = false;
                selected.OnUnselectFV.Resolve()?.Invoke();
                n.SelectedR = default;
            }
        }

        foreach (var c in n.NodesR.Span)
            UnselectUnselectables(c);
    }

    private bool HasChild(EntMut n, EntMut child)
    {
        if (n == child)
            return true;

        foreach (var c in n.NodesR.Span)
        {
            if (HasChild(c, child))
                return true;
        }

        return false;
    }

    private EntMut FindSelected(EntMut ent)
    {
        if (ent.IsSelectedR)
            return ent;

        foreach (var c in ent.NodesR.Span)
        {
            var selected = FindSelected(c);
            if (selected != default)
                return selected;
        }

        return default;
    }
}
