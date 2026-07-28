namespace AlvorKit.Mocking;

/// <summary>
/// Exposes non-executing metadata for one typed matcher.
/// </summary>
internal abstract class MockTypedMatcher(string description)
{
    /// <summary>Gets the non-executing diagnostic description.</summary>
    internal string Description { get; } = description;

    /// <summary>Matches a retained projection without invoking a live predicate.</summary>
    internal virtual bool MatchesProjected(object? value) => false;

}

/// <summary>
/// Retains one typed predicate delegate but never a live argument value.
/// </summary>
internal sealed class MockTypedMatcher<T>(
    RefPredicate<T> predicate,
    string description,
    Func<object?, bool>? projectedPredicate = null) :
    MockTypedMatcher(description)
    where T : allows ref struct
{
    /// <summary>Evaluates the predicate synchronously against one live value.</summary>
    internal bool Matches(scoped in T value) => predicate(in value);

    /// <inheritdoc />
    internal override bool MatchesProjected(object? value) =>
        projectedPredicate?.Invoke(value) == true;

}
