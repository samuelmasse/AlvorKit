namespace AlvorKit;

/// <summary>Runs one benchmark child without a shell, preserving isolation and captured diagnostics.</summary>
public class BenchProcessRunner
{
    /// <summary>Starts the current executable with exact arguments and fails explicitly on timeout.</summary>
    public BenchProcessResult Run(string assemblyPath, IEnumerable<string> arguments, TimeSpan timeout)
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("The process has no executable path.");
        var host = Path.GetFileNameWithoutExtension(executable);

        if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(timeout));

        if (!host.Equals("dotnet", StringComparison.OrdinalIgnoreCase) &&
            !host.Equals(Path.GetFileNameWithoutExtension(assemblyPath), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("An apphost may only relaunch its own benchmark assembly.");
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        if (host.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(assemblyPath);

        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);

        using var process = Process.Start(start) ?? throw new IOException("Could not start the benchmark child.");
        var output = process.StandardOutput.ReadToEndAsync();
        var errors = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit((int)timeout.TotalMilliseconds))
        {
            process.Kill(true);
            process.WaitForExit();
            throw new TimeoutException($"Benchmark child exceeded {timeout.TotalSeconds:N0} seconds.");
        }

        return new(output.GetAwaiter().GetResult(), errors.GetAwaiter().GetResult(), process.ExitCode);
    }
}
