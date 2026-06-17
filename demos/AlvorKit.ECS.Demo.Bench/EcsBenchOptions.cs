namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Stores command-line options for the ECS benchmark demo.</summary>
public sealed record EcsBenchOptions(int Operations, int Runs, int Warmups, string? JsonPath, bool ShowHelp)
{
    /// <summary>Parses benchmark options from command-line arguments.</summary>
    public static EcsBenchOptions Parse(string[] args)
    {
        int operations = 5_000_000;
        int runs = 7;
        int warmups = 2;
        string? jsonPath = null;
        bool showHelp = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (arg == "--quick")
            {
                operations = 1_000_000;
                runs = 3;
                warmups = 1;
            }
            else if (arg == "--operations")
            {
                operations = ParsePositive(args, ref i, arg);
            }
            else if (arg == "--runs")
            {
                runs = ParsePositive(args, ref i, arg);
            }
            else if (arg == "--warmups")
            {
                warmups = ParseNonNegative(args, ref i, arg);
            }
            else if (arg == "--json")
            {
                jsonPath = ParseValue(args, ref i, arg);
            }
            else if (arg is "--help" or "-h" or "/?")
            {
                showHelp = true;
            }
            else throw new ArgumentException($"Unknown ECS benchmark option '{arg}'.");
        }

        return new(operations, runs, warmups, jsonPath, showHelp);
    }

    /// <summary>Writes the supported benchmark options to the console.</summary>
    public static void PrintHelp()
    {
        Console.WriteLine("Usage: dotnet run -c Release --project demos/AlvorKit.ECS.Demo.Bench -- [options]");
        Console.WriteLine("Options:");
        Console.WriteLine("  --quick              Use a short agent-friendly sweep.");
        Console.WriteLine("  --operations <n>     Operations per measured run. Default: 5000000.");
        Console.WriteLine("  --runs <n>           Measured runs per benchmark. Default: 7.");
        Console.WriteLine("  --warmups <n>        Warmup runs per benchmark. Default: 2.");
        Console.WriteLine("  --json <path>        Write a JSON result file.");
    }

    private static int ParsePositive(string[] args, ref int index, string option)
    {
        int value = ParseNonNegative(args, ref index, option);
        return value > 0 ? value : throw new ArgumentOutOfRangeException(option, "Value must be positive.");
    }

    private static int ParseNonNegative(string[] args, ref int index, string option)
    {
        string value = ParseValue(args, ref index, option);
        return int.TryParse(value, out int parsed) && parsed >= 0
            ? parsed
            : throw new ArgumentOutOfRangeException(option, "Value must be a non-negative integer.");
    }

    private static string ParseValue(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Option '{option}' requires a value.");

        index++;
        return args[index];
    }
}
