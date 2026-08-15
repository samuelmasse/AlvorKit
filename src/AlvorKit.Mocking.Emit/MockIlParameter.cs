using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Describes shared parameter facts required while emitting typed dispatch IL.
/// </summary>
internal readonly record struct MockIlParameter(
    Type Type,
    bool IsIn,
    bool IsOut)
{
    /// <summary>Creates emitted-IL shapes from reflected parameters.</summary>
    internal static MockIlParameter[] Create(ParameterInfo[] parameters)
    {
        var result = new MockIlParameter[parameters.Length];
        for (int index = 0; index < parameters.Length; index++)
        {
            ParameterInfo parameter = parameters[index];
            result[index] = new(
                parameter.ParameterType,
                parameter.IsIn,
                parameter.IsOut);
        }

        return result;
    }

    /// <summary>Creates the stable declared-to-carrier map for emitted parameters.</summary>
    internal static ImmutableArray<int> CreateCarrierIndices(
        IReadOnlyList<MockIlParameter> parameters)
    {
        var result = new int[parameters.Count];
        for (int index = 0; index < parameters.Count; index++)
            result[index] = index;

        return ImmutableArray.Create(result);
    }
}
