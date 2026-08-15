using System.Collections.Immutable;

namespace AlvorKit;

/// <summary>
/// Owns an immutable decoded snapshot of the exact method body supplied by the loaded runtime.
/// </summary>
public sealed class LoadedMethodBodySnapshot
{
    /// <summary>The complete copied authoritative method-body bytes.</summary>
    private readonly ImmutableArray<byte> bytes;

    /// <summary>The stable identity of the complete bytes.</summary>
    private readonly LoadedMethodBodyIdentity identity;

    /// <summary>The decoded header encoding.</summary>
    private readonly LoadedMethodBodyHeaderKind headerKind;

    /// <summary>The loaded header length in bytes.</summary>
    private readonly int headerSize;

    /// <summary>The loaded IL stream length in bytes.</summary>
    private readonly int codeSize;

    /// <summary>The declared maximum stack depth.</summary>
    private readonly ushort maxStack;

    /// <summary>Whether local storage is initialized before execution.</summary>
    private readonly bool initLocals;

    /// <summary>The unresolved local-signature token.</summary>
    private readonly int localSignatureToken;

    /// <summary>The immutable decoded instruction sequence.</summary>
    private readonly ImmutableArray<LoadedIlInstruction> instructions;

    /// <summary>The immutable decoded exception clauses.</summary>
    private readonly ImmutableArray<LoadedExceptionRegion> exceptionRegions;

    /// <summary>Creates a validated immutable loaded-body snapshot.</summary>
    internal LoadedMethodBodySnapshot(
        ImmutableArray<byte> bytes,
        LoadedMethodBodyIdentity identity,
        LoadedMethodBodyHeaderKind headerKind,
        int headerSize,
        int codeSize,
        ushort maxStack,
        bool initLocals,
        int localSignatureToken,
        ImmutableArray<LoadedIlInstruction> instructions,
        ImmutableArray<LoadedExceptionRegion> exceptionRegions)
    {
        this.bytes = bytes;
        this.identity = identity;
        this.headerKind = headerKind;
        this.headerSize = headerSize;
        this.codeSize = codeSize;
        this.maxStack = maxStack;
        this.initLocals = initLocals;
        this.localSignatureToken = localSignatureToken;
        this.instructions = instructions;
        this.exceptionRegions = exceptionRegions;
    }

    /// <summary>Gets the immutable complete bytes, including headers and extra sections.</summary>
    public ImmutableArray<byte> Bytes => bytes;

    /// <summary>Gets the stable identity computed from <see cref="Bytes"/>.</summary>
    public LoadedMethodBodyIdentity Identity => identity;

    /// <summary>Gets the loaded method header encoding.</summary>
    public LoadedMethodBodyHeaderKind HeaderKind => headerKind;

    /// <summary>Gets the method header size in bytes.</summary>
    public int HeaderSize => headerSize;

    /// <summary>Gets the loaded IL stream size in bytes.</summary>
    public int CodeSize => codeSize;

    /// <summary>Gets the declared maximum evaluation-stack depth.</summary>
    public ushort MaxStack => maxStack;

    /// <summary>Gets whether the runtime initializes declared locals before executing IL.</summary>
    public bool InitLocals => initLocals;

    /// <summary>Gets the StandAloneSig local-variable token, or zero when absent.</summary>
    public int LocalSignatureToken => localSignatureToken;

    /// <summary>Gets instructions keyed to immutable loaded-baseline offsets.</summary>
    public ImmutableArray<LoadedIlInstruction> Instructions => instructions;

    /// <summary>Gets all decoded exception clauses in loaded-baseline offsets.</summary>
    public ImmutableArray<LoadedExceptionRegion> ExceptionRegions =>
        exceptionRegions;
}
