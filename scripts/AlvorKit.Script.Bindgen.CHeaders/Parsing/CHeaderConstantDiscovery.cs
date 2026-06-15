using ClangSharp;
using ClangSharp.Interop;

namespace AlvorKit.Script.Bindgen;

/// <summary>Discovers supported integer macro constants from the parsed translation unit.</summary>
internal sealed class CHeaderConstantDiscovery(
    BindgenConfig config,
    CHeaderParseState state,
    CHeaderTranslationUnit translationUnit)
{
    /// <summary>Adds macro and configured constants in deterministic managed-name order.</summary>
    public void Discover()
    {
        foreach (var cursor in translationUnit.Unit.TranslationUnitDecl.CursorChildren)
        {
            if (!ShouldConsider(cursor))
                continue;
            var nativeName = cursor.Handle.Spelling.ToString();
            var tokens = MacroTokens(cursor);
            var value = tokens.Count == 0 ? null : ConstantExpressionEvaluator.Evaluate(tokens, state.ValuesByNativeName);
            if (value is null)
                continue;

            state.ValuesByNativeName[nativeName] = value.Value;
            state.NativeNamesInOrder.Add(nativeName);
        }

        var usedManagedNames = state.Functions.Select(function => function.ManagedName)
            .Concat(config.Constants.Keys)
            .ToHashSet();
        foreach (var nativeName in state.NativeNamesInOrder)
            AddDiscoveredConstant(nativeName, usedManagedNames);
        foreach (var (name, value) in config.Constants)
            state.Constants.Add(new(name, value));
        state.Constants.Sort((a, b) => string.Compare(a.ManagedName, b.ManagedName, StringComparison.Ordinal));
    }

    /// <summary>Returns true when a macro definition belongs to the generated API surface.</summary>
    private bool ShouldConsider(Cursor cursor)
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        var nativeName = cursor.Handle.Spelling.ToString();
        return cursor.Handle.Kind == CXCursorKind.CXCursor_MacroDefinition
            && !cursor.Handle.IsMacroFunctionLike
            && translationUnit.Scope.IsInScope(cursor.Handle.Location)
            && prefixes.Any(prefix => nativeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            && !state.ValuesByNativeName.ContainsKey(nativeName)
            && !config.SkipConstants.ContainsKey(nativeName);
    }

    /// <summary>Returns macro tokens after the macro name token.</summary>
    private List<string> MacroTokens(Cursor cursor) =>
        [.. translationUnit.Handle.Tokenize(cursor.Handle.Extent)
            .ToArray()
            .Skip(1)
            .Select(token => token.GetSpelling(translationUnit.Handle).ToString())];

    /// <summary>Adds a discovered macro constant when its managed name is unused.</summary>
    private void AddDiscoveredConstant(string nativeName, HashSet<string> usedManagedNames)
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        var prefix = prefixes.First(namePrefix => nativeName.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        var managedName = CSharpName.FromNativeIdentifier(nativeName, prefix, config.DigitNamePrefix);
        if (usedManagedNames.Add(managedName))
            state.Constants.Add(new(managedName, state.ValuesByNativeName[nativeName]));
    }
}
