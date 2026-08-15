using System.Collections.Immutable;
using static AlvorKit.LoadedOperationOpCodes;

namespace AlvorKit;

/// <summary>Recognizes exact static and reference-receiver field operations.</summary>
internal static class LoadedFieldOperationRecognizer
{
    /// <summary>Recognizes one field-token instruction or returns a structured rejection.</summary>
    internal static bool TryRecognize(
        LoadedIlInstruction instruction,
        ImmutableArray<LoadedIlInstruction> prefixes,
        ILoadedOperationMetadataResolver resolver,
        out LoadedRecognizedOperation? operation,
        out LoadedOperationRejection? rejection)
    {
        var token = unchecked((int)instruction.Operand.IntegerValue);
        if (!resolver.TryResolveField(token, out var field))
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.UnresolvedMetadata,
                token,
                $"field token 0x{unchecked((uint)token):X8} was not resolved",
                out operation,
                out rejection);
        }
        if (field.ContainsOpenGenericParameters)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.OpenGenericSignature,
                token,
                $"signature '{field.CanonicalSignature}' remains open",
                out operation,
                out rejection);
        }

        var expectsStatic = instruction.OpCodeValue is
            LoadStaticField or StoreStaticField;
        if (field.IsStatic != expectsStatic)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.InvalidOperationSignature,
                token,
                expectsStatic
                    ? "static field opcode resolved to an instance field"
                    : "instance field opcode resolved to a static field",
                out operation,
                out rejection);
        }
        if (!field.IsStatic && field.IsByRefLikeReceiver)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.RefLikeReceiver,
                token,
                $"signature '{field.CanonicalSignature}' has a ref-like receiver",
                out operation,
                out rejection);
        }
        if (!field.IsStatic &&
            field.DeclaringTypeShape != LoadedTypeShape.ReferenceType)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.UnsupportedReceiver,
                token,
                "instance field receiver is not an ordinary closed reference type",
                out operation,
                out rejection);
        }

        var kind = instruction.OpCodeValue switch
        {
            LoadField => LoadedOperationKind.InstanceFieldRead,
            StoreField => LoadedOperationKind.InstanceFieldWrite,
            LoadStaticField => LoadedOperationKind.StaticFieldRead,
            StoreStaticField => LoadedOperationKind.StaticFieldWrite,
            _ => throw new UnreachableException()
        };
        if (!LoadedOperationPrefixRecognizer.TryValidate(
                instruction,
                kind,
                prefixes,
                null,
                out var acceptedPrefixes,
                out rejection))
        {
            operation = null;
            return false;
        }

        operation = new(
            kind,
            token,
            field.CanonicalSignature,
            acceptedPrefixes);
        rejection = null;
        return true;
    }

    /// <summary>Returns a structured field recognition rejection.</summary>
    private static bool Reject(
        LoadedIlInstruction instruction,
        LoadedOperationRejectionReason reason,
        int token,
        string detail,
        out LoadedRecognizedOperation? operation,
        out LoadedOperationRejection? rejection)
    {
        operation = null;
        rejection = LoadedOperationRejections.AtOperation(
            instruction,
            reason,
            token,
            detail);
        return false;
    }
}
