namespace AlvorKit.ECS.Generator;

[ExcludeFromCodeCoverage]
internal static class EntArchRowModelFactory
{
    private static readonly SymbolDisplayFormat TypeFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(
            SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
            SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
            SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    internal static EntArchRowModel[] Create(
        Compilation compilation,
        System.Collections.Immutable.ImmutableArray<InterfaceModel> groups,
        System.Collections.Immutable.ImmutableArray<InvocationExpressionSyntax> calls,
        CancellationToken cancellationToken)
    {
        var rowsByKey = new Dictionary<string, EntArchRowModel>(StringComparer.Ordinal);
        foreach (var call in calls)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var semanticModel = compilation.GetSemanticModel(call.SyntaxTree);
            if (!TryCreate(call, semanticModel, groups, out var row))
                continue;

            string key = row.Namespace + "|" + row.QueryType;
            if (!rowsByKey.ContainsKey(key))
                rowsByKey.Add(key, row);
        }

        return rowsByKey.Values.OrderBy(row => row.ExtensionType, StringComparer.Ordinal).ToArray();
    }

    private static bool TryCreate(
        InvocationExpressionSyntax call,
        SemanticModel semanticModel,
        System.Collections.Immutable.ImmutableArray<InterfaceModel> groups,
        out EntArchRowModel row)
    {
        var member = (MemberAccessExpressionSyntax)call.Expression;
        string @namespace = NamespaceAt(semanticModel, call.SpanStart);
        if (TryCreateFromType(
            semanticModel.GetTypeInfo(member.Expression).Type,
            semanticModel.Compilation.Assembly,
            @namespace,
            out row))
            return true;

        var selection = new List<PropertyModel>();
        if (!TryParseQuery(member.Expression, semanticModel, groups, selection, [], out var group) || selection.Count == 0)
        {
            row = null!;
            return false;
        }

        string groupType = QualifiedGroup(group);
        string selectType = "";
        foreach (var field in selection)
        {
            string markerType = groupType + "." + field.Name;
            selectType = string.IsNullOrEmpty(selectType)
                ? $"global::AlvorKit.ECS.EntArchSelect<{field.NullableType}, {markerType}, {groupType}>"
                : $"global::AlvorKit.ECS.EntArchSelect<{field.NullableType}, {markerType}, {groupType}, {selectType}>";
        }

        string queryType = $"global::AlvorKit.ECS.EntArchQuery<{groupType}, {selectType}>";
        var fields = DistinctProperties(selection)
            .Select(field => new EntArchRowFieldModel(
                field.Name,
                field.NullableType,
                groupType + "." + field.Name,
                ComponentAccess.WiderAccess(field.GetAccess, field.SetAccess)))
            .ToArray();
        row = CreateModel(@namespace, queryType, fields);
        return true;
    }

    private static bool TryCreateFromType(
        ITypeSymbol? type,
        IAssemblySymbol assembly,
        string @namespace,
        out EntArchRowModel row)
    {
        if (type is not INamedTypeSymbol query ||
            !IsQuery(query) ||
            query.TypeArguments[0].TypeKind == TypeKind.Error)
        {
            row = null!;
            return false;
        }

        var fields = new List<EntArchRowFieldModel>();
        if (!TryParseSelect(query.TypeArguments[1], query.TypeArguments[0], assembly, fields))
        {
            row = null!;
            return false;
        }

        var distinctFields = DistinctFields(fields);
        row = CreateModel(@namespace, query.ToDisplayString(TypeFormat), distinctFields);
        return true;
    }

    private static bool TryParseSelect(
        ITypeSymbol type,
        ITypeSymbol group,
        IAssemblySymbol assembly,
        List<EntArchRowFieldModel> fields)
    {
        if (type is not INamedTypeSymbol select || !IsSelect(select))
        {
            return false;
        }

        if (select.Arity == 4 && !TryParseSelect(select.TypeArguments[3], group, assembly, fields))
            return false;

        if (!SymbolEqualityComparer.Default.Equals(select.TypeArguments[2], group))
            return false;

        string getAccess = AccessAt(select.TypeArguments[1], assembly, "EntArchGetAccess");
        string setAccess = AccessAt(select.TypeArguments[1], assembly, "EntArchSetAccess");
        fields.Add(new(
            select.TypeArguments[1].Name,
            select.TypeArguments[0].ToDisplayString(TypeFormat),
            select.TypeArguments[1].ToDisplayString(TypeFormat),
            ComponentAccess.WiderAccess(getAccess, setAccess)));
        return true;
    }

    private static bool TryParseQuery(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        System.Collections.Immutable.ImmutableArray<InterfaceModel> groups,
        List<PropertyModel> selection,
        HashSet<SyntaxNode> visited,
        out InterfaceModel group)
    {
        expression = Unwrap(expression);
        if (!visited.Add(expression))
        {
            group = null!;
            return false;
        }

        if (expression is IdentifierNameSyntax identifier &&
            TryInitializer(identifier, semanticModel, out var initializer))
        {
            return TryParseQuery(initializer, semanticModel, groups, selection, visited, out group);
        }

        if (expression is not InvocationExpressionSyntax invocation ||
            invocation.Expression is not MemberAccessExpressionSyntax member)
        {
            group = null!;
            return false;
        }

        if (member.Name is GenericNameSyntax rootName &&
            rootName.Identifier.ValueText == "QueryArchetypal" &&
            rootName.TypeArgumentList.Arguments.Count == 1)
        {
            return TryFindGroup(rootName.TypeArgumentList.Arguments[0], groups, out group);
        }

        if (!TryParseQuery(member.Expression, semanticModel, groups, selection, visited, out group))
            return false;

        string? fieldName = member.Name switch
        {
            IdentifierNameSyntax name when name.Identifier.ValueText.StartsWith("With", StringComparison.Ordinal) =>
                name.Identifier.ValueText.Substring("With".Length),
            GenericNameSyntax name when name.Identifier.ValueText == "With" && name.TypeArgumentList.Arguments.Count == 2 =>
                RightmostIdentifier(name.TypeArgumentList.Arguments[1]),
            _ => null
        };
        var field = group.Properties.FirstOrDefault(property =>
            property.Archetypal && property.Name == fieldName);
        if (field == null)
            return false;

        selection.Add(field);
        return true;
    }

