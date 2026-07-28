namespace AlvorKit.Script.TestInterception;

/// <summary>Separates launcher options from arguments forwarded after <c>--</c>.</summary>
internal static class InterceptionCommandLine
{
    /// <summary>Splits one command line without interpreting child arguments.</summary>
    internal static (string[] Launcher, string[] Child) Split(
        IReadOnlyList<string> arguments)
    {
        var separator = -1;
        for (var index = 0; index < arguments.Count; index++)
        {
            if (arguments[index] == "--")
            {
                separator = index;
                break;
            }
        }

        if (separator < 0)
            return ([.. arguments], []);

        return (
            [.. arguments.Take(separator)],
            [.. arguments.Skip(separator + 1)]);
    }
}
