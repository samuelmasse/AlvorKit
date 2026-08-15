namespace AlvorKit;

/// <summary>Validates host configuration, authentication tokens, and arbitrary-execution requests.</summary>
internal static class LiveCodeHostRequestGuard
{
    /// <summary>Rejects host options that cannot produce a bounded loopback session.</summary>
    /// <param name="options">Host configuration to validate.</param>
    internal static void Validate(LiveCodeHostOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Name))
            throw new ArgumentException("LiveCode host name cannot be empty.", nameof(options));
        if (options.Port is < 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(options), "LiveCode port must be between 0 and 65535.");
        if (options.MaximumAssemblyBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum assembly bytes must be positive.");
        if (options.MaximumBridgePayloadBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum bridge payload bytes must be positive.");
        if (options.MaximumBridgeOperations <= 0)
            throw new ArgumentOutOfRangeException(nameof(options), "Maximum bridge operations must be positive.");
    }

    /// <summary>Compares a supplied token to the session token without leaking a matching prefix.</summary>
    /// <param name="session">Started session containing the expected token.</param>
    /// <param name="supplied">Token supplied by the client.</param>
    /// <returns><see langword="true"/> when the complete tokens match.</returns>
    internal static bool IsAuthorized(LiveCodeSessionManifest session, string supplied)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(session.Token);
        var suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        return expectedBytes.Length == suppliedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
    }

    /// <summary>Returns an execution rejection or <see langword="null"/> when the request is allowed.</summary>
    /// <param name="options">Host limits and capability switches.</param>
    /// <param name="request">Wire request to validate.</param>
    /// <returns>A client-facing rejection message, or <see langword="null"/>.</returns>
    internal static string? ValidateExecution(
        LiveCodeHostOptions options,
        LiveCodeWireRequest request)
    {
        if (!options.EnableCodeExecution)
            return "Arbitrary C# execution is disabled for this LiveCode session.";
        if (string.IsNullOrWhiteSpace(request.EntryType) || request.Assembly is null)
            return "LiveCode execution requires an entry type and compiled assembly.";
        if (request.Assembly.Length > options.MaximumAssemblyBytes)
            return $"Compiled assembly exceeds {options.MaximumAssemblyBytes} bytes.";
        if (request.Symbols?.Length > options.MaximumAssemblyBytes)
            return $"Compiled symbols exceed {options.MaximumAssemblyBytes} bytes.";
        return null;
    }
}
