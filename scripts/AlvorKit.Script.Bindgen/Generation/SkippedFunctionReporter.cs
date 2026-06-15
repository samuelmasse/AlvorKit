namespace AlvorKit.Script.Bindgen;

/// <summary>Writes a stable console summary for functions skipped during model construction.</summary>
internal static class SkippedFunctionReporter
{
    /// <summary>Prints skipped native functions when the parser reported any.</summary>
    public static void Print(IReadOnlyCollection<string> skippedFunctions)
    {
        if (skippedFunctions.Count == 0)
            return;

        Console.WriteLine("Skipped functions:");
        foreach (var skipped in skippedFunctions)
            Console.WriteLine($"  {skipped}");
    }
}
