using System.Collections.Immutable;
using static AlvorKit.Interception.CoreClr.Advanced.LoadedOperationOpCodes;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>
/// Recognizes exact caller operations from an immutable authoritative loaded-body baseline.
/// </summary>
public static class LoadedOperationRecognizer
{
    /// <summary>
    /// Produces pristine deterministic sites, or only structured rejections when unsafe.
    /// </summary>
    public static LoadedOperationRecognition Recognize(
        LoadedMethodBodySnapshot body,
        Guid moduleVersionId,
        int containingMethodToken,
        ILoadedOperationMetadataResolver resolver,
        string constructedContext = "")
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(constructedContext);
        if (moduleVersionId == Guid.Empty)
            throw new ArgumentException("A loaded module MVID is required.", nameof(moduleVersionId));
        if (containingMethodToken == 0)
            throw new ArgumentOutOfRangeException(nameof(containingMethodToken));

        var sites = ImmutableArray.CreateBuilder<LoadedOperationSiteDescriptor>();
        var rejections = ImmutableArray.CreateBuilder<LoadedOperationRejection>();
        for (var index = 0; index < body.Instructions.Length; ++index)
        {
            var instruction = body.Instructions[index];
            if (!IsCandidate(instruction.OpCodeValue))
                continue;

            var prefixes = Prefixes(body.Instructions, index);
            if (!TryRecognize(
                    instruction,
                    prefixes,
                    resolver,
                    out var operation,
                    out var rejection))
            {
                rejections.Add(rejection!);
                continue;
            }

            sites.Add(Site(
                body,
                moduleVersionId,
                containingMethodToken,
                constructedContext,
                instruction,
                operation!));
        }

        if (rejections.Count != 0)
            return new([], rejections.ToImmutable());
        return new(sites.ToImmutable(), []);
    }

    /// <summary>Dispatches a candidate instruction to method or field recognition.</summary>
    private static bool TryRecognize(
        LoadedIlInstruction instruction,
        ImmutableArray<LoadedIlInstruction> prefixes,
        ILoadedOperationMetadataResolver resolver,
        out LoadedRecognizedOperation? operation,
        out LoadedOperationRejection? rejection)
    {
        if (instruction.OpCodeValue is Call or CallVirt or NewObject)
        {
            return LoadedMethodOperationRecognizer.TryRecognize(
                instruction,
                prefixes,
                resolver,
                out operation,
                out rejection);
        }

        return LoadedFieldOperationRecognizer.TryRecognize(
            instruction,
            prefixes,
            resolver,
            out operation,
            out rejection);
    }

    /// <summary>Gets contiguous prefixes owned by one candidate instruction.</summary>
    private static ImmutableArray<LoadedIlInstruction> Prefixes(
        ImmutableArray<LoadedIlInstruction> instructions,
        int operationIndex)
    {
        var start = operationIndex;
        while (start > 0 && instructions[start - 1].IsPrefix)
            --start;
        return instructions[start..operationIndex];
    }

    /// <summary>Creates one deterministic immutable site descriptor.</summary>
    private static LoadedOperationSiteDescriptor Site(
        LoadedMethodBodySnapshot body,
        Guid moduleVersionId,
        int containingMethodToken,
        string constructedContext,
        LoadedIlInstruction instruction,
        LoadedRecognizedOperation operation) =>
        new(
            LoadedOperationSiteIdentity.Create(
                moduleVersionId,
                containingMethodToken,
                constructedContext,
                body.Identity,
                instruction,
                operation),
            moduleVersionId,
            containingMethodToken,
            constructedContext,
            body.Identity,
            instruction.BaselineOffset,
            instruction.OpCodeValue,
            operation.Kind,
            operation.MetadataToken,
            operation.CanonicalSignature,
            operation.Prefixes);

    /// <summary>Gets whether an opcode is a supported operation candidate.</summary>
    private static bool IsCandidate(ushort opCode) =>
        opCode is
            Call or
            CallVirt or
            NewObject or
            LoadField or
            StoreField or
            LoadStaticField or
            StoreStaticField;
}
