using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Proves the constructor initializer leaves an empty evaluation stack.</summary>
internal static class LoadedConstructorStackValidator
{
    /// <summary>Rejects a split whose incoming remainder stack is nonempty or unprovable.</summary>
    internal static void Validate(
        LoadedMethodBodySnapshot body,
        int splitOffset,
        ILoadedConstructorMetadataResolver metadata,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        var prefix = body.Instructions
            .Where(instruction => instruction.BaselineOffset < splitOffset)
            .ToDictionary(instruction => instruction.BaselineOffset);
        if (prefix.Count == 0)
        {
            Reject(
                0,
                splitOffset,
                "The constructor prefix has no entry instruction.",
                rejections);
            return;
        }

        var depths = new Dictionary<int, int>();
        var pending = new Queue<int>();
        Enqueue(body.Instructions[0].BaselineOffset, 0, depths, pending, rejections);
        foreach (LoadedExceptionRegion region in body.ExceptionRegions)
        {
            if (region.HandlerOffset < splitOffset)
            {
                int handlerDepth =
                    region.Kind is LoadedExceptionRegionKind.Catch or
                        LoadedExceptionRegionKind.Filter
                        ? 1
                        : 0;
                Enqueue(
                    region.HandlerOffset,
                    handlerDepth,
                    depths,
                    pending,
                    rejections);
            }
            if (region.FilterOffset >= 0 && region.FilterOffset < splitOffset)
            {
                Enqueue(
                    region.FilterOffset,
                    1,
                    depths,
                    pending,
                    rejections);
            }
        }

        int? boundaryDepth = null;
        while (pending.Count > 0)
        {
            int offset = pending.Dequeue();
            if (!prefix.TryGetValue(offset, out var instruction))
            {
                Reject(
                    offset,
                    splitOffset,
                    $"Control flow reaches non-instruction {Offset(offset)}.",
                    rejections);
                continue;
            }

            int inputDepth = depths[offset];
            if (!LoadedConstructorStackEffect.TryApply(
                    instruction,
                    inputDepth,
                    metadata,
                    out int outputDepth,
                    out string? detail))
            {
                Reject(
                    instruction.BaselineOffset,
                    splitOffset,
                    detail!,
                    rejections);
                continue;
            }

            foreach (int successor in Successors(instruction))
            {
                if (successor == splitOffset)
                {
                    if (boundaryDepth is null)
                        boundaryDepth = outputDepth;
                    else if (boundaryDepth.Value != outputDepth)
                    {
                        Reject(
                            instruction.BaselineOffset,
                            splitOffset,
                            "Control-flow paths disagree on the evaluation-stack " +
                            $"depth at {Offset(splitOffset)}.",
                            rejections);
                    }
                    continue;
                }
                if (successor < splitOffset)
                {
                    Enqueue(
                        successor,
                        outputDepth,
                        depths,
                        pending,
                        rejections);
                }
            }
        }

        if (boundaryDepth is null)
        {
            Reject(
                splitOffset,
                splitOffset,
                $"No verified control-flow path reaches {Offset(splitOffset)}.",
                rejections);
        }
        else if (boundaryDepth.Value != 0)
        {
            Reject(
                splitOffset,
                splitOffset,
                $"Constructor initializer split {Offset(splitOffset)} retains " +
                $"{boundaryDepth.Value} evaluation-stack value(s); the moved " +
                "remainder is not independently callable.",
                rejections);
        }
    }

    private static IEnumerable<int> Successors(
        LoadedIlInstruction instruction)
    {
        return instruction.OpCode.FlowControl switch
        {
            FlowControl.Branch => instruction.Operand.BranchTargets,
            FlowControl.Cond_Branch => instruction.Operand.BranchTargets
                                .Append(instruction.NextBaselineOffset),
            FlowControl.Return or FlowControl.Throw => [],
            _ => [instruction.NextBaselineOffset],
        };
    }

    private static void Enqueue(
        int offset,
        int depth,
        Dictionary<int, int> depths,
        Queue<int> pending,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections)
    {
        if (depths.TryGetValue(offset, out int current))
        {
            if (current != depth)
            {
                Reject(
                    offset,
                    offset,
                    $"Control-flow paths disagree on evaluation-stack depth " +
                    $"at {Offset(offset)} ({current} versus {depth}).",
                    rejections);
            }
            return;
        }

        depths.Add(offset, depth);
        pending.Enqueue(offset);
    }

    private static void Reject(
        int baselineOffset,
        int splitOffset,
        string detail,
        ImmutableArray<LoadedConstructorRemainderRejection>.Builder rejections) =>
        rejections.Add(
            new(
                LoadedConstructorRemainderRejectionReason
                    .NonEmptyEvaluationStack,
                baselineOffset,
                splitOffset,
                detail));

    private static string Offset(int offset) => $"IL_{offset:X4}";
}
