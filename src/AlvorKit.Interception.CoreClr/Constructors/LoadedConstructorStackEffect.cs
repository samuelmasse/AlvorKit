namespace AlvorKit;

/// <summary>Computes a fixed evaluation-stack transition for one loaded IL instruction.</summary>
internal static class LoadedConstructorStackEffect
{
    /// <summary>Metadata opcode value for <c>call</c>.</summary>
    private const ushort Call = 0x0028;

    /// <summary>Metadata opcode value for <c>callvirt</c>.</summary>
    private const ushort CallVirtual = 0x006F;

    /// <summary>Metadata opcode value for <c>newobj</c>.</summary>
    private const ushort NewObject = 0x0073;

    /// <summary>Computes the outgoing stack depth or explains why the effect is not provable.</summary>
    /// <param name="instruction">Loaded instruction to evaluate.</param>
    /// <param name="inputDepth">Proven incoming stack depth.</param>
    /// <param name="metadata">Resolver for variable call signatures.</param>
    /// <param name="outputDepth">Computed outgoing stack depth.</param>
    /// <param name="detail">Rejection detail when the transition cannot be proved.</param>
    /// <returns><see langword="true"/> when the transition is fixed and valid.</returns>
    internal static bool TryApply(
        LoadedIlInstruction instruction,
        int inputDepth,
        ILoadedConstructorMetadataResolver metadata,
        out int outputDepth,
        out string? detail)
    {
        if (instruction.OpCode == OpCodes.Leave ||
            instruction.OpCode == OpCodes.Leave_S)
        {
            outputDepth = 0;
            detail = null;
            return true;
        }

        if (!TryPopCount(instruction, metadata, out int pops, out detail) ||
            !TryPushCount(instruction, metadata, out int pushes, out detail))
        {
            outputDepth = 0;
            return false;
        }
        if (inputDepth < pops)
        {
            outputDepth = 0;
            detail =
                $"Instruction {Offset(instruction.BaselineOffset)} " +
                $"('{instruction.OpCode.Name}') underflows the proven " +
                "evaluation stack.";
            return false;
        }

        outputDepth = (inputDepth - pops + pushes);
        detail = null;
        return true;
    }

    /// <summary>Resolves the fixed pop count for one instruction.</summary>
    private static bool TryPopCount(
        LoadedIlInstruction instruction,
        ILoadedConstructorMetadataResolver metadata,
        out int count,
        out string? detail)
    {
        switch (instruction.OpCode.StackBehaviourPop)
        {
            case StackBehaviour.Pop0:
                count = 0;
                detail = null;
                return true;
            case StackBehaviour.Pop1:
            case StackBehaviour.Popi:
            case StackBehaviour.Popref:
                count = 1;
                detail = null;
                return true;
            case StackBehaviour.Pop1_pop1:
            case StackBehaviour.Popi_pop1:
            case StackBehaviour.Popi_popi:
            case StackBehaviour.Popi_popi8:
            case StackBehaviour.Popi_popr4:
            case StackBehaviour.Popi_popr8:
            case StackBehaviour.Popref_pop1:
            case StackBehaviour.Popref_popi:
                count = 2;
                detail = null;
                return true;
            case StackBehaviour.Popi_popi_popi:
            case StackBehaviour.Popref_popi_popi:
            case StackBehaviour.Popref_popi_popi8:
            case StackBehaviour.Popref_popi_popr4:
            case StackBehaviour.Popref_popi_popr8:
            case StackBehaviour.Popref_popi_popref:
                count = 3;
                detail = null;
                return true;
            case StackBehaviour.Varpop:
                return TryVariableCallEffect(
                    instruction,
                    metadata,
                    out count,
                    out _,
                    out detail);
            default:
                count = 0;
                detail =
                    $"Instruction {Offset(instruction.BaselineOffset)} has " +
                    $"unsupported pop behavior " +
                    $"'{instruction.OpCode.StackBehaviourPop}'.";
                return false;
        }
    }

    /// <summary>Resolves the fixed push count for one instruction.</summary>
    private static bool TryPushCount(
        LoadedIlInstruction instruction,
        ILoadedConstructorMetadataResolver metadata,
        out int count,
        out string? detail)
    {
        switch (instruction.OpCode.StackBehaviourPush)
        {
            case StackBehaviour.Push0:
                count = 0;
                detail = null;
                return true;
            case StackBehaviour.Push1:
            case StackBehaviour.Pushi:
            case StackBehaviour.Pushi8:
            case StackBehaviour.Pushr4:
            case StackBehaviour.Pushr8:
            case StackBehaviour.Pushref:
                count = 1;
                detail = null;
                return true;
            case StackBehaviour.Push1_push1:
                count = 2;
                detail = null;
                return true;
            case StackBehaviour.Varpush:
                return TryVariableCallEffect(
                    instruction,
                    metadata,
                    out _,
                    out count,
                    out detail);
            default:
                count = 0;
                detail =
                    $"Instruction {Offset(instruction.BaselineOffset)} has " +
                    $"unsupported push behavior " +
                    $"'{instruction.OpCode.StackBehaviourPush}'.";
                return false;
        }
    }

    /// <summary>Resolves pop and push counts from a fixed method signature.</summary>
    private static bool TryVariableCallEffect(
        LoadedIlInstruction instruction,
        ILoadedConstructorMetadataResolver metadata,
        out int pops,
        out int pushes,
        out string? detail)
    {
        if (instruction.OpCode == OpCodes.Ret)
        {
            pops = 0;
            pushes = 0;
            detail = null;
            return true;
        }
        if (instruction.OpCodeValue is not (Call or CallVirtual or NewObject) ||
            instruction.Operand.Kind != LoadedIlOperandKind.MetadataToken ||
            !metadata.TryResolveMethod(
                ((int)instruction.Operand.IntegerValue),
                out LoadedMethodOperand? method) ||
            method.IsVariableArguments)
        {
            pops = 0;
            pushes = 0;
            detail =
                $"Instruction {Offset(instruction.BaselineOffset)} " +
                $"('{instruction.OpCode.Name}') has no provable fixed stack effect.";
            return false;
        }

        pops = method.ParameterCount +
            (instruction.OpCodeValue == NewObject || !method.HasThis ? 0 : 1);
        pushes = instruction.OpCodeValue == NewObject || method.ReturnsValue
            ? 1
            : 0;
        detail = null;
        return true;
    }

    /// <summary>Formats one baseline IL coordinate.</summary>
    private static string Offset(int offset) => $"IL_{offset:X4}";
}
