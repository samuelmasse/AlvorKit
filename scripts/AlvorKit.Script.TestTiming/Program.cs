namespace AlvorKit.Script.TestTiming;

/// <summary>Entry point for the unit test timing guard.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs the timing guard and returns a process exit code.</summary>
    /// <param name="args">Timing guard options followed by optional <c>dotnet test</c> arguments.</param>
    public static int Main(string[] args)
    {
        try
        {
            var options = new TestTimingCommandParser().Parse(args);
            if (options.IsHelp)
            {
                Console.WriteLine(TestTimingCommandParser.HelpText);
                return 0;
            }

            return new TestTimingRunner().Run(options);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
