namespace AlvorKit.Demo;

/// <summary>Downloads the demo font (Inter, OFL) on first run.</summary>
public static class DemoFont
{
    private const string Url = "https://github.com/google/fonts/raw/main/ofl/inter/Inter%5Bopsz,wght%5D.ttf";
    private static readonly HttpClient Http = new();

    public static string GetPath()
    {
        var path = Path.Combine(Path.GetTempPath(), "Inter.ttf");
        if (File.Exists(path))
            return path;

        using var request = new HttpRequestMessage(HttpMethod.Get, Url);
        using var response = Http.Send(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var input = response.Content.ReadAsStream();
        using var output = File.Create(path);
        input.CopyTo(output);

        return path;
    }
}
