namespace AlvorKit.Script.LiveCode;

/// <summary>Proves that two source versions differ only in one supported existing method body.</summary>
internal static class SourceUpdateEditValidator
{
    internal static SourceUpdateValidatedEdit Validate(
        string oldSource,
        string newSource,
        SemanticModel oldModel,
        SemanticModel newModel,
        CancellationToken cancellationToken)
    {
        var oldRoot = (CompilationUnitSyntax)oldModel.SyntaxTree.GetRoot(cancellationToken);
        var newRoot = (CompilationUnitSyntax)newModel.SyntaxTree.GetRoot(cancellationToken);
        var oldMethods = oldRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        var newMethods = newRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToArray();
        if (oldMethods.Length != newMethods.Length)
            throw Unsupported("Method declarations changed.");

        var changed = new List<(MethodDeclarationSyntax Old, MethodDeclarationSyntax New)>();
        for (var i = 0; i < oldMethods.Length; i++)
        {
            var oldBody = Body(oldMethods[i]);
            var newBody = Body(newMethods[i]);
            if (oldBody.ToFullString() != newBody.ToFullString())
                changed.Add((oldMethods[i], newMethods[i]));
        }
        if (changed.Count != 1)
            throw Unsupported($"Expected exactly one changed method body, but found {changed.Count}.");

        var (Old, New) = changed[0];
        var oldBodyNode = Body(Old);
        var newBodyNode = Body(New);
        if (oldSource[..oldBodyNode.SpanStart] != newSource[..newBodyNode.SpanStart] ||
            oldSource[oldBodyNode.Span.End..] != newSource[newBodyNode.Span.End..])
        {
            throw Unsupported("Text outside the selected method body changed.");
        }

        RejectSyntax(New);
        var oldSymbol = oldModel.GetDeclaredSymbol(Old, cancellationToken)
            ?? throw Unsupported("The old method symbol could not be resolved.");
        var newSymbol = newModel.GetDeclaredSymbol(New, cancellationToken)
            ?? throw Unsupported("The new method symbol could not be resolved.");
        RejectSymbol(oldSymbol);
        RejectDynamic(newBodyNode, newModel, cancellationToken);
        RejectNewPrimaryCaptures(
            oldBodyNode,
            newBodyNode,
            oldModel,
            newModel,
            cancellationToken);
        return new(Old, New, oldSymbol, newSymbol);
    }

    private static SyntaxNode Body(MethodDeclarationSyntax method) =>
        method.Body
        ?? (SyntaxNode?)method.ExpressionBody
        ?? throw Unsupported($"Method '{method.Identifier.ValueText}' has no source-authored body.");

    private static void RejectSyntax(MethodDeclarationSyntax method)
    {
        if (method.Modifiers.Any(SyntaxKind.AsyncKeyword) ||
            method.Modifiers.Any(SyntaxKind.UnsafeKeyword))
        {
            throw Unsupported("Async and unsafe methods are not supported.");
        }

        var body = Body(method);
        if (body.DescendantNodesAndSelf().Any(static node =>
            node is AnonymousFunctionExpressionSyntax
                or LocalFunctionStatementSyntax
                or AnonymousObjectCreationExpressionSyntax
                or StackAllocArrayCreationExpressionSyntax
                or PointerTypeSyntax
                or FunctionPointerTypeSyntax
                or UnsafeStatementSyntax
                or AwaitExpressionSyntax
                or YieldStatementSyntax))
        {
            throw Unsupported(
                "Lambdas, local functions, anonymous objects, async/iterator, and unsafe shapes are not supported.");
        }
    }

    private static void RejectSymbol(IMethodSymbol symbol)
    {
        if (symbol.MethodKind != MethodKind.Ordinary)
            throw Unsupported("Only ordinary existing methods are supported.");
        if (symbol.IsGenericMethod || symbol.ContainingType.IsGenericType)
            throw Unsupported("Generic methods and methods in generic types are not supported.");
    }

    private static void RejectDynamic(
        SyntaxNode body,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        foreach (var expression in body.DescendantNodesAndSelf().OfType<ExpressionSyntax>())
        {
            if (model.GetTypeInfo(expression, cancellationToken).Type?.TypeKind == TypeKind.Dynamic)
                throw Unsupported("Dynamic operations are not supported.");
        }
    }

    private static void RejectNewPrimaryCaptures(
        SyntaxNode oldBody,
        SyntaxNode newBody,
        SemanticModel oldModel,
        SemanticModel newModel,
        CancellationToken cancellationToken)
    {
        var oldParameters = PrimaryParameters(oldBody, oldModel, cancellationToken);
        var newParameters = PrimaryParameters(newBody, newModel, cancellationToken);
        if (!newParameters.IsSubsetOf(oldParameters))
            throw Unsupported("The edit newly captures a primary-constructor parameter.");
    }

    private static HashSet<string> PrimaryParameters(
        SyntaxNode body,
        SemanticModel model,
        CancellationToken cancellationToken)
    {
        var parameters = new HashSet<string>(StringComparer.Ordinal);
        foreach (var identifier in body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
        {
            if (model.GetSymbolInfo(identifier, cancellationToken).Symbol is not IParameterSymbol parameter ||
                parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor })
            {
                continue;
            }
            var syntax = parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
            if (syntax?.Parent?.Parent is ConstructorDeclarationSyntax)
                continue;

            parameters.Add(parameter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
        return parameters;
    }

    private static InvalidOperationException Unsupported(string message) =>
        new($"Unsupported Source Update edit: {message}");
}
