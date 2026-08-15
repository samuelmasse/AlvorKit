namespace AlvorKit;

/// <summary>
/// Atomically claims configured return values in order and repeats the final
/// value after exhaustion.
/// </summary>
internal sealed class MockReturnSequenceBehavior : MockConfiguredBehavior
{
    private static readonly object?[] NoReferenceValues = [];
    private readonly object?[] values;
    private int nextIndex;

    /// <summary>Creates sequence behavior from an owned copy of configured values.</summary>
    internal MockReturnSequenceBehavior(ReadOnlySpan<object?> values)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException(
                "A return sequence must contain at least one value.",
                nameof(values));
        }

        this.values = values.ToArray();
    }

    /// <inheritdoc />
    internal override MockBehaviorExecution Claim()
    {
        var index = ClaimIndex();
        return new(
            MockBehaviorExecutionKind.Return,
            values[index],
            NoReferenceValues,
            null);
    }

    /// <summary>
    /// Claims the next sequence index, saturating at the final index so the
    /// counter cannot overflow after exhaustion.
    /// </summary>
    private int ClaimIndex()
    {
        while (true)
        {
            var current = Volatile.Read(ref nextIndex);
            if (current >= values.Length - 1)
                return values.Length - 1;

            if (Interlocked.CompareExchange(
                    ref nextIndex,
                    current + 1,
                    current) == current)
            {
                return current;
            }
        }
    }
}
