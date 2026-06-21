namespace AlvorKit.ECS.Generator;

/// <summary>Creates generator models from Roslyn symbols.</summary>
[ExcludeFromCodeCoverage]
internal static class ComponentModelFactory
{
    /// <summary>The fully qualified metadata name used to discover ECS component interfaces.</summary>
    internal const string ComponentsAttributeName = "AlvorKit.ECS.Generator.ComponentsAttribute";

    /// <summary>Formats nullable-aware type names for generated accessors.</summary>
    private static readonly SymbolDisplayFormat NullableFriendlyFormat = new(
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>Creates a generation model for the attributed interface.</summary>
    internal static InterfaceModel Create(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        var interfaceSymbol = (INamedTypeSymbol)context.TargetSymbol;
        var namespaceName = interfaceSymbol.ContainingNamespace?.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) ?? "";

        var properties = interfaceSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(IsComponentProperty)
            .OrderBy(property => property.Locations.FirstOrDefault()?.SourceSpan.Start ?? int.MaxValue)
            .Select(property => CreateProperty(property, cancellationToken))
            .ToArray();

        return new(
            Namespace: namespaceName,
            InterfaceName: interfaceSymbol.Name,
            ClassName: ComponentNames.StripInterfacePrefix(interfaceSymbol.Name),
            Properties: properties,
            SkipBuilder: ShouldSkipBuilder(context),
            Access: ComponentAccess.ToAccessString(interfaceSymbol.DeclaredAccessibility));
    }

    /// <summary>Returns whether a property can become a component accessor.</summary>
    private static bool IsComponentProperty(IPropertySymbol property) =>
        !property.IsStatic &&
        property.Parameters.Length == 0 &&
        property.GetMethod is not null &&
        property.SetMethod is not null;

    /// <summary>Creates a property generation model.</summary>
    private static PropertyModel CreateProperty(IPropertySymbol property, CancellationToken cancellationToken) =>
        new(
            Name: property.Name,
            ValueType: property.Type.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
            NullableType: property.Type.ToDisplayString(NullableFriendlyFormat),
            AddToString: HasAttribute(property, "ComponentToStringAttribute"),
            LazyInitialize: HasAttribute(property, "ComponentLazyInitializeAttribute"),
            IsDelegate: property.Type.TypeKind == TypeKind.Delegate,
            Comment: ReadXmlComment(property, cancellationToken),
            GetAccess: ComponentAccess.ToAccessString(property.GetMethod!.DeclaredAccessibility),
            SetAccess: ComponentAccess.ToAccessString(property.SetMethod!.DeclaredAccessibility));

    /// <summary>Returns whether the attribute requested no builder-style mutator extensions.</summary>
    private static bool ShouldSkipBuilder(GeneratorAttributeSyntaxContext context) =>
        context.Attributes.Any(attribute => attribute.NamedArguments
            .Any(argument => argument.Key == "SkipBuilder" && argument.Value.Value is true));

    /// <summary>Returns whether a property carries an attribute with the given short type name.</summary>
    private static bool HasAttribute(IPropertySymbol property, string attributeName) =>
        property.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == attributeName);

    /// <summary>Reads source XML documentation trivia so generated accessors preserve component docs.</summary>
    private static string? ReadXmlComment(IPropertySymbol property, CancellationToken cancellationToken)
    {
        var syntaxNode = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken);
        if (syntaxNode is null)
            return null;

        var lines = syntaxNode.GetLeadingTrivia()
            .ToFullString()
            .Split('\n')
            .Select(line => line.TrimStart())
            .Where(line => line.StartsWith("///", StringComparison.Ordinal))
            .Select(line => line.TrimEnd())
            .ToArray();

        return lines.Length == 0 ? null : string.Join("\n", lines);
    }
}
