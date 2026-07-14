namespace AlvorKit.Windowing;

/// <summary>Owns UTF-8 strings and native filter records for one synchronous NFDe call.</summary>
internal sealed class NfdeDialogMemory : IDisposable
{
    private readonly List<nint> allocations = [];

    /// <summary>Creates native filters and optional path/name strings for one dialog.</summary>
    internal NfdeDialogMemory(ReadOnlySpan<FileDialogFilter> filters, string? defaultPath, string? defaultName = null)
    {
        for (var i = 0; i < filters.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(filters[i].Name))
                throw new ArgumentException("File dialog filter names must not be empty.", nameof(filters));
            if (string.IsNullOrWhiteSpace(filters[i].Extensions))
                throw new ArgumentException("File dialog filter extensions must not be empty.", nameof(filters));
        }

        Filters = new NfdUtf8FilterItem[filters.Length];
        for (var i = 0; i < filters.Length; i++)
            Filters[i] = new() { Name = Add(filters[i].Name), Spec = Add(filters[i].Extensions) };

        DefaultPath = Add(defaultPath);
        DefaultName = Add(defaultName);
    }

    /// <summary>Gets the native filter records kept alive by this owner.</summary>
    internal NfdUtf8FilterItem[] Filters { get; }

    /// <summary>Gets the optional native UTF-8 default path.</summary>
    internal nint DefaultPath { get; }

    /// <summary>Gets the optional native UTF-8 default name.</summary>
    internal nint DefaultName { get; }

    /// <summary>Frees every temporary UTF-8 allocation.</summary>
    public void Dispose()
    {
        foreach (var allocation in allocations)
            Marshal.FreeCoTaskMem(allocation);
        allocations.Clear();
    }

    /// <summary>Copies optional text to native UTF-8 memory and tracks its ownership.</summary>
    private nint Add(string? value)
    {
        if (value is null)
            return 0;

        var allocation = Marshal.StringToCoTaskMemUTF8(value);
        allocations.Add(allocation);
        return allocation;
    }
}
