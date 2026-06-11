namespace AlvorKit.Demo;

/// <summary>Downloads the demo font (Inter, OFL) on first run.</summary>
public static class DemoFont
{
    private const string Url = "https://github.com/google/fonts/raw/main/ofl/inter/Inter%5Bopsz,wght%5D.ttf";

    public static async Task<string> DownloadAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), "Inter.ttf");
        if (File.Exists(path))
            return path;

        using var http = new HttpClient();
        File.WriteAllBytes(path, await http.GetByteArrayAsync(Url));
        return path;
    }
}
