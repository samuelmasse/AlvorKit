namespace AlvorKit.FreeType.Demo;

/// <summary>Finds or downloads a TrueType font for the demo run.</summary>
internal static class DemoFont
{
    /// <summary>Resolves the font path from command-line input, common system fonts, or a downloaded fallback.</summary>
    /// <param name="args">Command-line arguments; the first argument may be a font path.</param>
    /// <param name="outputRoot">The demo output directory used for the downloaded fallback.</param>
    /// <param name="fallbackUrl">The fallback font URL.</param>
    /// <returns>An absolute path to a readable font file.</returns>
    public static string Resolve(string[] args, string outputRoot, string fallbackUrl)
    {
        if (args.Length > 0)
            return RequireFont(args[0]);

        foreach (var candidate in CandidateFonts())
        {
            if (File.Exists(candidate))
                return Path.GetFullPath(candidate);
        }

        var fallbackPath = Path.Combine(outputRoot, "Inter.ttf");
        if (File.Exists(fallbackPath))
            return Path.GetFullPath(fallbackPath);

        using var http = new HttpClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, fallbackUrl);
        using var response = http.Send(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        Directory.CreateDirectory(outputRoot);
        using var input = response.Content.ReadAsStream();
        using var output = File.Create(fallbackPath);
        input.CopyTo(output);

        return Path.GetFullPath(fallbackPath);
    }

    /// <summary>Returns common local outline fonts, ordered by platform-specific usefulness for this tour.</summary>
    /// <returns>Candidate font paths.</returns>
    private static IEnumerable<string> CandidateFonts()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return @"C:\Windows\Fonts\arial.ttf";
            yield return @"C:\Windows\Fonts\segoeui.ttf";
            yield break;
        }

        if (OperatingSystem.IsLinux())
        {
            yield return "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
            yield return "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf";
            yield break;
        }

        if (OperatingSystem.IsMacOS())
        {
            yield return "/System/Library/Fonts/Supplemental/Arial.ttf";
            yield return "/System/Library/Fonts/Supplemental/Times New Roman.ttf";
        }
    }

    /// <summary>Validates an explicit font path and returns it as an absolute path.</summary>
    /// <param name="path">The user-provided font path.</param>
    /// <returns>The absolute font path.</returns>
    private static string RequireFont(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("The requested font file does not exist.", fullPath);

        return fullPath;
    }
}
