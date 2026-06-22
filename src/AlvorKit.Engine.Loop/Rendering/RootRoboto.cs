namespace AlvorKit.Engine.Loop;

/// <summary>Root-owned embedded Roboto Mono font used by demos and games that want a built-in monospace face.</summary>
[Root]
[ExcludeFromCodeCoverage]
public sealed class RootRoboto
{
    private const string ResourceName = "AlvorKit.Engine.Loop.res.fonts.RobotoMono-Regular.ttf";

    private readonly Font font;

    /// <summary>Creates the font from the embedded Roboto Mono resource.</summary>
    public RootRoboto(RootFonts fonts)
    {
        var assembly = typeof(RootRoboto).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException("Embedded Roboto Mono font resource is missing.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        font = fonts.Open(new() { Data = memory.ToArray() });
    }

    /// <summary>Gets or creates the requested pixel size.</summary>
    public FontSize this[int size] => font.Size(size);

    /// <summary>Gets the underlying font.</summary>
    public Font Font => font;
}
