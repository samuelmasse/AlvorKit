namespace AlvorKit;

/// <summary>Formats hierarchy indentation and binary byte units consistently across reports.</summary>
internal static class BenchFormatting
{
    /// <summary>Binary byte units advance by ten bits.</summary>
    private const int ByteUnitShift = 10;
    /// <summary>Boundary between byte and kilobyte output.</summary>
    private const long BytesPerKilobyte = 1L << ByteUnitShift;
    /// <summary>Boundary between kilobyte and megabyte output.</summary>
    private const long BytesPerMegabyte = BytesPerKilobyte << ByteUnitShift;
    /// <summary>Boundary between megabyte and gigabyte output.</summary>
    private const long BytesPerGigabyte = BytesPerMegabyte << ByteUnitShift;
    /// <summary>Boundary between gigabyte and terabyte output.</summary>
    private const long BytesPerTerabyte = BytesPerGigabyte << ByteUnitShift;

    /// <summary>Returns two spaces for each hierarchy level.</summary>
    internal static string Indent(int depth) => new(' ', depth * 2);

    /// <summary>Selects a binary unit while preserving readable fractional magnitudes.</summary>
    internal static string FormatBytes(double bytes)
    {
        if (bytes < BytesPerKilobyte)
            return $"{bytes:N0} B";

        if (bytes < BytesPerMegabyte)
            return $"{(double)bytes / BytesPerKilobyte:N1} KB";

        if (bytes < BytesPerGigabyte)
            return $"{(double)bytes / BytesPerMegabyte:N1} MB";

        if (bytes < BytesPerTerabyte)
            return $"{(double)bytes / BytesPerGigabyte:N1} GB";

        return $"{(double)bytes / BytesPerTerabyte:N1} TB";
    }

}
