namespace AlvorKit.LiveCode;

/// <summary>Reports a protocol or target rejection returned to a LiveCode client.</summary>
public class LiveCodeClientException(string message) : Exception(message);
