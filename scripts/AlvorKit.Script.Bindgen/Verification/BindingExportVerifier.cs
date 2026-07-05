namespace AlvorKit.Script.Bindgen;

/// <summary>Verifies generated C header imports against the restored native package for the host runtime.</summary>
/// <param name="options">Command-line options that decide whether verification failures are fatal.</param>
[ExcludeFromCodeCoverage]
internal sealed class BindingExportVerifier(BindgenOptions options)
{
    /// <summary>Checks whether every generated import has a matching native export.</summary>
    public async Task VerifyAsync(NativeLibraryBinding library, BindingModel model)
    {
        var nativePackage = await NativePackageResolver.ResolveHostLibraryAsync(library);
        if (!nativePackage.LibraryExists)
        {
            if (options.Strict)
                throw new FileNotFoundException(
                    $"strict mode requires {nativePackage.PackageId} {nativePackage.Version} for export verification: " +
                    $"{nativePackage.LibraryPath}");
            Console.WriteLine(
                $"Exports: skipped ({nativePackage.PackageId} {nativePackage.Version} not restored: " +
                $"{nativePackage.Failure})");
            return;
        }

        var verification = NativeExportVerifier.Verify(nativePackage.LibraryPath, model);
        var missingRequired = verification.MissingRequired;
        var missingPlatform = verification.MissingPlatform;
        if (missingRequired.Count > 0 && options.Strict)
            throw new InvalidOperationException(
                $"{missingRequired.Count} entry points missing from " +
                $"{Path.GetFileName(verification.LibraryPath)}: {FunctionList(missingRequired)}");

        var libraryName = Path.GetFileName(verification.LibraryPath);
        Console.WriteLine(missingRequired.Count == 0
            ? $"Exports: all {model.Functions.Count - missingPlatform.Count} host entry points found in " +
                $"{nativePackage.PackageId} {nativePackage.Version} ({libraryName})"
            : $"WARNING: {missingRequired.Count} entry points missing from {libraryName}: " +
                $"{FunctionList(missingRequired)}");
        if (missingPlatform.Count > 0)
            Console.WriteLine(
                $"Exports: {missingPlatform.Count} platform-specific entry points not exported by {libraryName} " +
                $"(expected): {FunctionList(missingPlatform)}");
    }

    /// <summary>Formats a short list of native entry points for console diagnostics.</summary>
    private static string FunctionList(List<BindingFunction> functions) =>
        string.Join(", ", functions.Select(function => function.NativeName).Take(10));
}
