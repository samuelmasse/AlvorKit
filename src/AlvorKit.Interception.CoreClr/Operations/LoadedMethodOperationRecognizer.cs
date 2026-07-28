using System.Collections.Immutable;
using static AlvorKit.Interception.CoreClr.Advanced.LoadedOperationOpCodes;

namespace AlvorKit.Interception.CoreClr.Advanced;

/// <summary>Recognizes exact call, virtual-call, and construction operations.</summary>
internal static class LoadedMethodOperationRecognizer
{
    /// <summary>Recognizes one method-token instruction or returns a structured rejection.</summary>
    internal static bool TryRecognize(
        LoadedIlInstruction instruction,
        ImmutableArray<LoadedIlInstruction> prefixes,
        ILoadedOperationMetadataResolver resolver,
        out LoadedRecognizedOperation? operation,
        out LoadedOperationRejection? rejection)
    {
        var token = unchecked((int)instruction.Operand.IntegerValue);
        if (!resolver.TryResolveMethod(token, out var method))
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.UnresolvedMetadata,
                token,
                $"method token 0x{unchecked((uint)token):X8} was not resolved",
                out operation,
                out rejection);
        }

        if (method.IsVariableArguments)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.VariableArguments,
                token,
                $"vararg signature '{method.CanonicalSignature}' cannot " +
                "use an exact managed route",
                out operation,
                out rejection);
        }
        if (method.ContainsOpenGenericParameters)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.OpenGenericSignature,
                token,
                $"signature '{method.CanonicalSignature}' remains open",
                out operation,
                out rejection);
        }

        if (!LoadedOperationPrefixRecognizer.TryResolveConstrainedType(
                instruction,
                prefixes,
                resolver,
                out var constrainedType,
                out rejection))
        {
            operation = null;
            return false;
        }

        if ((method.HasThis && method.IsByRefLikeReceiver) ||
            constrainedType?.IsByRefLike == true)
        {
            return Reject(
                instruction,
                LoadedOperationRejectionReason.RefLikeReceiver,
                token,
                $"signature '{method.CanonicalSignature}' has a ref-like " +
                "live receiver",
                out operation,
                out rejection);
        }

        if (!TryClassify(
                instruction,
                method,
                constrainedType,
                token,
                out var kind,
                out rejection))
        {
            operation = null;
            return false;
        }

        if (!LoadedOperationPrefixRecognizer.TryValidate(
                instruction,
                kind,
                prefixes,
                constrainedType,
                out var acceptedPrefixes,
                out rejection))
        {
            operation = null;
            return false;
        }

        operation = new(
            kind,
            token,
            method.CanonicalSignature,
            acceptedPrefixes);
        rejection = null;
        return true;
    }

    /// <summary>Classifies static, reference, value, constrained, and construction calls.</summary>
    private static bool TryClassify(
        LoadedIlInstruction instruction,
        LoadedMethodOperand method,
        LoadedTypeOperand? constrainedType,
        int token,
        out LoadedOperationKind kind,
        out LoadedOperationRejection? rejection)
    {
        if (instruction.OpCodeValue == NewObject)
        {
            if (!method.HasThis || !method.IsConstructor)
            {
                return InvalidShape(
                    instruction,
                    token,
                    "newobj requires an instance-constructor signature",
                    out kind,
                    out rejection);
            }
            kind = LoadedOperationKind.ObjectConstruction;
            rejection = null;
            return true;
        }

        if (method.IsConstructor)
        {
            return InvalidShape(
                instruction,
                token,
                "constructor calls are planned only by constructor-remainder analysis",
                out kind,
                out rejection);
        }
        if (!method.HasThis)
        {
            if (instruction.OpCodeValue != Call)
            {
                return InvalidShape(
                    instruction,
                    token,
                    "callvirt cannot target a static signature",
                    out kind,
                    out rejection);
            }
            kind = LoadedOperationKind.StaticCall;
            rejection = null;
            return true;
        }

        if (constrainedType is not null)
        {
            if (method.DeclaringTypeShape != LoadedTypeShape.Interface ||
                constrainedType.Shape != LoadedTypeShape.ValueType)
            {
                return UnsupportedReceiver(
                    instruction,
                    token,
                    "constrained. requires an interface method and a closed value receiver",
                    out kind,
                    out rejection);
            }
            kind = LoadedOperationKind.StructMethod;
            rejection = null;
            return true;
        }

        switch (method.DeclaringTypeShape)
        {
            case LoadedTypeShape.ReferenceType or LoadedTypeShape.Interface:
                kind = LoadedOperationKind.InstanceCall;
                rejection = null;
                return true;
            case LoadedTypeShape.ValueType:
                kind = LoadedOperationKind.StructMethod;
                rejection = null;
                return true;
            default:
                return UnsupportedReceiver(
                    instruction,
                    token,
                    "open generic receiver has no exact live stack shape",
                    out kind,
                    out rejection);
        }
    }

    /// <summary>Returns a structured opcode/signature mismatch.</summary>
    private static bool InvalidShape(
        LoadedIlInstruction instruction,
        int token,
        string detail,
        out LoadedOperationKind kind,
        out LoadedOperationRejection? rejection)
    {
        kind = default;
        rejection = LoadedOperationRejections.AtOperation(
            instruction,
            LoadedOperationRejectionReason.InvalidOperationSignature,
            token,
            detail);
        return false;
    }

    /// <summary>Returns a structured unsupported receiver rejection.</summary>
    private static bool UnsupportedReceiver(
        LoadedIlInstruction instruction,
        int token,
        string detail,
        out LoadedOperationKind kind,
        out LoadedOperationRejection? rejection)
    {
        kind = default;
        rejection = LoadedOperationRejections.AtOperation(
            instruction,
            LoadedOperationRejectionReason.UnsupportedReceiver,
            token,
            detail);
        return false;
    }

    /// <summary>Returns a structured rejection before operation classification.</summary>
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