    private static bool TryInitializer(
        IdentifierNameSyntax identifier,
        SemanticModel semanticModel,
        out ExpressionSyntax initializer)
    {
        if (semanticModel.GetSymbolInfo(identifier).Symbol is ILocalSymbol local)
        {
            var declarator = local.DeclaringSyntaxReferences
                .Select(reference => reference.GetSyntax())
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault();
            if (declarator?.Initializer != null)
            {
                initializer = declarator.Initializer.Value;
                return true;
            }
        }

        var block = identifier.Ancestors().OfType<BlockSyntax>().FirstOrDefault();
        var fallback = block?.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .Where(candidate => candidate.SpanStart < identifier.SpanStart &&
                candidate.Identifier.ValueText == identifier.Identifier.ValueText &&
                candidate.Initializer != null)
            .LastOrDefault();
        if (fallback?.Initializer != null)
        {
            initializer = fallback.Initializer.Value;
            return true;
        }

        initializer = null!;
        return false;
    }

    private static bool TryFindGroup(
        TypeSyntax type,
        System.Collections.Immutable.ImmutableArray<InterfaceModel> groups,
        out InterfaceModel group)
    {
        string typeName = type.ToString().Replace("global::", "");
        if (typeName.Contains("."))
        {
            group = groups.FirstOrDefault(candidate =>
                (string.IsNullOrEmpty(candidate.Namespace)
                    ? candidate.ClassName
                    : candidate.Namespace + "." + candidate.ClassName) == typeName)!;
            return group != null;
        }

        var matches = groups.Where(candidate => candidate.ClassName == typeName).Take(2).ToArray();
        group = matches.Length == 1 ? matches[0] : null!;
        return matches.Length == 1;
    }

    private static ExpressionSyntax Unwrap(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;

        return expression;
    }

    private static string RightmostIdentifier(SyntaxNode type) => type switch
    {
        IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
        GenericNameSyntax generic => generic.Identifier.ValueText,
        QualifiedNameSyntax qualified => RightmostIdentifier(qualified.Right),
        AliasQualifiedNameSyntax alias => RightmostIdentifier(alias.Name),
        _ => type.ToString().Split('.').Last()
    };

    private static string NamespaceAt(SemanticModel semanticModel, int position)
    {
        var symbol = semanticModel.GetEnclosingSymbol(position);
        var value = symbol?.ContainingNamespace;
        return value == null || value.IsGlobalNamespace ? "" : value.ToDisplayString();
    }

    private static string QualifiedGroup(InterfaceModel group) =>
        string.IsNullOrEmpty(group.Namespace)
            ? "global::" + group.ClassName
            : "global::" + group.Namespace + "." + group.ClassName;

    private static EntArchRowModel CreateModel(
        string @namespace,
        string queryType,
        EntArchRowFieldModel[] fields)
    {
        string id = StableId(@namespace + "|" + queryType);
        return new(
            @namespace,
            "EntArchRowsExtensions_" + id,
            "EntArchRow_" + id,
            "EntArchRows_" + id,
            queryType,
            fields);
    }

    private static string StableId(string value)
    {
        const ulong offset = 14695981039346656037;
        const ulong prime = 1099511628211;
        ulong hash = offset;
        foreach (char character in value)
        {
            hash ^= character;
            hash *= prime;
        }

        return hash.ToString("X16");
    }

    private static bool IsSelect(INamedTypeSymbol select) =>
        select.Name == "EntArchSelect" &&
        select.ContainingNamespace.ToDisplayString() == "AlvorKit.ECS" &&
        select.Arity is 3 or 4;

    private static bool IsQuery(INamedTypeSymbol query) =>
        query.Name == "EntArchQuery" &&
        query.ContainingNamespace.ToDisplayString() == "AlvorKit.ECS" &&
        query.Arity == 2;

    private static string AccessAt(ITypeSymbol marker, IAssemblySymbol assembly, string name)
    {
        string value = marker.GetMembers(name)
            .OfType<IFieldSymbol>()
            .Select(field => field.ConstantValue as string)
            .FirstOrDefault(access => access != null) ?? "public";
        return value == "public" || SymbolEqualityComparer.Default.Equals(marker.ContainingAssembly, assembly)
            ? value
            : "private";
    }

    private static PropertyModel[] DistinctProperties(List<PropertyModel> fields)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        return fields.Where(field => names.Add(field.Name)).ToArray();
    }

    private static EntArchRowFieldModel[] DistinctFields(List<EntArchRowFieldModel> fields)
    {
        var markers = new HashSet<string>(StringComparer.Ordinal);
        return fields.Where(field => markers.Add(field.MarkerType)).ToArray();
    }
}
