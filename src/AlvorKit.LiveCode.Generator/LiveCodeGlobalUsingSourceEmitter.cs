namespace AlvorKit;

/// <summary>Emits assembly metadata for one compilation's distinct global-using clauses.</summary>
internal static class LiveCodeGlobalUsingSourceEmitter
{
    internal static string Emit(IEnumerable<string> clauses)
    {
        var distinct = new SortedSet<string>(clauses, StringComparer.Ordinal);
        var attributes = string.Join(
            "\n",
            distinct.Select(static clause =>
                LiveCodeGeneratorTemplate.Render(
                    "global-using-attribute.csfrag.tmpl",
                    ("Clause", SymbolDisplay.FormatLiteral(clause, quote: true)))
                .TrimEnd()));
        return LiveCodeGeneratorTemplate.Render(
            "global-usings.cs.tmpl",
            ("Attributes", attributes));
    }
}
