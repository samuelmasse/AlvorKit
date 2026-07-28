using System.Collections.Immutable;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Composes disjoint exact sites into one immutable symbolic caller generation.</summary>
public static class LoadedSymbolicCallerComposer
{
    /// <summary>
    /// Rebuilds from the authoritative baseline and publishes no generation when validation fails.
    /// </summary>
    public static LoadedSymbolicComposition Compose(
        LoadedMethodBodySnapshot body,
        Guid moduleVersionId,
        int containingMethodToken,
        IEnumerable<LoadedOperationSiteDescriptor> sites,
        string constructedContext = "")
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(sites);
        ArgumentNullException.ThrowIfNull(constructedContext);
        if (moduleVersionId == Guid.Empty)
            throw new ArgumentException("A loaded module MVID is required.", nameof(moduleVersionId));
        if (containingMethodToken == 0)
            throw new ArgumentOutOfRangeException(nameof(containingMethodToken));

        var edits = LoadedSymbolicCompositionValidator.Validate(
            body,
            moduleVersionId,
            containingMethodToken,
            constructedContext,
            sites,
            out var rejections);
        if (!rejections.IsEmpty)
            return new(null, rejections);

        LoadedSymbolicEmitter.Emit(
            body,
            edits,
            constructedContext,
            out var instructions,
            out var relocations,
            out var ilMap,
            out var exceptionRegions);
        var sortedSites =
            edits.Select(edit => edit.Site).ToImmutableArray();
        var generation = new LoadedSymbolicMethodGeneration(
            LoadedSymbolicGenerationIdentity.Create(
                body,
                moduleVersionId,
                containingMethodToken,
                constructedContext,
                sortedSites),
            body.Identity,
            moduleVersionId,
            containingMethodToken,
            constructedContext,
            body.MaxStack,
            body.InitLocals,
            body.LocalSignatureToken,
            instructions,
            exceptionRegions,
            relocations,
            ilMap,
            sortedSites);
        return new(generation, []);
    }
}
