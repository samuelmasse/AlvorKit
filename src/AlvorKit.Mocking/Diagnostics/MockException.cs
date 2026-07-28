namespace AlvorKit.Mocking;

/// <summary>Exception thrown when a mock cannot be created, configured, or matched.</summary>
/// <param name="message">The actionable mocking failure message.</param>
public class MockException(string message) : Exception(message);
