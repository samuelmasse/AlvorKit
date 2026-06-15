namespace AlvorKit.Script.Bindgen;

/// <summary>Checks that generated binding functions are present in a native library.</summary>
[ExcludeFromCodeCoverage]
public static class NativeExportVerifier
{
    /// <summary>Verifies generated function exports against a native library path.</summary>
    public static NativeExportVerification Verify(string libraryPath, BindingModel model)
    {
        if (!File.Exists(libraryPath))
            return new(libraryPath, LibraryExists: false, MissingFunctions: []);

        var elfExports = TryReadElfExports(libraryPath);
        if (elfExports is not null)
            return new(libraryPath, LibraryExists: true, MissingFrom(elfExports, model));

        var nativeLibrary = NativeLibrary.Load(libraryPath);
        try
        {
            return new(libraryPath, LibraryExists: true, MissingFrom(nativeLibrary, model));
        }
        finally
        {
            NativeLibrary.Free(nativeLibrary);
        }
    }

    /// <summary>Returns functions missing from a read export set.</summary>
    private static List<BindingFunction> MissingFrom(HashSet<string> exports, BindingModel model) =>
        [.. model.Functions.Where(function => !exports.Contains(function.NativeName))];

    /// <summary>Returns functions missing from a loaded native library handle.</summary>
    private static List<BindingFunction> MissingFrom(nint nativeLibrary, BindingModel model) =>
        [.. model.Functions.Where(function => !NativeLibrary.TryGetExport(nativeLibrary, function.NativeName, out _))];

    /// <summary>Reads ELF exports with readelf when the library is an ELF shared object.</summary>
    private static HashSet<string>? TryReadElfExports(string libraryPath)
    {
        if (!IsElf(libraryPath))
            return null;

        var startInfo = new ProcessStartInfo
        {
            FileName = "readelf",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("-Ws");
        startInfo.ArgumentList.Add(libraryPath);

        try
        {
            using var process = Process.Start(startInfo);
            if (process is null)
                return null;

            var output = process.StandardOutput.ReadToEnd();
            process.StandardError.ReadToEnd();
            process.WaitForExit();
            return process.ExitCode == 0 ? ParseReadElfSymbols(output) : null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Parses function symbols from readelf output.</summary>
    private static HashSet<string> ParseReadElfSymbols(string output)
    {
        var symbols = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in output.Split('\n'))
        {
            var fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length < 8)
                continue;
            if (fields[3] != "FUNC")
                continue;
            if (fields[4] is not ("GLOBAL" or "WEAK"))
                continue;
            if (fields[6] == "UND")
                continue;

            var name = fields[7];
            var versionSuffix = name.IndexOf('@', StringComparison.Ordinal);
            symbols.Add(versionSuffix >= 0 ? name[..versionSuffix] : name);
        }

        return symbols;
    }

    /// <summary>Returns true when a file starts with the ELF magic bytes.</summary>
    private static bool IsElf(string libraryPath)
    {
        Span<byte> magic = stackalloc byte[4];
        using var file = File.OpenRead(libraryPath);
        return file.Read(magic) == magic.Length
            && magic[0] == 0x7f
            && magic[1] == (byte)'E'
            && magic[2] == (byte)'L'
            && magic[3] == (byte)'F';
    }
}
