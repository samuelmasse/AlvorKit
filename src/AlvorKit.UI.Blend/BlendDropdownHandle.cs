namespace AlvorKit;

/// <summary>Controls one mounted popup; fields sharing this handle share its current options and selection.</summary>
public class BlendDropdownHandle
{
    private readonly EntMut panel;
    private EntMut anchor;
    private IReadOnlyList<BlendDropdownItem>? items;
    private Action<int>? onPick;
    private int selectedIndex;
    private int highlightIndex;
    private bool openedThisUpdate;

    internal EntMut Anchor => anchor;
    internal IReadOnlyList<BlendDropdownItem> Items => items!;
    internal int SelectedIndex => selectedIndex;
    internal int HighlightIndex => highlightIndex;

    /// <summary>Gets whether this popup is open.</summary>
    public bool IsOpen => items != null;

    internal BlendDropdownHandle(EntMut panel) => this.panel = panel;

    /// <summary>Returns whether this popup is open for the supplied field.</summary>
    public bool IsOpenFor(EntMut field) => IsOpen && anchor == field;

    /// <summary>Opens or replaces this popup's options and requests their preparation before the next layout.</summary>
    public void Open(EntMut anchor, IReadOnlyList<BlendDropdownItem> items, int selectedIndex, Action<int> onPick)
    {
        this.anchor = anchor;
        this.items = items;
        this.onPick = onPick;
        this.selectedIndex = selectedIndex;
        highlightIndex = selectedIndex;
        openedThisUpdate = true;
        panel.Mutate()
            .RefreshCountV(1);
    }

    /// <summary>Closes this popup without choosing an option.</summary>
    public void Close()
    {
        anchor = default;
        items = null;
        onPick = null;
    }

    internal bool SkipOpeningUpdate()
    {
        var skip = openedThisUpdate;
        openedThisUpdate = false;
        return skip;
    }

    internal void Highlight(int index) => highlightIndex = index;

    internal void MoveHighlight(int direction) => highlightIndex = (highlightIndex + direction + items!.Count) % items.Count;

    internal void Pick(int index)
    {
        var pick = onPick;
        Close();
        pick?.Invoke(index);
    }
}
