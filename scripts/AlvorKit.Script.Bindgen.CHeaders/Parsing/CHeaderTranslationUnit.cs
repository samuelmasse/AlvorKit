using ClangSharp;
using ClangSharp.Interop;

namespace AlvorKit.Script.Bindgen;

/// <summary>Owns a Clang translation unit and the declarations selected from it.</summary>
internal sealed class CHeaderTranslationUnit : IDisposable
{
    /// <summary>Clang index that owns translation-unit resources.</summary>
    private CXIndex ClangIndex { get; }

    /// <summary>Raw Clang translation-unit handle used for tokenization.</summary>
    public CXTranslationUnit Handle { get; }

    /// <summary>Managed ClangSharp translation unit wrapper.</summary>
    public TranslationUnit Unit { get; }

    /// <summary>Scope used to decide which declarations are considered inputs.</summary>
    public CHeaderScope Scope { get; }

    /// <summary>Top-level declarations that belong to the input scope.</summary>
    public List<Decl> Declarations { get; }

    /// <summary>Stores the parsed Clang handles and filtered declarations.</summary>
    private CHeaderTranslationUnit(
        CXIndex clangIndex,
        CXTranslationUnit handle,
        TranslationUnit unit,
        CHeaderScope scope,
        List<Decl> declarations)
    {
        ClangIndex = clangIndex;
        Handle = handle;
        Unit = unit;
        Scope = scope;
        Declarations = declarations;
    }

    /// <summary>Parses the C file with the configured include roots and target triple.</summary>
    public static CHeaderTranslationUnit Parse(
        BindgenConfig config,
        string translationUnitPath,
        string includeDirectory,
        string filterRoot,
        string libraryDirectory,
        string targetTriple)
    {
        var clangIndex = CXIndex.Create();
        var error = CXTranslationUnit.TryParse(
            clangIndex,
            translationUnitPath,
            BuildArguments(config, includeDirectory, libraryDirectory, targetTriple),
            [],
            CXTranslationUnit_Flags.CXTranslationUnit_DetailedPreprocessingRecord,
            out var handle);
        if (error != CXErrorCode.CXError_Success)
            throw new InvalidOperationException($"clang parse failed: {error}");

        ThrowIfClangReportedErrors(handle);
        var unit = TranslationUnit.GetOrCreate(handle);
        var scope = new CHeaderScope(filterRoot, libraryDirectory, translationUnitPath);
        var declarations = unit.TranslationUnitDecl.Decls
            .Where(declaration => scope.IsInScope(declaration.Location))
            .ToList();
        return new(clangIndex, handle, unit, scope, declarations);
    }

    /// <summary>Releases the Clang translation unit and index.</summary>
    public void Dispose()
    {
        Unit.Dispose();
        ClangIndex.Dispose();
    }

    /// <summary>Builds the command-line arguments passed to Clang.</summary>
    private static string[] BuildArguments(
        BindgenConfig config,
        string includeDirectory,
        string libraryDirectory,
        string targetTriple) =>
        [
            "-x", "c", "-std=c11", $"--target={targetTriple}",
            "-nostdinc", $"-isystem{Path.Combine(AppContext.BaseDirectory, "include")}",
            $"-I{includeDirectory}", .. ImplementationIncludeArguments(config, libraryDirectory),
            $"-I{libraryDirectory}", "-fparse-all-comments",
            .. config.ExtraDefines.Select(define => $"-D{define}")
        ];

    /// <summary>Adds the configured implementation file directory so sibling includes still resolve from temp TUs.</summary>
    private static IEnumerable<string> ImplementationIncludeArguments(BindgenConfig config, string libraryDirectory)
    {
        if (config.ImplFile is not { Length: > 0 } implFile)
            yield break;

        var implDirectory = Path.GetDirectoryName(implFile);
        if (string.IsNullOrEmpty(implDirectory))
            yield break;

        yield return $"-I{Path.Combine(libraryDirectory, implDirectory)}";
    }

    /// <summary>Throws the first Clang diagnostic at error severity or above.</summary>
    private static void ThrowIfClangReportedErrors(CXTranslationUnit handle)
    {
        for (uint i = 0; i < handle.NumDiagnostics; i++)
        {
            var diagnostic = handle.GetDiagnostic(i);
            if (diagnostic.Severity >= CXDiagnosticSeverity.CXDiagnostic_Error)
                throw new InvalidOperationException($"clang: {diagnostic.Format(CXDiagnostic.DefaultDisplayOptions)}");
        }
    }
}
