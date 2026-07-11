namespace AlvorKit.ECS.Demo.Bench;

internal sealed partial class EcsArchBenchWorker
{
    private const int NoFieldOrdinal = -1;
    private const uint OrdinalHashMultiplier = 0x9E3779B9U;

    private EcsArchBenchSample RunMembership(string scenarioId)
    {
        int width = int.Parse(scenarioId.AsSpan(^2), CultureInfo.InvariantCulture);
        MembershipAlgorithm algorithm = scenarioId switch
        {
            _ when scenarioId.StartsWith("arch-membership-indexof-", StringComparison.Ordinal) => MembershipAlgorithm.IndexOf,
            _ when scenarioId.StartsWith("arch-membership-binary-", StringComparison.Ordinal) => MembershipAlgorithm.Binary,
            _ when scenarioId.StartsWith("arch-membership-ordinalhash-", StringComparison.Ordinal) => MembershipAlgorithm.OrdinalHash,
            _ => MembershipAlgorithm.IdealDirect,
        };
        MembershipProbe probe = scenarioId switch
        {
            _ when scenarioId.Contains("-present-first-", StringComparison.Ordinal) => MembershipProbe.PresentFirst,
            _ when scenarioId.Contains("-present-rotating-", StringComparison.Ordinal) => MembershipProbe.PresentRotating,
            _ when scenarioId.Contains("-absent-interior-", StringComparison.Ordinal) => MembershipProbe.AbsentInterior,
            _ => MembershipProbe.AbsentHigh,
        };

        return Measure<RunArch>(scenarioId, () => CreateMembershipCase(width, algorithm, probe));
    }

    private EcsArchBenchCase CreateMembershipCase(
        int width,
        MembershipAlgorithm algorithm,
        MembershipProbe probe)
    {
        var fieldIds = new int[width];
        var interiorFieldIds = new int[width];
        int interiorCount = Math.Max(1, width - 1);
        for (int fieldIndex = 0; fieldIndex < fieldIds.Length; fieldIndex++)
        {
            fieldIds[fieldIndex] = fieldIndex * 2;
            interiorFieldIds[fieldIndex] = ((fieldIndex % interiorCount) << 1) + 1;
        }

        var indexSlots = new int[OrdinalHashCapacity(fieldIds.Length)];
        BuildOrdinalHash(fieldIds, indexSlots);

        var directOrdinals = new int[fieldIds[^1] + 2];
        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
            directOrdinals[fieldIds[ordinal]] = ordinal + 1;

        Action body = (algorithm, probe) switch
        {
            (MembershipAlgorithm.IndexOf, MembershipProbe.PresentFirst) => IndexOfPresentFirst,
            (MembershipAlgorithm.IndexOf, MembershipProbe.PresentRotating) => IndexOfPresentRotating,
            (MembershipAlgorithm.IndexOf, MembershipProbe.AbsentInterior) => IndexOfAbsentInterior,
            (MembershipAlgorithm.IndexOf, MembershipProbe.AbsentHigh) => IndexOfAbsentHigh,
            (MembershipAlgorithm.Binary, MembershipProbe.PresentFirst) => BinaryPresentFirst,
            (MembershipAlgorithm.Binary, MembershipProbe.PresentRotating) => BinaryPresentRotating,
            (MembershipAlgorithm.Binary, MembershipProbe.AbsentInterior) => BinaryAbsentInterior,
            (MembershipAlgorithm.Binary, MembershipProbe.AbsentHigh) => BinaryAbsentHigh,
            (MembershipAlgorithm.OrdinalHash, MembershipProbe.PresentFirst) => OrdinalHashPresentFirst,
            (MembershipAlgorithm.OrdinalHash, MembershipProbe.PresentRotating) => OrdinalHashPresentRotating,
            (MembershipAlgorithm.OrdinalHash, MembershipProbe.AbsentInterior) => OrdinalHashAbsentInterior,
            (MembershipAlgorithm.OrdinalHash, MembershipProbe.AbsentHigh) => OrdinalHashAbsentHigh,
            (MembershipAlgorithm.IdealDirect, MembershipProbe.PresentFirst) => IdealDirectPresentFirst,
            (MembershipAlgorithm.IdealDirect, MembershipProbe.PresentRotating) => IdealDirectPresentRotating,
            (MembershipAlgorithm.IdealDirect, MembershipProbe.AbsentInterior) => IdealDirectAbsentInterior,
            _ => IdealDirectAbsentHigh,
        };

        return new("lookup", options.Operations, body, fieldIds, true);

        void IndexOfPresentFirst()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int fieldId = signature[0];
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindIndexOf(signature, fieldId);
            longSink = sum;
        }

