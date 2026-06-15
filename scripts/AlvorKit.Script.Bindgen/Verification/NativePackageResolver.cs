namespace AlvorKit.Script.Bindgen;

/// <summary>Resolved native package location for export verification.</summary>
/// <param name="PackageId">Native package identifier expected by the backend project.</param>
/// <param name="Version">Native package version expected by the backend project.</param>
/// <param name="LibraryPath">Resolved or expected host native library path.</param>
/// <param name="LibraryExists">Whether the host native library exists at the resolved path.</param>
/// <param name="Failure">Reason the host native library could not be resolved, when absent.</param>
public sealed record NativePackageLibraryResolution(
    string PackageId,
    string Version,
    string LibraryPath,
    bool LibraryExists,
    string? Failure);

/// <summary>Finds the restored host native library package used to verify generated imports.</summary>
public static class NativePackageResolver
{
    /// <summary>Resolves the host native library, restoring the generated backend project when needed.</summary>
    public static async Task<NativePackageLibraryResolution> ResolveHostLibraryAsync(NativeLibraryBinding library)
    {
        if (TryFindHostLibrary(library) is { } libraryPath)
            return Found(library, libraryPath);

        if (!await RestoreBackendProjectAsync(library))
            return Missing(library, "dotnet restore could not restore the native package");

        return TryFindHostLibrary(library) is { } restoredPath
            ? Found(library, restoredPath)
            : Missing(
                library,
                $"restored package does not contain runtimes/{library.HostRuntimeIdentifier}/native/{library.HostNativeLibraryFileName}");
    }

    /// <summary>Creates a successful package resolution value.</summary>
    private static NativePackageLibraryResolution Found(NativeLibraryBinding library, string libraryPath) =>
        new(library.NativePackageId, library.NativeVersion, libraryPath, LibraryExists: true, Failure: null);

    /// <summary>Creates a missing package resolution value with the path bindgen expected to find.</summary>
    private static NativePackageLibraryResolution Missing(NativeLibraryBinding library, string failure) =>
        new(library.NativePackageId, library.NativeVersion, ExpectedHostLibraryPath(library), LibraryExists: false, failure);

    /// <summary>Searches all known package roots for the host native library.</summary>
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

    /// <summary>Returns the package path that would contain the host native library in the first known package root.</summary>
    private static string ExpectedHostLibraryPath(NativeLibraryBinding library) =>
        HostLibraryPath(PackageRoots(library).FirstOrDefault() ?? DefaultGlobalPackagesRoot(), library);

    /// <summary>Combines a package root with the native package identity and host runtime path.</summary>
    private static string HostLibraryPath(string packageRoot, NativeLibraryBinding library) =>
        Path.Combine(
            packageRoot,
            library.NativePackageId.ToLowerInvariant(),
            library.NativeVersion.ToLowerInvariant(),
            "runtimes",
            library.HostRuntimeIdentifier,
            "native",
            library.HostNativeLibraryFileName);

    /// <summary>Returns NuGet package roots from project assets, environment configuration, and the default cache.</summary>
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

    /// <summary>Reads package roots from the generated backend project's project.assets.json file.</summary>
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

    /// <summary>Restores the generated backend project so its native package reference can be resolved.</summary>
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

    /// <summary>Returns the generated backend project's assets file path.</summary>
    private static string ProjectAssetsPath(NativeLibraryBinding library) =>
        Path.Combine(
            library.RepositoryRoot,
            "obj",
            Path.GetFileName(library.Config.BackendProject),
            "project.assets.json");

    /// <summary>Returns the generated backend project file path.</summary>
    private static string BackendProjectPath(NativeLibraryBinding library)
    {
        var projectName = Path.GetFileName(library.Config.BackendProject);
        return Path.Combine(library.RepositoryRoot, library.Config.BackendProject, projectName + ".csproj");
    }

    /// <summary>Returns the current user's default NuGet global package cache.</summary>
    private static string DefaultGlobalPackagesRoot() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

    /// <summary>Normalizes a package root so duplicate forms compare equal.</summary>
    private static string NormalizeDirectory(string directory) =>
        directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
