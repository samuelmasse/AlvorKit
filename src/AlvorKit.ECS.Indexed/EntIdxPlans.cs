namespace AlvorKit;

/// <summary>Typed sparse registry; the first eight context slots avoid an extra page indirection.</summary>
internal static class EntIdxPlans<T, N>
{
    /// <summary>Bits reserved for the offset within one context page.</summary>
    private const int PageBits = 3;
    /// <summary>Small page capacity keeps rare overflow contexts compact.</summary>
    private const int PageSize = 1 << PageBits;
    /// <summary>Extracts the within-page context offset.</summary>
    private const int PageMask = PageSize - 1;

    /// <summary>Serializes cold page creation across independent contexts.</summary>
    private static readonly Lock gate = new();
    /// <summary>Direct slots for the common small-context case.</summary>
    private static readonly EntIdxWritePlan<T, N>?[] first = new EntIdxWritePlan<T, N>?[PageSize];
    /// <summary>Stable overflow pages shared by this component type.</summary>
    private static EntIdxWritePlan<T, N>?[]?[] pages = [];

    /// <summary>A live Indexed Ent keeps its context slot owned; disposed contexts clear plans before slot reuse.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static EntIdxWritePlan<T, N>? Get(int contextIndex)
    {
        if (contextIndex < PageSize)
            return first[contextIndex];

        contextIndex -= PageSize;
        var current = pages;
        int pageIndex = contextIndex >> PageBits;
        return pageIndex < current.Length ? current[pageIndex]?[contextIndex & PageMask] : null;
    }

    /// <summary>Publishes or clears the plan for a context-owned slot.</summary>
    internal static void Set(int contextIndex, EntIdxWritePlan<T, N>? plan)
    {
        if (contextIndex < PageSize)
        {
            first[contextIndex] = plan;
            return;
        }

        contextIndex -= PageSize;
        int pageIndex = contextIndex >> PageBits;
        var current = pages;

        if (pageIndex < current.Length && current[pageIndex] is { } page)
        {
            page[contextIndex & PageMask] = plan;
            return;
        }

        CreatePage(contextIndex, plan);
    }

    /// <summary>Existing pages never move. Each context owns its slot, even while another thread grows the table.</summary>
    private static void CreatePage(int contextIndex, EntIdxWritePlan<T, N>? plan)
    {
        lock (gate)
        {
            int pageIndex = contextIndex >> PageBits;

            if (pageIndex >= pages.Length)
                Array.Resize(ref pages, (int)BitOperations.RoundUpToPowerOf2((uint)pageIndex + 1));

            var page = pages[pageIndex] ??= new EntIdxWritePlan<T, N>?[PageSize];
            page[contextIndex & PageMask] = plan;
        }
    }
}
