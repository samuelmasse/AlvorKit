namespace AlvorKit.Mocking;

/// <summary>Argument matcher captured while setting up a mocked call.</summary>
internal record struct Matcher(MatcherType Type, object? Object);
