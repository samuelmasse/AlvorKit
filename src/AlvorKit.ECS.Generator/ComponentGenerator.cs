namespace AlvorKit.ECS.Generator;

[Generator]
[ExcludeFromCodeCoverage]
internal sealed class ComponentGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            ComponentModelFactory.ComponentsAttributeName,
            static (node, _) => node is InterfaceDeclarationSyntax,
            static (ctx, ct) => ComponentModelFactory.Create(ctx, ct));

        var rowCalls = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => IsRowsCall(node),
            static (ctx, _) => (InvocationExpressionSyntax)ctx.Node);

        context.RegisterSourceOutput(provider, static (context, model) =>
        {
            if (model.Properties.Length == 0)
                return;

            var source = ComponentSourceEmitter.Emit(model);
            context.AddSource($"{model.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        });

        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()).Combine(rowCalls.Collect()),
            static (context, input) =>
            {
                var rows = EntArchRowModelFactory.Create(
                    input.Left.Left,
                    input.Left.Right,
                    input.Right,
                    context.CancellationToken);
                foreach (var row in rows)
                {
                    var source = EntArchRowSourceEmitter.Emit(row);
                    context.AddSource($"{row.ExtensionType}.g.cs", SourceText.From(source, Encoding.UTF8));
                }
            });
    }

    private static bool IsRowsCall(SyntaxNode node) =>
        node is InvocationExpressionSyntax
        {
            ArgumentList.Arguments.Count: 0,
            Expression: MemberAccessExpressionSyntax
            {
                Name: IdentifierNameSyntax { Identifier.ValueText: "Rows" }
            }
        };
}
