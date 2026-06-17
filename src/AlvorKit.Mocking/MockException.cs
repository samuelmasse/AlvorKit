namespace AlvorKit.Mocking;

/// <summary>Exception thrown when a mock cannot be created, configured, or matched.</summary>
public class MockException(string message) : Exception(message);
