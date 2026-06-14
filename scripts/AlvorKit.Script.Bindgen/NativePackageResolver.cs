using System.Diagnostics;
using System.Text.Json;

namespace AlvorKit.Script.Bindgen;

public sealed record NativePackageLibraryResolution(
    string PackageId,
    string Version,
    string LibraryPath,
    bool LibraryExists,
    string? Failure);

public static class NativePackageResolver
{
    public static async Task<NativePackageLibraryResolution> ResolveHostLibraryAsync(NativeLibraryBinding library)
    {
        if (TryFindHostLibrary(library) is { } libraryPath)
            return Found(library, libraryPath);

        if (!await RestoreBackendProjectAsync(library))
            return Missing(library, "dotnet restore could not restore the native package");

        return TryFindHostLibrary(library) is { } restoredPath
            ? Found(library, restoredPath)
            : Missing(library, $"restored package does not contain runtimes/{library.HostRuntimeIdentifier}/native/{library.HostNativeLibraryFileName}");
    }

    private static NativePackageLibraryResolution Found(NativeLibraryBinding library, string libraryPath) =>
        new(library.NativePackageId, library.NativeVersion, libraryPath, LibraryExists: true, Failure: null);

    private static NativePackageLibraryResolution Missing(NativeLibraryBinding library, string failure) =>
        new(library.NativePackageId, library.NativeVersion, ExpectedHostLibraryPath(library), LibraryExists: false, failure);

    private static string? TryFindHostLibrary(NativeLibraryBinding library)
    {
        foreach (var root in PackageRoots(library))
        {
            var libraryPath = HostLibraryPath(root, library);
            if (File.Exists(libraryPath))
                return libraryPath;
        }

        return null;
    }

    private static string ExpectedHostLibraryPath(NativeLibraryBinding library) =>
        HostLibraryPath(PackageRoots(library).FirstOrDefault() ?? DefaultGlobalPackagesRoot(), library);

    private static string HostLibraryPath(string packageRoot, NativeLibraryBinding library) =>
        Path.Combine(
            packageRoot,
            library.NativePackageId.ToLowerInvariant(),
            library.NativeVersion.ToLowerInvariant(),
            "runtimes",
            library.HostRuntimeIdentifier,
            "native",
            library.HostNativeLibraryFileName);

    private static IEnumerable<string> PackageRoots(NativeLibraryBinding library)
    {
        var roots = new List<string>();
        roots.AddRange(ProjectAssetsPackageRoots(library));

        if (Environment.GetEnvironmentVariable("NUGET_PACKAGES") is { Length: > 0 } configuredRoot)
            roots.Add(configuredRoot);

        roots.Add(DefaultGlobalPackagesRoot());
        return roots
            .Select(NormalizeDirectory)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ProjectAssetsPackageRoots(NativeLibraryBinding library)
    {
        var path = ProjectAssetsPath(library);
        if (!File.Exists(path))
            yield break;

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty("packageFolders", out var packageFolders))
            yield break;

        foreach (var folder in packageFolders.EnumerateObject())
            yield return folder.Name;
    }

    private static async Task<bool> RestoreBackendProjectAsync(NativeLibraryBinding library)
    {
        var projectPath = BackendProjectPath(library);
        if (!File.Exists(projectPath))
            return false;

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("restore");
        startInfo.ArgumentList.Add(projectPath);
        startInfo.ArgumentList.Add("-v:quiet");

        using var process = Process.Start(startInfo);
        if (process is null)
            return false;

        await process.StandardOutput.ReadToEndAsync();
        await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return process.ExitCode == 0;
    }

    private static string ProjectAssetsPath(NativeLibraryBinding library) =>
        Path.Combine(library.RepositoryRoot, "obj", Path.GetFileName(library.Config.BackendProject), "project.assets.json");

    private static string BackendProjectPath(NativeLibraryBinding library)
    {
        var projectName = Path.GetFileName(library.Config.BackendProject);
        return Path.Combine(library.RepositoryRoot, library.Config.BackendProject, projectName + ".csproj");
    }

    private static string DefaultGlobalPackagesRoot() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

    private static string NormalizeDirectory(string directory) =>
        directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
