namespace AlvorKit;

public class RootFileRoots(ReadOnlySpan<string> roots)
{
    private readonly string[] roots = [.. roots];

    public ReadOnlySpan<string> Span => roots;
}
