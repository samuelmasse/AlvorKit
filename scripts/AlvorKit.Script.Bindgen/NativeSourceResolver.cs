using System.Formats.Tar;
using System.IO.Compression;

namespace AlvorKit.Script.Bindgen;

public sealed class NativeSourceResolver
{
    public async Task EnsureSourceAsync(NativeLibraryBinding library)
    {
        if (File.Exists(library.HeaderPath))
            return;

        if (library.Config.SourceUrl is null)
            throw new FileNotFoundException($"{library.HeaderPath} not found and no sourceUrl configured.");

        var url = library.ReplaceVersionTokens(library.Config.SourceUrl);
        Console.WriteLine($"Fetching {url}");
        using var http = new HttpClient();

        // A direct .xml url (the gl.xml registry) is fetched as a single file: its repository
        // archive carries the full specs and reference pages, far too heavy for one file.
        if (url.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(library.SourceDirectory);
            await File.WriteAllBytesAsync(library.HeaderPath, await http.GetByteArrayAsync(url));
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(library.SourceDirectory)!);
        await using var archive = new GZipStream(await http.GetStreamAsync(url), CompressionMode.Decompress);
        await TarFile.ExtractToDirectoryAsync(archive, Path.GetDirectoryName(library.SourceDirectory)!, overwriteFiles: true);
    }
}
