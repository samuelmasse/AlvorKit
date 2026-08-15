namespace AlvorKit;

/// <summary>
/// Assigns logical entry order and exposes only a contiguous published
/// checkpoint watermark.
/// </summary>
internal sealed class MockInvocationTimeline
{
    private static long nextTimelineId;
    private readonly Lock gate = new();
    private readonly HashSet<long> publishedOutOfOrder = [];
    private long nextSequence;
    private long publishedWatermark;

    /// <summary>Creates an independent mock-local invocation timeline.</summary>
    internal MockInvocationTimeline()
    {
        Id = Interlocked.Increment(ref nextTimelineId);
    }

    /// <summary>Gets the runtime-unique timeline ID.</summary>
    internal long Id { get; }

    /// <summary>Reserves a unique logical entry number.</summary>
    internal MockTimelineReservation Reserve() =>
        new(Id, Interlocked.Increment(ref nextSequence));

    /// <summary>
    /// Publishes a reservation after its invocation slot is visible and
    /// advances the contiguous checkpoint watermark when possible.
    /// </summary>
    internal void Publish(MockTimelineReservation reservation) =>
        FinishReservation(reservation);

    /// <summary>
    /// Cancels a reservation that failed before an invocation was appended so
    /// later checkpoints are not blocked by a permanent sequence gap.
    /// </summary>
    internal void Cancel(MockTimelineReservation reservation) =>
        FinishReservation(reservation);

    /// <summary>Captures the last contiguously published logical entry.</summary>
    internal MockInvocationCoordinate Checkpoint() =>
        new(Id, Volatile.Read(ref publishedWatermark));

    private void FinishReservation(MockTimelineReservation reservation)
    {
        if (reservation.TimelineId != Id)
            throw new ArgumentException("The reservation belongs to another timeline.", nameof(reservation));
        if (reservation.Sequence <= 0 || reservation.Sequence > Volatile.Read(ref nextSequence))
            throw new ArgumentOutOfRangeException(nameof(reservation));

        lock (gate)
        {
            if (reservation.Sequence <= publishedWatermark ||
                !publishedOutOfOrder.Add(reservation.Sequence))
            {
                throw new InvalidOperationException(
                    $"Timeline reservation {reservation.Sequence} was already finished.");
            }

            while (publishedOutOfOrder.Remove(publishedWatermark + 1))
                publishedWatermark++;

            Volatile.Write(ref publishedWatermark, publishedWatermark);
        }
    }
}
