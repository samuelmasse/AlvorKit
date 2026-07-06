namespace AlvorKit.Script.ZoneSolution;

/// <summary>Finds default paths for AlvorZone solution generation.</summary>
internal static class ZoneSolutionDefaults
{
    /// <summary>Finds the sibling repository root from the current AlvorKit checkout.</summary>
    public static string FindZoneRoot()
    {
        var alvorKitRoot = ProjectRoot.FindFromCurrentProcess(typeof(ZoneSolutionDefaults));
        return Directory.GetParent(alvorKitRoot)?.FullName
            ?? throw new InvalidOperationException($"AlvorKit repository root '{alvorKitRoot}' has no parent directory.");
    }
}
