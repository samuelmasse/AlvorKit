using System.Collections.Immutable;
using static AlvorKit.Interception.CoreClr.Advanced.LoadedOperationOpCodes;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Validates site freshness and disjoint edit regions before symbolic emission.</summary>
internal static class LoadedSymbolicCompositionValidator
{
    /// <summary>Returns sorted disjoint edits or pristine deterministic rejections.</summary>
    internal static ImmutableArray<LoadedSymbolicEdit> Validate(
        LoadedMethodBodySnapshot body,
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        IEnumerable<LoadedOperationSiteDescriptor> sites,
        out ImmutableArray<LoadedSymbolicCompositionRejection> rejections)
    {
        var instructions = body.Instructions
            .Select((instruction, index) => (instruction, index))
            .ToDictionary(
                pair => pair.instruction.BaselineOffset,
                pair => pair);
        var rejected =
            ImmutableArray.CreateBuilder<LoadedSymbolicCompositionRejection>();
        var edits = ImmutableArray.CreateBuilder<LoadedSymbolicEdit>();
        foreach (var site in sites
            .OrderBy(site => site.BaselineOffset)
            .ThenBy(site => site.StableId, StringComparer.Ordinal))
        {
            if (!body.Identity.Equals(site.BodyIdentity))
            {
                rejected.Add(Reject(
                    site,
                    LoadedSymbolicCompositionRejectionReason.StaleBodyIdentity,
                    site.BaselineOffset,
                    "site belongs to another authoritative loaded body"));
                continue;
            }
            if (site.ModuleVersionId != moduleVersionId ||
                site.ContainingMethodToken != containingMethodToken ||
                !StringComparer.Ordinal.Equals(
                    site.ConstructedContext,
                    constructedContext))
            {
                rejected.Add(Reject(
                    site,
                    LoadedSymbolicCompositionRejectionReason.StaleSiteIdentity,
                    site.BaselineOffset,
                    "site location or constructed context does not match the generation"));
                continue;
            }
            if (!instructions.TryGetValue(
                    site.BaselineOffset,
                    out var operationEntry) ||
                !OperationMatches(site, operationEntry.instruction))
            {
                rejected.Add(Reject(
                    site,
                    LoadedSymbolicCompositionRejectionReason.StaleOperation,
                    site.BaselineOffset,
                    "baseline operation opcode or metadata token changed"));
                continue;
            }

            var prefixes = Prefixes(body.Instructions, operationEntry.index);
            if (!PrefixesMatch(site.Prefixes, prefixes))
            {
                rejected.Add(Reject(
                    site,
                    LoadedSymbolicCompositionRejectionReason.StalePrefix,
                    prefixes.IsEmpty
                        ? site.BaselineOffset
                        : prefixes[0].BaselineOffset,
                    "accepted prefix sequence changed"));
                continue;
            }

            var recognized = new LoadedRecognizedOperation(
                site.Kind,
                site.MetadataToken,
                site.CanonicalSignature,
                site.Prefixes);
            var expectedId = LoadedOperationSiteIdentity.Create(
                moduleVersionId,
                containingMethodToken,
                constructedContext,
                body.Identity,
                operationEntry.instruction,
                recognized);
            if (!StringComparer.Ordinal.Equals(site.StableId, expectedId))
            {
                rejected.Add(Reject(
                    site,
                    LoadedSymbolicCompositionRejectionReason.StaleSiteIdentity,
                    site.BaselineOffset,
                    $"stable identity should be '{expectedId}'"));
                continue;
            }

            edits.Add(new(
                site,
                operationEntry.instruction,
                prefixes,
                prefixes.IsEmpty
                    ? site.BaselineOffset
                    : prefixes[0].BaselineOffset,
                operationEntry.instruction.NextBaselineOffset));
        }

        var sortedEdits = edits
            .OrderBy(edit => edit.StartOffset)
            .ThenBy(edit => edit.Site.StableId, StringComparer.Ordinal)
            .ToImmutableArray();
        LoadedSymbolicEdit? active = null;
        foreach (var current in sortedEdits)
        {
            if (active is null)
            {
                active = current;
                continue;
            }
            if (current.StartOffset >= active.EndOffset)
            {
                active = current;
                continue;
            }
            rejected.Add(new(
                LoadedSymbolicCompositionRejectionReason.OverlappingEdit,
                current.StartOffset,
                active.StartOffset,
                current.Site.StableId,
                $"Site '{current.Site.StableId}' edit " +
                $"[{current.StartOffset}, {current.EndOffset}) overlaps " +
                $"site '{active.Site.StableId}' edit " +
                $"[{active.StartOffset}, {active.EndOffset})."));
            if (current.EndOffset > active.EndOffset)
                active = current;
        }

        rejections =
        [
            .. rejected
                .OrderBy(rejection => rejection.BaselineOffset)
                .ThenBy(rejection => rejection.RelatedOffset)
                .ThenBy(rejection => rejection.Reason)
                .ThenBy(rejection => rejection.SiteId, StringComparer.Ordinal)
        ];
        return rejections.IsEmpty ? sortedEdits : [];
    }

    /// <summary>Gets contiguous prefix instructions owned by one operation.</summary>
    private static ImmutableArray<LoadedIlInstruction> Prefixes(
        ImmutableArray<LoadedIlInstruction> instructions,
        int operationIndex)
    {
        var start = operationIndex;
        while (start > 0 && instructions[start - 1].IsPrefix)
            --start;
        return instructions[start..operationIndex];
    }

    /// <summary>Checks exact operation opcode and unresolved token coordinates.</summary>
    private static bool OperationMatches(
        LoadedOperationSiteDescriptor site,
        LoadedIlInstruction operation) =>
        site.OpCodeValue == operation.OpCodeValue &&
        operation.Operand.Kind == LoadedIlOperandKind.MetadataToken &&
        site.MetadataToken == unchecked((int)operation.Operand.IntegerValue);

    /// <summary>Checks exact accepted prefix kinds, offsets, and constrained tokens.</summary>
    private static bool PrefixesMatch(
        ImmutableArray<LoadedOperationPrefixDescriptor> expected,
        ImmutableArray<LoadedIlInstruction> actual)
    {
        if (expected.Length != actual.Length)
            return false;
        for (var index = 0; index < expected.Length; ++index)
        {
            var descriptor = expected[index];
            var instruction = actual[index];
            var kind = instruction.OpCodeValue switch
            {
                VolatilePrefix => LoadedOperationPrefixKind.Volatile,
                ConstrainedPrefix => LoadedOperationPrefixKind.Constrained,
                _ => (LoadedOperationPrefixKind?)null
            };
            if (kind != descriptor.Kind ||
                instruction.BaselineOffset != descriptor.BaselineOffset)
            {
                return false;
            }
            if (kind == LoadedOperationPrefixKind.Constrained &&
                descriptor.MetadataToken !=
                    unchecked((int)instruction.Operand.IntegerValue))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>Creates one site-specific freshness rejection.</summary>
    private static LoadedSymbolicCompositionRejection Reject(
        LoadedOperationSiteDescriptor site,
        LoadedSymbolicCompositionRejectionReason reason,
        int relatedOffset,
        string detail) =>
        new(
            reason,
            site.BaselineOffset,
            relatedOffset,
            site.StableId,
            $"Cannot compose site '{site.StableId}' at " +
            $"IL_{site.BaselineOffset:X4}: {detail}.");
}
