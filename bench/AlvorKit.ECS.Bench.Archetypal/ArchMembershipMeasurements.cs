namespace AlvorKit;

[Bench]
public class ArchMembershipMeasurements
{
    private object? retained;

    public BenchResult IndexOfPresentFirst(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIndexOfPresentFirst.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IndexOfPresentRotating(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIndexOfPresentRotating.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IndexOfAbsentInterior(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIndexOfAbsentInterior.Run(fieldIds, interiorFieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IndexOfAbsentHigh(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIndexOfAbsentHigh.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult BinaryPresentFirst(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipBinaryPresentFirst.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult BinaryPresentRotating(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipBinaryPresentRotating.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult BinaryAbsentInterior(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipBinaryAbsentInterior.Run(fieldIds, interiorFieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult BinaryAbsentHigh(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipBinaryAbsentHigh.Run(fieldIds, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult OrdinalHashPresentFirst(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipOrdinalHashPresentFirst.Run(fieldIds, indexSlots, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult OrdinalHashPresentRotating(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipOrdinalHashPresentRotating.Run(fieldIds, indexSlots, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult OrdinalHashAbsentInterior(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipOrdinalHashAbsentInterior.Run(fieldIds, interiorFieldIds, indexSlots, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult OrdinalHashAbsentHigh(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipOrdinalHashAbsentHigh.Run(fieldIds, indexSlots, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IdealDirectPresentFirst(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIdealDirectPresentFirst.Run(fieldIds, directOrdinals, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IdealDirectPresentRotating(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIdealDirectPresentRotating.Run(fieldIds, directOrdinals, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IdealDirectAbsentInterior(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIdealDirectAbsentInterior.Run(fieldIds, interiorFieldIds, directOrdinals, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }

    public BenchResult IdealDirectAbsentHigh(int width)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);

        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[ArchMembershipApproaches.OrdinalHashCapacity(fieldIds.Length)];
        ArchMembershipApproaches.BuildOrdinalHash(fieldIds, indexSlots);
        var directOrdinals = new int[fieldIds[^1] + 2];

        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;
        var timer = BenchTimer.Start();
        var observed = MembershipIdealDirectAbsentHigh.Run(fieldIds, directOrdinals, 1048576);
        var result = timer.Stop(1048576, "lookup");
        retained = observed;
        return result;
    }
}
