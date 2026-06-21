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
            var commandArgs = args.Length == 0 ? ["--help"] : args;
            var split = TestTimingCommandLineSplit.Create(commandArgs);
            var command = TestTimingCommandParser.CreateRootCommand(
                () => ProjectRoot.FindFromCurrentProcess(typeof(TestTimingCommandParser)),
                split.ForwardedArguments,
                options => new TestTimingRunner().Run(options));
            return command.Parse(split.TimingArguments).Invoke(new() { EnableDefaultExceptionHandler = false });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
