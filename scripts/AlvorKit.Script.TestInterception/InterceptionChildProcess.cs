namespace AlvorKit.Script.TestInterception;

/// <summary>Runs one bounded child and forwards its captured output.</summary>
internal static class InterceptionChildProcess
{
    /// <summary>Runs a child, killing its process tree when the hard timeout expires.</summary>
    internal static async Task<int> RunAsync(
        ProcessStartInfo startInfo,
        TimeSpan timeout,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
            throw new InvalidOperationException("Process.Start returned false.");

        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        deadline.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(deadline.Token);
        }
        catch (OperationCanceledException) when (
            !cancellationToken.IsCancellationRequested)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync(CancellationToken.None);
            Console.Error.WriteLine(
                $"Interception child exceeded {timeout.TotalSeconds:0.###} seconds.");
            return 124;
        }

        Console.Out.Write(await output);
        Console.Error.Write(await error);
        return process.ExitCode;
    }
}
