namespace AlvorKit;

/// <summary>One conservatively validated existing method-body update.</summary>
internal sealed record SourceUpdateValidatedEdit(
    MethodDeclarationSyntax OldMethod,
    MethodDeclarationSyntax NewMethod,
    IMethodSymbol OldSymbol,
    IMethodSymbol NewSymbol);
