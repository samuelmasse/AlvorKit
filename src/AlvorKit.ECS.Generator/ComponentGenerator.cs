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

        context.RegisterSourceOutput(provider, static (context, model) =>
        {
            if (model.Properties.Length == 0)
                return;

            var source = ComponentSourceEmitter.Emit(model);
            context.AddSource($"{model.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }
}
