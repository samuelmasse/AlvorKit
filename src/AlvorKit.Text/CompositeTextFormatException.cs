namespace AlvorKit.Text;

/// <summary>Creates consistent exceptions for invalid composite format strings.</summary>
internal static class CompositeTextFormatException
{
    /// <summary>Creates the standard invalid-format exception outside the formatting hot path.</summary>
    public static FormatException Create() =>
        new("Input string was not in a correct format.");
}
