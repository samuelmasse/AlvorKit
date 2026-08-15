using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Plans one safe post-initializer constructor remainder from loaded IL.</summary>
public static class LoadedConstructorRemainderPlanner
{
    /// <summary>The stable encoded value of the direct <c>call</c> opcode.</summary>
    private const ushort CallOpCode = 0x0028;

    /// <summary>
    /// Identifies one exact direct-base or delegating-this call and partitions its immutable body.
    /// </summary>
    public static LoadedConstructorRemainderPlanning Plan(
        LoadedMethodBodySnapshot body,
        ILoadedConstructorMetadataResolver metadata)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(metadata);

        var candidates = ImmutableArray.CreateBuilder<InitializerCandidate>();
        var rejections =
            ImmutableArray.CreateBuilder<LoadedConstructorRemainderRejection>();
        for (var index = 0; index < body.Instructions.Length; ++index)
        {
            var instruction = body.Instructions[index];
            if (instruction.OpCodeValue != CallOpCode ||
                instruction.Operand.Kind != LoadedIlOperandKind.MetadataToken)
            {
                continue;
            }

            var token = ((int)instruction.Operand.IntegerValue);
            if (!metadata.TryResolveInitializerKind(token, out var kind))
                continue;

            if (!metadata.TryResolveMethod(token, out var method) ||
                !method.HasThis ||
                !method.IsConstructor)
            {
                rejections.Add(
                    new(
                        LoadedConstructorRemainderRejectionReason
                            .InvalidInitializerMetadata,
                        instruction.BaselineOffset,
                        instruction.BaselineOffset,
                        $"Initializer token 0x{token:X8} at " +
                        $"{Offset(instruction.BaselineOffset)} did not resolve " +
                        "to an exact instance constructor."));
                continue;
            }

            candidates.Add(
                new(
                    index,
                    instruction,
                    token,
                    kind!.Value,
                    method.CanonicalSignature));
        }

        if (rejections.Count > 0)
            return Rejected(rejections);
        if (candidates.Count != 1)
            return RejectInitializerCount(candidates);

        var candidate = candidates[0];
        var splitOffset = candidate.Instruction.NextBaselineOffset;
        if (candidate.Index == body.Instructions.Length - 1)
        {
            return Rejected(
                [
                    new(
                        LoadedConstructorRemainderRejectionReason
                            .MissingRemainder,
                        candidate.Instruction.BaselineOffset,
                        splitOffset,
                        $"Initializer at " +
                        $"{Offset(candidate.Instruction.BaselineOffset)} has " +
                        "no post-initializer constructor remainder.")
                ]);
        }

        var preservedExceptions =
            ImmutableArray.CreateBuilder<LoadedExceptionRegion>();
        var movedExceptions =
            ImmutableArray.CreateBuilder<LoadedExceptionRegion>();
        LoadedConstructorSplitValidator.Validate(
            body,
            splitOffset,
            metadata,
            preservedExceptions,
            movedExceptions,
            rejections);
        if (rejections.Count > 0)
            return Rejected(rejections);

        var preservedInstructions = body.Instructions
            .Take(candidate.Index + 1)
            .ToImmutableArray();
        var movedInstructions = body.Instructions
            .Skip(candidate.Index + 1)
            .ToImmutableArray();
        return new(
            new LoadedConstructorRemainderPlan(
                body.Identity,
                candidate.Kind,
                candidate.Instruction.BaselineOffset,
                candidate.MetadataToken,
                candidate.CanonicalSignature,
                new(
                    0,
                    splitOffset,
                    preservedInstructions),
                new(
                    splitOffset,
                    body.CodeSize,
                    movedInstructions),
                preservedExceptions.ToImmutable(),
                movedExceptions.ToImmutable()),
            []);
    }

    /// <summary>Creates the one deterministic candidate-count rejection.</summary>
    private static LoadedConstructorRemainderPlanning RejectInitializerCount(
        ImmutableArray<InitializerCandidate>.Builder candidates)
    {
        var offsets = candidates.Count == 0
            ? "none"
            : string.Join(
                ", ",
                candidates.Select(candidate =>
                    Offset(candidate.Instruction.BaselineOffset)));
        var baselineOffset = candidates.Count == 0
            ? 0
            : candidates[0].Instruction.BaselineOffset;
        return Rejected(
            [
                new(
                    LoadedConstructorRemainderRejectionReason.InitializerCount,
                    baselineOffset,
                    baselineOffset,
                    "Expected exactly one direct-base or delegating-this " +
                    $"constructor call; found {candidates.Count} at {offsets}.")
            ]);
    }

    /// <summary>Sorts rejections into deterministic baseline diagnostic order.</summary>
    private static LoadedConstructorRemainderPlanning Rejected(
        IEnumerable<LoadedConstructorRemainderRejection> rejections) =>
        new(
            null,
            [.. rejections
                .OrderBy(rejection => rejection.BaselineOffset)
                .ThenBy(rejection => rejection.RelatedOffset)
                .ThenBy(rejection => rejection.Reason)
                .ThenBy(rejection => rejection.Detail, StringComparer.Ordinal)]);

    /// <summary>Formats one baseline coordinate.</summary>
    private static string Offset(int offset) => $"IL_{offset:X4}";

    /// <summary>Identifies one safe initializer call candidate.</summary>
    private sealed record InitializerCandidate(
        int Index,
        LoadedIlInstruction Instruction,
        int MetadataToken,
        LoadedConstructorInitializerKind Kind,
        string CanonicalSignature);
}
