namespace AlvorKit.Script.Bindgen;

/// <summary>Decides whether an enum should be emitted with the <see cref="FlagsAttribute"/>.</summary>
internal static class CHeaderFlagsHeuristic
{
    /// <summary>Returns true when configuration or member values indicate bit flags.</summary>
    public static bool ShouldEmit(BindgenConfig config, string nativeName, List<BindingEnumMember> members)
    {
        if (config.FlagsEnums.Contains(nativeName))
            return true;

        var nonZeroValues = members.Select(member => member.Value).Where(value => value > 0).Distinct().ToList();
        var powerOfTwoValues = nonZeroValues.Where(IsPowerOfTwo).Distinct().ToList();
        if (powerOfTwoValues.Count < 3 || powerOfTwoValues.Count < nonZeroValues.Count * 0.6 || nonZeroValues.Max() <= nonZeroValues.Count)
            return false;

        var combinedBits = powerOfTwoValues.Aggregate(0L, (bits, value) => bits | value);
        return nonZeroValues.All(value => (value & ~combinedBits) == 0);
    }

    /// <summary>Returns true when a value has exactly one bit set.</summary>
    private static bool IsPowerOfTwo(long value) => (value & (value - 1)) == 0;
}
