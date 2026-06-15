using System.Formats.Tar;
using System.IO.Compression;

namespace AlvorKit.Script.NativeBuild;

/// <summary>Downloads and extracts upstream source archives when not already cached.</summary>
[ExcludeFromCodeCoverage]
internal static class SourceArchiveFetcher
{
    /// <summary>Ensures the source directory exists for a library.</summary>
    public static async Task EnsureSourceAsync(LibraryBuildContext library)
    {
        if (Directory.Exists(library.SourceDirectory))
            return;

        if (library.Metadata.SourceUrl is null)
            throw new FileNotFoundException($"{library.SourceDirectory} not found and no sourceUrl configured.");

        Directory.CreateDirectory(library.WorkRoot);
        var url = library.ReplaceVersionTokens(library.Metadata.SourceUrl);
        Console.WriteLine($"Fetching {url}");
        using var http = new HttpClient();
        await using var archive = new GZipStream(await http.GetStreamAsync(url), CompressionMode.Decompress);
        await TarFile.ExtractToDirectoryAsync(archive, library.WorkRoot, overwriteFiles: true);
    }
}
