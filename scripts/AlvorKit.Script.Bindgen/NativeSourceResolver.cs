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
        Directory.CreateDirectory(Path.GetDirectoryName(library.SourceDirectory)!);
        using var http = new HttpClient();
        await using var archive = new GZipStream(await http.GetStreamAsync(url), CompressionMode.Decompress);
        await TarFile.ExtractToDirectoryAsync(archive, Path.GetDirectoryName(library.SourceDirectory)!, overwriteFiles: true);
    }
}
