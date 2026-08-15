using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>Emits deterministic symbolic caller routes from validated disjoint edits.</summary>
internal static class LoadedSymbolicEmitter
{
    /// <summary>Emits the complete symbolic stream, relocations, IL map, and EH labels.</summary>
    internal static void Emit(
        LoadedMethodBodySnapshot body,
        ImmutableArray<LoadedSymbolicEdit> edits,
        string constructedContext,
        out ImmutableArray<LoadedSymbolicInstruction> instructions,
        out ImmutableArray<LoadedSymbolicRelocation> relocations,
        out ImmutableArray<LoadedSymbolicIlMapEntry> ilMap,
        out ImmutableArray<LoadedSymbolicExceptionRegion> exceptionRegions)
    {
        var emitted = ImmutableArray.CreateBuilder<LoadedSymbolicInstruction>();
        var relocationBuilder =
            ImmutableArray.CreateBuilder<LoadedSymbolicRelocation>();
        var map = ImmutableArray.CreateBuilder<LoadedSymbolicIlMapEntry>();
        var editsByStart = edits.ToDictionary(edit => edit.StartOffset);
        for (var index = 0; index < body.Instructions.Length;)
        {
            var baseline = body.Instructions[index];
            if (editsByStart.TryGetValue(
                    baseline.BaselineOffset,
                    out var edit))
            {
                LoadedSymbolicRouteEmitter.Emit(
                    edit,
                    constructedContext,
                    emitted,
                    relocationBuilder,
                    map);
                while (index < body.Instructions.Length &&
                    body.Instructions[index].BaselineOffset < edit.EndOffset)
                {
                    ++index;
                }
                continue;
            }

            var emittedIndex = emitted.Count;
            var label = Label(baseline.BaselineOffset);
            emitted.Add(new(
                LoadedSymbolicInstructionKind.Baseline,
                [label],
                baseline.BaselineOffset,
                baseline.OpCodeValue,
                baseline.Operand,
                Targets(baseline.Operand),
                "",
                ""));
            map.Add(new(baseline.BaselineOffset, emittedIndex, label));
            ++index;
        }

        emitted.Add(new(
            LoadedSymbolicInstructionKind.End,
            [EndLabel, Label(body.CodeSize)],
            body.CodeSize,
            0,
            LoadedIlOperand.None,
            [],
            "",
            ""));
        instructions = emitted.ToImmutable();
        relocations = relocationBuilder.ToImmutable();
        ilMap = map.ToImmutable();
        exceptionRegions =
        [
            .. body.ExceptionRegions.Select(region => new LoadedSymbolicExceptionRegion(
                region.Kind,
                region.RawFlags,
                Label(region.TryOffset),
                Label(region.TryOffset + region.TryLength),
                Label(region.HandlerOffset),
                Label(region.HandlerOffset + region.HandlerLength),
                region.Kind == LoadedExceptionRegionKind.Filter
                    ? Label(region.FilterOffset)
                    : "",
                region.CatchTypeToken))
        ];
    }

    /// <summary>Converts absolute baseline branch targets to symbolic labels.</summary>
    private static ImmutableArray<string> Targets(LoadedIlOperand operand) =>
        [.. operand.BranchTargets.Select(Label)];

    /// <summary>Formats one immutable baseline label.</summary>
    internal static string Label(int offset) =>
        offset < 0 ? "" : $"IL_{offset:X8}";

    /// <summary>The symbolic label for the exclusive end of the IL stream.</summary>
    internal const string EndLabel = "IL_END";
}
