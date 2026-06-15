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
        SeedConfiguredNativeValues();

        foreach (var cursor in translationUnit.Unit.TranslationUnitDecl.CursorChildren)
        {
            if (!ShouldConsider(cursor))
                continue;
            var nativeName = cursor.Handle.Spelling.ToString();
            var tokens = MacroTokens(cursor);
            var value = tokens.Count == 0 ? null : ConstantExpressionEvaluator.Evaluate(tokens, state.ValuesByNativeName);
            value ??= ConfiguredNativeValue(nativeName);
            if (value is null)
                continue;

            state.ValuesByNativeName[nativeName] = value.Value;
            state.NativeNamesInOrder.Add(nativeName);
        }

        AddMissingConfiguredNativeValues();

        var usedManagedNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var nativeName in state.NativeNamesInOrder)
            AddDiscoveredConstant(nativeName, usedManagedNames);
        AddConfiguredManagedConstants(usedManagedNames);
        state.ConstantTokens.Sort((a, b) => string.Compare(a.ManagedName, b.ManagedName, StringComparison.Ordinal));
    }

    /// <summary>Seeds native-style configured values so later macro expressions can refer to them.</summary>
    private void SeedConfiguredNativeValues()
    {
        foreach (var (nativeName, value) in config.Constants)
        {
            if (IsNativeConstantName(nativeName) && !config.SkipConstants.ContainsKey(nativeName))
                state.ValuesByNativeName[nativeName] = value;
        }
    }

    /// <summary>Returns true when a macro definition belongs to the generated API surface.</summary>
    private bool ShouldConsider(Cursor cursor)
    {
        var nativeName = cursor.Handle.Spelling.ToString();
        return cursor.Handle.Kind == CXCursorKind.CXCursor_MacroDefinition
            && !cursor.Handle.IsMacroFunctionLike
            && translationUnit.Scope.IsInScope(cursor.Handle.Location)
            && IsNativeConstantName(nativeName)
            && !state.NativeNamesInOrder.Contains(nativeName, StringComparer.Ordinal)
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
            state.ConstantTokens.Add(new(nativeName, managedName, state.ValuesByNativeName[nativeName]));
    }

    /// <summary>Returns a configured fallback value for a considered native macro, if one was supplied.</summary>
    private long? ConfiguredNativeValue(string nativeName) =>
        config.Constants.TryGetValue(nativeName, out var value) ? value : null;

    /// <summary>Adds configured native-style constants that have no visible macro definition.</summary>
    private void AddMissingConfiguredNativeValues()
    {
        foreach (var (nativeName, value) in config.Constants)
        {
            if (!IsNativeConstantName(nativeName)
                || config.SkipConstants.ContainsKey(nativeName)
                || state.NativeNamesInOrder.Contains(nativeName, StringComparer.Ordinal))
                continue;

            state.ValuesByNativeName[nativeName] = value;
            state.NativeNamesInOrder.Add(nativeName);
        }
    }

    /// <summary>Adds managed-name configured constants for values that are not native macro fallbacks.</summary>
    private void AddConfiguredManagedConstants(HashSet<string> usedManagedNames)
    {
        foreach (var (managedName, value) in config.Constants)
        {
            if (!IsNativeConstantName(managedName) && usedManagedNames.Add(managedName))
                state.ConstantTokens.Add(new(managedName, managedName, value));
        }
    }

    /// <summary>Returns true when a configured or discovered name belongs to a generated native prefix.</summary>
    private bool IsNativeConstantName(string nativeName)
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        return prefixes.Any(prefix => nativeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
