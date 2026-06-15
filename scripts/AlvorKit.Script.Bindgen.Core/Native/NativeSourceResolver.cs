using System.Formats.Tar;
using System.IO.Compression;

namespace AlvorKit.Script.Bindgen;

/// <summary>Downloads and extracts native source material required by a binding run.</summary>
public sealed class NativeSourceResolver
{
    /// <summary>Ensures the configured primary header or registry source exists locally.</summary>
    public async Task EnsureSourceAsync(NativeLibraryBinding library)
    {
        if (File.Exists(library.HeaderPath))
            return;

        if (library.Config.SourceUrl is null)
            throw new FileNotFoundException($"{library.HeaderPath} not found and no sourceUrl configured.");

        var url = library.ReplaceVersionTokens(library.Config.SourceUrl);
        Console.WriteLine($"Fetching {url}");

        // A direct .xml url (the gl.xml registry) is fetched as a single file: its repository
        // archive carries the full specs and reference pages, far too heavy for one file.
        if (url.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(library.SourceDirectory);
            using var http = new HttpClient();
            await File.WriteAllBytesAsync(library.HeaderPath, await http.GetByteArrayAsync(url));
            return;
        }

        await ExtractTarballAsync(url, Path.GetDirectoryName(library.SourceDirectory)!);
    }

    /// <summary>Fetches and extracts the OpenGL reference pages tarball when doc generation is configured.</summary>
    public async Task EnsureDocSourceAsync(NativeLibraryBinding library)
    {
        if (library.DocReadDirectory is null || Directory.Exists(library.DocReadDirectory))
            return;

        var url = library.ReplaceVersionTokens(library.Config.DocUrl!);
        Console.WriteLine($"Fetching {url}");
        await ExtractTarballAsync(url, library.WorkRoot);
    }

    /// <summary>Downloads a gzip-compressed tarball and extracts it into a destination directory.</summary>
    private static async Task ExtractTarballAsync(string url, string destination)
    {
        Directory.CreateDirectory(destination);
        using var http = new HttpClient();
        await using var archive = new GZipStream(await http.GetStreamAsync(url), CompressionMode.Decompress);
        await TarFile.ExtractToDirectoryAsync(archive, destination, overwriteFiles: true);
    }
}
