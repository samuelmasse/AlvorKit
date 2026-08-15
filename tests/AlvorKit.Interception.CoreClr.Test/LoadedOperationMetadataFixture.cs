namespace AlvorKit;

/// <summary>Resolves reviewed exact metadata for isolated operation-recognition tests.</summary>
internal sealed class LoadedOperationMetadataFixture :
    ILoadedOperationMetadataResolver
{
    /// <summary>Exact method operands indexed by their raw metadata tokens.</summary>
    private readonly Dictionary<int, LoadedMethodOperand> methods = [];

    /// <summary>Exact field operands indexed by their raw metadata tokens.</summary>
    private readonly Dictionary<int, LoadedFieldOperand> fields = [];

    /// <summary>Exact constrained type operands indexed by their raw metadata tokens.</summary>
    private readonly Dictionary<int, LoadedTypeOperand> types = [];

    /// <summary>Registers one exact method operand.</summary>
    internal LoadedOperationMetadataFixture Method(
        int token,
        LoadedMethodOperand method)
    {
        methods.Add(token, method);
        return this;
    }

    /// <summary>Registers one exact field operand.</summary>
    internal LoadedOperationMetadataFixture Field(
        int token,
        LoadedFieldOperand field)
    {
        fields.Add(token, field);
        return this;
    }

    /// <summary>Registers one exact constrained type operand.</summary>
    internal LoadedOperationMetadataFixture Type(
        int token,
        LoadedTypeOperand type)
    {
        types.Add(token, type);
        return this;
    }

    /// <summary>Resolves a registered exact method operand.</summary>
    public bool TryResolveMethod(
        int metadataToken,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
        out LoadedMethodOperand? method) =>
        methods.TryGetValue(metadataToken, out method);

    /// <summary>Resolves a registered exact field operand.</summary>
    public bool TryResolveField(
        int metadataToken,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
        out LoadedFieldOperand? field) =>
        fields.TryGetValue(metadataToken, out field);

    /// <summary>Resolves a registered exact constrained type operand.</summary>
    public bool TryResolveType(
        int metadataToken,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
        out LoadedTypeOperand? type) =>
        types.TryGetValue(metadataToken, out type);
}
