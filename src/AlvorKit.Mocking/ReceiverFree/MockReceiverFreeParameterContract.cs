namespace AlvorKit.Mocking;

/// <summary>Validates exact delegate parameter metadata for an intercepted operation.</summary>
internal static class MockReceiverFreeParameterContract
{
    /// <summary>Validates the expected parameters after any synthetic delegate prefix.</summary>
    internal static void Validate(
        MockInterceptionSiteDescriptor site,
        ParameterInfo[] actual,
        ParameterInfo[] expected,
        int actualOffset)
    {
        if (actual.Length != expected.Length + actualOffset)
        {
            throw Failure(
                site,
                $"delegate has {actual.Length} parameters; expected " +
                $"{expected.Length + actualOffset}");
        }

        for (int index = 0; index < expected.Length; index++)
        {
            ParameterInfo left = actual[index + actualOffset];
            ParameterInfo right = expected[index];
            ValidateParameter(site, left, right, $"parameter {index}");
        }
    }

    /// <summary>Validates one parameter's type, direction, and custom modifiers.</summary>
    internal static void ValidateParameter(
        MockInterceptionSiteDescriptor site,
        ParameterInfo actual,
        ParameterInfo expected,
        string location)
    {
        if (actual.ParameterType != expected.ParameterType ||
            actual.IsIn != expected.IsIn ||
            actual.IsOut != expected.IsOut ||
            !actual.GetRequiredCustomModifiers().SequenceEqual(
                expected.GetRequiredCustomModifiers()) ||
            !actual.GetOptionalCustomModifiers().SequenceEqual(
                expected.GetOptionalCustomModifiers()))
        {
            throw Failure(
                site,
                $"delegate {location} does not preserve exact metadata");
        }
    }

    /// <summary>Creates a consistent delegate-contract failure.</summary>
    private static MockException Failure(
        MockInterceptionSiteDescriptor site,
        string detail) =>
        new($"Interception site '{site}' is invalid because {detail}.");
}
