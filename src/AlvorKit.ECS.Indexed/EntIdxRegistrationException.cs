namespace AlvorKit.ECS.Indexed;

/// <summary>Reports an invalid indexed ECS hook or bag registration.</summary>
public class EntIdxRegistrationException(string message) : Exception(message);

