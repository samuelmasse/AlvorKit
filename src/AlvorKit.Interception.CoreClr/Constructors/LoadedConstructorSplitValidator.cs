using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Validates control-flow and exception-clause ownership at a constructor split.</summary>
internal static class LoadedConstructorSplitValidator
{
    /// <summary>
    /// Rejects cross-boundary branches and partitions exception clauses that remain whole.
    /// </summary>
    internal static void Validate(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ILoadedConstructorMetadataResolver metadata,
        ImmutableArray<LoadedExceptionRegion>.Builder preserved,
        ImmutableArray<LoadedExceptionRegion>.Builder moved,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        RejectCrossBoundaryBranches(body, splitOffset, rejections);
        RejectPrefixCycles(body, splitOffset, rejections);
        LoadedConstructorStackValidator.Validate(
            body,
            splitOffset,
            metadata,
            rejections);
        RejectCrossBoundaryLocals(body, splitOffset, rejections);
        LoadedConstructorExceptionPartitioner.Partition(
            body,
            splitOffset,
            preserved,
            moved,
            rejections);
    }

    /// <summary>Rejects retained back-edges so the initializer is reached at most once.</summary>
    private static void RejectPrefixCycles(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        foreach (LoadedIlInstruction instruction in body.Instructions)
        {
            if (instruction.BaselineOffset >= splitOffset)
                continue;
            foreach (int target in instruction.Operand.BranchTargets)
            {
                if (target >= splitOffset ||
                    target > instruction.BaselineOffset)
                {
                    continue;
                }
                rejections.Add(
                    new(
                        LoadedConstructorRemainderRejectionReason
                            .PrefixControlFlowCycle,
                        instruction.BaselineOffset,
                        target,
                        $"Retained branch at " +
                        $"{Offset(instruction.BaselineOffset)} targets " +
                        $"{Offset(target)} and prevents proof that the " +
                        "constructor initializer executes exactly once."));
            }
        }
    }

    /// <summary>Rejects local storage whose lifetime crosses the extracted boundary.</summary>
    private static void RejectCrossBoundaryLocals(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        var preserved = body.Instructions
            .Where(instruction => instruction.BaselineOffset < splitOffset)
            .Select(LocalIndex)
            .Where(index => index >= 0)
            .ToHashSet();
        var moved = body.Instructions
            .Where(instruction => instruction.BaselineOffset >= splitOffset)
            .Select(LocalIndex)
            .Where(index => index >= 0)
            .ToHashSet();
        foreach (var index in preserved.Intersect(moved).Order())
        {
            rejections.Add(
                new(
                    LoadedConstructorRemainderRejectionReason
                        .CrossBoundaryLocal,
                    splitOffset,
                    index,
                    $"Local {index} is used before and after constructor " +
                    $"initializer split {Offset(splitOffset)} and cannot be " +
                    "captured by an independently callable remainder."));
        }
    }

    /// <summary>Returns one referenced local index, or minus one.</summary>
    private static int LocalIndex(LoadedIlInstruction instruction)
    {
        string? name = instruction.OpCode.Name;
        if (name is null ||
            (!name.StartsWith("ldloc", StringComparison.Ordinal) &&
             !name.StartsWith("stloc", StringComparison.Ordinal)))
        {
            return -1;
        }

        if (instruction.Operand.Kind == LoadedIlOperandKind.VariableIndex)
            return ((int)instruction.Operand.IntegerValue);
        int separator = name.LastIndexOf('.');
        return separator >= 0 &&
            int.TryParse(name.AsSpan(separator + 1), out var index)
                ? index
                : -1;
    }

    /// <summary>Rejects any explicit branch whose source and target straddle the split.</summary>
    private static void RejectCrossBoundaryBranches(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        foreach (var instruction in body.Instructions)
        {
            foreach (var target in instruction.Operand.BranchTargets)
            {
                if ((instruction.BaselineOffset < splitOffset) ==
                    (target < splitOffset))
                {
                    continue;
                }

                rejections.Add(
                    new(
                        LoadedConstructorRemainderRejectionReason
                            .CrossBoundaryBranch,
                        instruction.BaselineOffset,
                        target,
                        $"Branch at {Offset(instruction.BaselineOffset)} " +
                        $"targets {Offset(target)} across constructor " +
                        $"initializer split {Offset(splitOffset)}."));
            }
        }
    }

    /// <summary>Formats one baseline coordinate.</summary>
    private static string Offset(int offset) => $"IL_{offset:X4}";
}
