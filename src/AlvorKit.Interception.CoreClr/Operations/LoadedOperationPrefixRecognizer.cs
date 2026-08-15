using System.Collections.Immutable;
using static AlvorKit.LoadedOperationOpCodes;

namespace AlvorKit;

/// <summary>Resolves and validates prefixes owned by one candidate operation.</summary>
internal static class LoadedOperationPrefixRecognizer
{
    /// <summary>Finds and resolves the first constrained type needed for call classification.</summary>
    internal static bool TryResolveConstrainedType(
        LoadedIlInstruction operation,
        ImmutableArray<LoadedIlInstruction> prefixes,
        ILoadedOperationMetadataResolver resolver,
        out LoadedTypeOperand? constrainedType,
        out LoadedOperationRejection? rejection)
    {
        foreach (var prefix in prefixes)
        {
            if (prefix.OpCodeValue != ConstrainedPrefix)
                continue;

            var token = unchecked((int)prefix.Operand.IntegerValue);
            if (!resolver.TryResolveType(token, out constrainedType))
            {
                rejection = LoadedOperationRejections.AtPrefix(
                    operation,
                    prefix,
                    LoadedOperationRejectionReason.UnresolvedMetadata,
                    token,
                    $"constrained type token 0x{unchecked((uint)token):X8} " +
                    "was not resolved");
                return false;
            }

            rejection = null;
            return true;
        }

        constrainedType = null;
        rejection = null;
        return true;
    }

    /// <summary>Validates the accepted prefix matrix and returns immutable descriptors.</summary>
    internal static bool TryValidate(
        LoadedIlInstruction operation,
        LoadedOperationKind kind,
        ImmutableArray<LoadedIlInstruction> prefixes,
        LoadedTypeOperand? constrainedType,
        out ImmutableArray<LoadedOperationPrefixDescriptor> descriptors,
        out LoadedOperationRejection? rejection)
    {
        var result =
            ImmutableArray.CreateBuilder<LoadedOperationPrefixDescriptor>(
                prefixes.Length);
        var sawVolatile = false;
        var sawConstrained = false;
        foreach (var prefix in prefixes)
        {
            if (prefix.OpCodeValue == VolatilePrefix)
            {
                if (sawVolatile)
                {
                    return Duplicate(
                        operation,
                        prefix,
                        "volatile.",
                        out descriptors,
                        out rejection);
                }
                sawVolatile = true;
                if (!IsField(kind))
                {
                    return Unsupported(
                        operation,
                        prefix,
                        kind,
                        out descriptors,
                        out rejection);
                }
                result.Add(new(
                    LoadedOperationPrefixKind.Volatile,
                    prefix.BaselineOffset,
                    0,
                    ""));
                continue;
            }

            if (prefix.OpCodeValue == ConstrainedPrefix)
            {
                if (sawConstrained)
                {
                    return Duplicate(
                        operation,
                        prefix,
                        "constrained.",
                        out descriptors,
                        out rejection);
                }
                sawConstrained = true;
                if (kind != LoadedOperationKind.StructMethod ||
                    constrainedType is null)
                {
                    return Unsupported(
                        operation,
                        prefix,
                        kind,
                        out descriptors,
                        out rejection);
                }

                var token = unchecked((int)prefix.Operand.IntegerValue);
                result.Add(new(
                    LoadedOperationPrefixKind.Constrained,
                    prefix.BaselineOffset,
                    token,
                    constrainedType.CanonicalSignature));
                continue;
            }

            return Unsupported(
                operation,
                prefix,
                kind,
                out descriptors,
                out rejection);
        }

        descriptors = result.MoveToImmutable();
        rejection = null;
        return true;
    }

    /// <summary>Gets whether volatile replay is supported for the operation kind.</summary>
    private static bool IsField(LoadedOperationKind kind) =>
        kind is
            LoadedOperationKind.StaticFieldRead or
            LoadedOperationKind.StaticFieldWrite or
            LoadedOperationKind.InstanceFieldRead or
            LoadedOperationKind.InstanceFieldWrite;

    /// <summary>Returns a stable duplicate-prefix rejection.</summary>
    private static bool Duplicate(
        LoadedIlInstruction operation,
        LoadedIlInstruction prefix,
        string name,
        out ImmutableArray<LoadedOperationPrefixDescriptor> descriptors,
        out LoadedOperationRejection? rejection)
    {
        descriptors = [];
        rejection = LoadedOperationRejections.AtPrefix(
            operation,
            prefix,
            LoadedOperationRejectionReason.DuplicatePrefix,
            0,
            $"prefix '{name}' occurs more than once");
        return false;
    }

    /// <summary>Returns a stable unsupported-prefix rejection.</summary>
    private static bool Unsupported(
        LoadedIlInstruction operation,
        LoadedIlInstruction prefix,
        LoadedOperationKind kind,
        out ImmutableArray<LoadedOperationPrefixDescriptor> descriptors,
        out LoadedOperationRejection? rejection)
    {
        descriptors = [];
        rejection = LoadedOperationRejections.AtPrefix(
            operation,
            prefix,
            LoadedOperationRejectionReason.UnsupportedPrefix,
            prefix.Operand.Kind == LoadedIlOperandKind.MetadataToken
                ? unchecked((int)prefix.Operand.IntegerValue)
                : 0,
            $"prefix '{prefix.OpCode.Name}' is unsupported for {kind}");
        return false;
    }
}
