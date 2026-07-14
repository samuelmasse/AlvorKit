namespace AlvorKit.ECS.Generator;

[ExcludeFromCodeCoverage]
internal static class ComponentModelFactory
{
    internal const string ComponentsAttributeName = "AlvorKit.ECS.Generator.ComponentsAttribute";
    internal const string ArchetypalAttributeName = "AlvorKit.ECS.Generator.ArchetypalAttribute";

    private static readonly SymbolDisplayFormat NullableFriendlyFormat = new(
    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
        SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

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

    private static bool IsComponentProperty(IPropertySymbol property) =>
    !property.IsStatic &&
    property.Parameters.Length == 0 &&
    property.GetMethod is not null &&
    property.SetMethod is not null;

    private static PropertyModel CreateProperty(IPropertySymbol property, CancellationToken cancellationToken) =>
    new(
        Name: property.Name,
        ValueType: property.Type.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)),
        NullableType: property.Type.ToDisplayString(NullableFriendlyFormat),
        AddToString: HasAttribute(property, "ComponentToStringAttribute"),
        LazyInitialize: HasAttribute(property, "ComponentLazyInitializeAttribute"),
        Archetypal: HasAttribute(property, ArchetypalAttributeName),
        IsDelegate: property.Type.TypeKind == TypeKind.Delegate,
        Comment: ReadXmlComment(property, cancellationToken),
        GetAccess: ComponentAccess.ToAccessString(property.GetMethod!.DeclaredAccessibility),
        SetAccess: ComponentAccess.ToAccessString(property.SetMethod!.DeclaredAccessibility));

    private static bool ShouldSkipBuilder(GeneratorAttributeSyntaxContext context) =>
    context.Attributes.Any(attribute => attribute.NamedArguments
        .Any(argument => argument.Key == "SkipBuilder" && argument.Value.Value is true));

    private static bool HasAttribute(IPropertySymbol property, string attributeName) =>
    property.GetAttributes().Any(attribute =>
        attribute.AttributeClass?.Name == attributeName ||
        attribute.AttributeClass?.ToDisplayString() == attributeName);

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
