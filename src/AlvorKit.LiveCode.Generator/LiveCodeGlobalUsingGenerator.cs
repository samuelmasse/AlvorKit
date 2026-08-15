namespace AlvorKit;

/// <summary>Publishes one executable project's resolved global usings as assembly metadata.</summary>
[Generator]
[ExcludeFromCodeCoverage]
internal sealed class LiveCodeGlobalUsingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var clauses = context.SyntaxProvider.CreateSyntaxProvider(
                static (node, _) =>
                    node is UsingDirectiveSyntax directive &&
                    directive.GlobalKeyword.RawKind != 0,
                static (syntax, _) => CreateClause((UsingDirectiveSyntax)syntax.Node))
            .Where(static clause => clause.Length > 0)
            .Collect();

        context.RegisterSourceOutput(
            clauses,
            static (output, input) =>
            {
                if (input.IsDefaultOrEmpty)
                    return;

                output.AddSource(
                    "LiveCodeGlobalUsings.g.cs",
                    SourceText.From(
                        LiveCodeGlobalUsingSourceEmitter.Emit(input),
                        Encoding.UTF8));
            });
    }

    private static string CreateClause(UsingDirectiveSyntax directive)
    {
        var clause = new StringBuilder();
        if (directive.UnsafeKeyword.RawKind != 0)
            clause.Append("unsafe ");
        if (directive.StaticKeyword.RawKind != 0)
            clause.Append("static ");
        if (directive.Alias is { } alias)
            clause.Append(alias.Name).Append(" = ");
        clause.Append(directive.Name);
        return clause.ToString();
    }
}
