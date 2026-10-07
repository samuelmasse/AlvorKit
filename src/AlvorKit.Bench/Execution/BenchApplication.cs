namespace AlvorKit;

/// <summary>Coordinates suite selection, execution, and optional result export.</summary>
[Bench]
public class BenchApplication(
    BenchCommand command,
    IBenchSuiteProvider suiteProvider,
    BenchConsole output,
    BenchCatalogBuilder catalogBuilder,
    BenchSelector selector,
    BenchRunner runner,
    BenchJsonWriter jsonWriter)
{
    /// <summary>Exit status for a selection containing no runnable cases.</summary>
    private const int InvalidArgumentsExitCode = 2;

    /// <summary>Lists or measures selected cases and reports an empty selection as an error.</summary>
    public int Run()
    {
        var catalog = catalogBuilder.Create(suiteProvider.Create());
        var benchCases = selector.Select(catalog, command.IncludePatterns, command.ExcludePatterns);

        if (benchCases.Length == 0)
        {
            output.WriteError("No benchmarks matched the supplied patterns.");
            return InvalidArgumentsExitCode;
        }

        if (command.Kind == BenchCommandKind.List)
        {
            output.WriteCatalog(catalog, benchCases);
            return 0;
        }

        var execution = runner.Run(catalog, benchCases);
        WriteJson(catalog, execution);
        return 0;
    }

    /// <summary>Exports only when requested and prints a file path only in human-output mode.</summary>
    private void WriteJson(BenchCatalog catalog, BenchExecution execution)
    {
        if (command.JsonPath is null)
            return;

        var path = jsonWriter.Write(command.JsonPath, catalog, command, execution);

        if (path is not null && !command.Display.Quiet)
            output.WriteJsonPath(path);
    }
}