        void IndexOfPresentRotating()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = signature[i & mask];
                sum += FindIndexOf(signature, fieldId);
            }
            longSink = sum;
        }

        void IndexOfAbsentInterior()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = interiorFieldIds[i & mask];
                sum += FindIndexOf(signature, fieldId);
            }
            longSink = sum;
        }

        void IndexOfAbsentHigh()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int fieldId = signature[^1] + 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindIndexOf(signature, fieldId);
            longSink = sum;
        }

        void BinaryPresentFirst()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int fieldId = signature[0];
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindBinary(signature, fieldId);
            longSink = sum;
        }

        void BinaryPresentRotating()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = signature[i & mask];
                sum += FindBinary(signature, fieldId);
            }
            longSink = sum;
        }

        void BinaryAbsentInterior()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = interiorFieldIds[i & mask];
                sum += FindBinary(signature, fieldId);
            }
            longSink = sum;
        }

        void BinaryAbsentHigh()
        {
            ReadOnlySpan<int> signature = fieldIds;
            int fieldId = signature[^1] + 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindBinary(signature, fieldId);
            longSink = sum;
        }

        void OrdinalHashPresentFirst()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> slots = indexSlots;
            int fieldId = signature[0];
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindOrdinalHash(signature, slots, fieldId);
            longSink = sum;
        }

        void OrdinalHashPresentRotating()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> slots = indexSlots;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = signature[i & mask];
                sum += FindOrdinalHash(signature, slots, fieldId);
            }
            longSink = sum;
        }

        void OrdinalHashAbsentInterior()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> slots = indexSlots;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = interiorFieldIds[i & mask];
                sum += FindOrdinalHash(signature, slots, fieldId);
            }
            longSink = sum;
        }

        void OrdinalHashAbsentHigh()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> slots = indexSlots;
            int fieldId = signature[^1] + 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindOrdinalHash(signature, slots, fieldId);
            longSink = sum;
        }

        void IdealDirectPresentFirst()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> ordinals = directOrdinals;
            int fieldId = signature[0];
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindIdealDirect(ordinals, fieldId);
            longSink = sum;
        }

        void IdealDirectPresentRotating()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> ordinals = directOrdinals;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = signature[i & mask];
                sum += FindIdealDirect(ordinals, fieldId);
            }
            longSink = sum;
        }

        void IdealDirectAbsentInterior()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> ordinals = directOrdinals;
            int mask = signature.Length - 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
            {
                int fieldId = interiorFieldIds[i & mask];
                sum += FindIdealDirect(ordinals, fieldId);
            }
            longSink = sum;
        }

        void IdealDirectAbsentHigh()
        {
            ReadOnlySpan<int> signature = fieldIds;
            ReadOnlySpan<int> ordinals = directOrdinals;
            int fieldId = signature[^1] + 1;
            long sum = 0;
            for (int i = 0; i < options.Operations; i++)
                sum += FindIdealDirect(ordinals, fieldId);
            longSink = sum;
        }
    }

    private static int FindIndexOf(ReadOnlySpan<int> fieldIds, int fieldId) =>
        fieldIds.IndexOf(fieldId);

    private static int FindIdealDirect(ReadOnlySpan<int> ordinals, int fieldId) =>
        ordinals[fieldId] - 1;

    private static int OrdinalHashCapacity(int fieldCount) =>
        (int)BitOperations.RoundUpToPowerOf2((uint)(fieldCount * 2));

    private static void BuildOrdinalHash(ReadOnlySpan<int> fieldIds, Span<int> slots)
    {
        int mask = slots.Length - 1;
        for (int ordinal = 0; ordinal < fieldIds.Length; ordinal++)
        {
            int slot = OrdinalHashSlot(fieldIds[ordinal], mask);
            while (slots[slot] != 0)
                slot = (slot + 1) & mask;

            slots[slot] = ordinal + 1;
        }
    }

    private static int FindOrdinalHash(ReadOnlySpan<int> fieldIds, ReadOnlySpan<int> slots, int fieldId)
    {
        int mask = slots.Length - 1;
        int slot = OrdinalHashSlot(fieldId, mask);

        while (true)
        {
            int encodedOrdinal = slots[slot];
            if (encodedOrdinal == 0)
                return NoFieldOrdinal;

            int ordinal = encodedOrdinal - 1;
            if (fieldIds[ordinal] == fieldId)
                return ordinal;

            slot = (slot + 1) & mask;
        }
    }

    private static int OrdinalHashSlot(int fieldId, int mask)
    {
        uint hash = unchecked((uint)fieldId * OrdinalHashMultiplier);
        return unchecked((int)(hash ^ (hash >> 16))) & mask;
    }

    private static int FindBinary(ReadOnlySpan<int> fieldIds, int fieldId)
    {
        int low = 0;
        int high = fieldIds.Length - 1;

        while (low <= high)
        {
            int middle = (low + high) >> 1;
            int candidate = fieldIds[middle];
            if (candidate < fieldId)
                low = middle + 1;
            else if (candidate > fieldId)
                high = middle - 1;
            else
                return middle;
        }

        return NoFieldOrdinal;
    }

    private enum MembershipAlgorithm
    {
        IndexOf,
        Binary,
        OrdinalHash,
        IdealDirect,
    }

    private enum MembershipProbe
    {
        PresentFirst,
        PresentRotating,
        AbsentInterior,
        AbsentHigh,
    }
}
