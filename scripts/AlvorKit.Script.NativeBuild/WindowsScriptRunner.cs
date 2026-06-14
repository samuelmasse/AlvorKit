using System.ComponentModel;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Writes generated PowerShell to a temporary file and executes it.</summary>
internal sealed class WindowsScriptRunner
{
    /// <summary>Process runner used for pwsh and Windows PowerShell fallback.</summary>
    private readonly IProcessRunner processRunner;

    /// <summary>Creates a script runner around a process runner.</summary>
    public WindowsScriptRunner(IProcessRunner processRunner)
    {
        this.processRunner = processRunner;
    }

    /// <summary>Runs generated PowerShell and falls back to powershell.exe when pwsh is absent.</summary>
    public async Task RunAsync(string script)
    {
        var path = Path.Combine(Path.GetTempPath(), "alvorkit-native-" + Guid.NewGuid().ToString("N") + ".ps1");
        await File.WriteAllTextAsync(path, "$ErrorActionPreference = 'Stop'\n" + script);
        try
        {
            try
            {
                await RunPowerShellAsync("pwsh", path);
            }
            catch (Win32Exception)
            {
                // GitHub Windows images have pwsh, but local developer machines may only have Windows PowerShell.
                await RunPowerShellAsync("powershell", path);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Runs one PowerShell executable against a generated script file.</summary>
    private Task RunPowerShellAsync(string executable, string path) =>
        processRunner.RunAsync(new(executable, ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", path]));
}
