using System.ComponentModel;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Writes generated PowerShell to a temporary file and executes it.</summary>
internal static class WindowsScriptRunner
{
    /// <summary>Runs generated PowerShell and falls back to powershell.exe when pwsh is absent.</summary>
    public static async Task RunAsync(IProcessRunner processRunner, string script)
    {
        var path = Path.Combine(Path.GetTempPath(), "alvorkit-native-" + Guid.NewGuid().ToString("N") + ".ps1");
        await File.WriteAllTextAsync(path, "$ErrorActionPreference = 'Stop'\n" + script);
        try
        {
            try
            {
                await RunPowerShellAsync(processRunner, "pwsh", path);
            }
            catch (Win32Exception)
            {
                // GitHub Windows images have pwsh, but local developer machines may only have Windows PowerShell.
                await RunPowerShellAsync(processRunner, "powershell", path);
            }
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Runs one PowerShell executable against a generated script file.</summary>
    private static Task RunPowerShellAsync(IProcessRunner processRunner, string executable, string path) =>
        processRunner.RunAsync(new(executable, ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", path]));
}
