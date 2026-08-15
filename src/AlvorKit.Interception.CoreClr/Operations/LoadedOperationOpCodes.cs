namespace AlvorKit;

/// <summary>Defines the exact encoded opcode values used by operation recognition.</summary>
internal static class LoadedOperationOpCodes
{
    /// <summary>The direct call opcode.</summary>
    internal const ushort Call = 0x28;

    /// <summary>The virtual call opcode.</summary>
    internal const ushort CallVirt = 0x6F;

    /// <summary>The object-construction opcode.</summary>
    internal const ushort NewObject = 0x73;

    /// <summary>The instance field-read opcode.</summary>
    internal const ushort LoadField = 0x7B;

    /// <summary>The instance field-write opcode.</summary>
    internal const ushort StoreField = 0x7D;

    /// <summary>The static field-read opcode.</summary>
    internal const ushort LoadStaticField = 0x7E;

    /// <summary>The static field-write opcode.</summary>
    internal const ushort StoreStaticField = 0x80;

    /// <summary>The volatile memory-semantics prefix.</summary>
    internal const ushort VolatilePrefix = 0xFE13;

    /// <summary>The constrained receiver prefix.</summary>
    internal const ushort ConstrainedPrefix = 0xFE16;
}
