using ClangSharp;
using ClangSharp.Interop;
using ClangType = ClangSharp.Type;

namespace AlvorKit.Script.Bindgen;

/// <summary>Parses a native C header with libclang and builds a managed binding model.</summary>
public sealed class CHeaderBindingParser(BindgenConfig config, string managedTypePrefix)
{
    private readonly Dictionary<string, BindingEnum> enumByNativeName = [];
    private readonly Dictionary<string, BindingStruct> structByNativeName = [];
    private readonly HashSet<string> failedStructs = [];
    private readonly Dictionary<string, RecordDecl> recordByNativeName = [];
    private readonly List<BindingFunction> functions = [];
    private readonly List<BindingConstant> constants = [];
    private readonly Dictionary<(string ElementType, int Count), InlineBufferDefinition> inlineBuffers = [];
    private readonly List<string> skippedFunctions = [];
    private readonly SortedSet<string> sizeofTypes = [];
    private CXIndex clangIndex;
    private CXTranslationUnit translationUnitHandle;
    private TranslationUnit? translationUnit;
    private string sourceRoot = "";
    private string shimRoot = "";
    private string translationUnitFileName = "";

    public BindingModel Parse(string translationUnitPath, string includeDirectory, string filterRoot, string libraryDirectory, string targetTriple)
    {
        var declarations = ParseTranslationUnit(translationUnitPath, includeDirectory, filterRoot, libraryDirectory, targetTriple);
        DiscoverEnums(declarations);
        IndexRecords(declarations);
        foreach (var nativeName in config.TransparentStructs)
            ResolveStruct(nativeName);
        DiscoverFunctions(declarations);
        DiscoverMacroConstants();

        var model = new BindingModel(
            [.. enumByNativeName.Values.DistinctBy(e => e.NativeName)],
            [.. structByNativeName.Values],
            functions,
            constants,
            [.. inlineBuffers.Values],
            skippedFunctions,
            [.. sizeofTypes]);
        DisposeClang();
        return model;
    }

    /// <summary>
    /// Re-parses for another target and verifies every emitted struct still has
    /// natural layout for that ABI.
    /// </summary>
    public static void ValidateNaturalLayout(
        BindgenConfig config,
        string translationUnitPath,
        string includeDirectory,
        string filterRoot,
        string libraryDirectory,
        string targetTriple,
        IEnumerable<string> nativeStructNames)
    {
        var parser = new CHeaderBindingParser(config, config.ApiClass);
        var declarations = parser.ParseTranslationUnit(translationUnitPath, includeDirectory, filterRoot, libraryDirectory, targetTriple);
        parser.IndexRecords(declarations);
        foreach (var nativeName in nativeStructNames)
        {
            if (parser.recordByNativeName.TryGetValue(nativeName, out var record))
                parser.ValidateNaturalRecordLayout(record, targetTriple);
        }
        parser.DisposeClang();
    }

    private List<Decl> ParseTranslationUnit(
        string translationUnitPath,
        string includeDirectory,
        string filterRoot,
        string libraryDirectory,
        string targetTriple)
    {
        var arguments = new List<string>
        {
            "-x", "c", "-std=c11", $"--target={targetTriple}",
            "-nostdinc", $"-isystem{Path.Combine(AppContext.BaseDirectory, "include")}",
            $"-I{includeDirectory}", $"-I{libraryDirectory}", "-fparse-all-comments"
        };
        arguments.AddRange(config.ExtraDefines.Select(define => $"-D{define}"));

        clangIndex = CXIndex.Create();
        var error = CXTranslationUnit.TryParse(
            clangIndex,
            translationUnitPath,
            arguments.ToArray(),
            [],
            CXTranslationUnit_Flags.CXTranslationUnit_DetailedPreprocessingRecord,
            out translationUnitHandle);
        if (error != CXErrorCode.CXError_Success)
            throw new InvalidOperationException($"clang parse failed: {error}");
        ThrowIfClangReportedErrors();

        translationUnit = TranslationUnit.GetOrCreate(translationUnitHandle);
        sourceRoot = Path.GetFullPath(filterRoot);
        shimRoot = Path.GetFullPath(libraryDirectory);
        translationUnitFileName = Path.GetFileName(translationUnitPath);
        return translationUnit.TranslationUnitDecl.Decls.Where(declaration => IsInScope(declaration.Location)).ToList();
    }

    private void ThrowIfClangReportedErrors()
    {
        for (uint i = 0; i < translationUnitHandle.NumDiagnostics; i++)
        {
            var diagnostic = translationUnitHandle.GetDiagnostic(i);
            if (diagnostic.Severity >= CXDiagnosticSeverity.CXDiagnostic_Error)
                throw new InvalidOperationException($"clang: {diagnostic.Format(CXDiagnostic.DefaultDisplayOptions)}");
        }
    }

    private void DisposeClang()
    {
        translationUnit?.Dispose();
        translationUnit = null;
        clangIndex.Dispose();
    }

    private bool IsInScope(CXSourceLocation location)
    {
        location.GetExpansionLocation(out var file, out _, out _, out _);
        var fileName = file.Name.ToString();
        if (fileName.Length == 0)
            return false;

        var path = Path.GetFullPath(fileName);
        return path.StartsWith(sourceRoot, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(shimRoot, StringComparison.OrdinalIgnoreCase)
            || Path.GetFileName(path) == translationUnitFileName;
    }

    private void DiscoverEnums(List<Decl> declarations)
    {
        TypedefDecl? previousTypedef = null;
        uint previousTypedefLine = 0;

        foreach (var declaration in declarations)
        {
            declaration.Location.GetExpansionLocation(out _, out var line, out _, out _);
            if (declaration is TypedefDecl typedef)
            {
                if (typedef.UnderlyingType.CanonicalType is EnumType enumType)
                {
                    AddEnum(typedef.Name, enumType.Decl, enumType.Decl.IntegerType, typedef);
                    continue;
                }

                previousTypedef = typedef;
                previousTypedefLine = line;
            }
            else if (declaration is EnumDecl enumDecl && previousTypedef is not null && line == previousTypedefLine)
            {
                AddEnum(previousTypedef.Name, enumDecl, previousTypedef.UnderlyingType, previousTypedef);
            }
        }
    }

    private void AddEnum(string nativeName, EnumDecl enumDecl, ClangType underlyingType, TypedefDecl typedef)
    {
        if (enumByNativeName.ContainsKey(nativeName))
            return;

        var lookupName = nativeName.TrimStart('_');
        var members = ReadEnumMembers(enumDecl);
        var managedUnderlyingType = MapIntegerType(underlyingType);
        var range = RangeFor(managedUnderlyingType);
        foreach (var outOfRange in members.Where(m => m.Value < range.Min || m.Value > range.Max).ToList())
        {
            Console.WriteLine($"  dropping {nativeName}.{outOfRange.ManagedName} = {outOfRange.Value} (out of range for underlying type)");
            members.Remove(outOfRange);
        }

        enumByNativeName[lookupName] = enumByNativeName[nativeName] = new(
            nativeName,
            TypeName(lookupName),
            managedUnderlyingType,
            ShouldEmitFlagsAttribute(nativeName, members),
            members,
            XmlDocComment.Parse(typedef.Handle.RawCommentText.ToString())?.Summary);
    }

    private static (long Min, long Max) RangeFor(string integerType) => integerType switch
    {
        "byte" => (0L, byte.MaxValue),
        "sbyte" => (sbyte.MinValue, sbyte.MaxValue),
        "ushort" => (0L, ushort.MaxValue),
        "short" => (short.MinValue, short.MaxValue),
        "uint" => (0L, uint.MaxValue),
        _ => (long.MinValue, long.MaxValue)
    };

    private List<BindingEnumMember> ReadEnumMembers(EnumDecl enumDecl)
    {
        var members = new List<BindingEnumMember>();
        var declarationLines = new List<uint>();
        var commentLines = new List<uint>();

        foreach (var enumerator in enumDecl.Enumerators)
        {
            enumerator.Location.GetExpansionLocation(out _, out var declarationLine, out _, out _);
            enumerator.Handle.CommentRange.Start.GetExpansionLocation(out _, out var commentLine, out _, out _);
            members.Add(new(
                CSharpName.FromNativeIdentifier(enumerator.Name, config.Prefix, config.DigitNamePrefix),
                enumerator.Handle.EnumConstantDeclValue,
                XmlDocComment.Member(enumerator.Handle.RawCommentText.ToString())));
            declarationLines.Add(declarationLine);
            commentLines.Add(commentLine);
        }

        FixTrailingMemberComments(members, declarationLines, commentLines);
        return members;
    }

    private static void FixTrailingMemberComments(List<BindingEnumMember> members, List<uint> declarationLines, List<uint> commentLines)
    {
        for (var i = 1; i < members.Count; i++)
        {
            if (members[i].Documentation is null || commentLines[i] != declarationLines[i - 1] || declarationLines[i] == declarationLines[i - 1])
                continue;

            if (members[i - 1].Documentation is null)
                members[i - 1] = members[i - 1] with { Documentation = members[i].Documentation };
            members[i] = members[i] with { Documentation = null };
        }
    }

    private void IndexRecords(List<Decl> declarations)
    {
        foreach (var declaration in declarations)
        {
            if (declaration is RecordDecl { Name.Length: > 0 } record && record.Definition is not null)
                recordByNativeName[record.Name] = (RecordDecl)record.Definition;
            else if (declaration is TypedefDecl typedef
                && typedef.UnderlyingType.CanonicalType is RecordType recordType
                && recordType.Decl is RecordDecl { Definition: not null } aliased)
                recordByNativeName.TryAdd(typedef.Name, (RecordDecl)aliased.Definition);
        }
    }

    private BindingStruct? ResolveStruct(string nativeName)
    {
        if (structByNativeName.TryGetValue(nativeName, out var existing))
            return existing;
        if (failedStructs.Contains(nativeName) || !recordByNativeName.TryGetValue(nativeName, out var record))
            return null;
        return BuildStruct(record, nativeName);
    }

    private BindingStruct? BuildStruct(RecordDecl record, string nativeName)
    {
        var isUnion = record.Handle.Kind == CXCursorKind.CXCursor_UnionDecl;
        var built = new BindingStruct(
            nativeName,
            TypeName(nativeName),
            isUnion,
            (int)record.TypeForDecl.Handle.SizeOf,
            [],
            XmlDocComment.Parse(record.Handle.RawCommentText.ToString())?.Summary);
        structByNativeName[nativeName] = built;

        foreach (var field in record.Fields)
        {
            var managedType = field.Name.Length == 0 ? null : MapStructFieldType(field, nativeName);
            if (managedType is null)
            {
                if (isUnion)
                    continue;

                structByNativeName.Remove(nativeName);
                failedStructs.Add(nativeName);
                return null;
            }

            built.Fields.Add(new(
                CSharpName.FromNativeIdentifier(field.Name, config.Prefix, config.DigitNamePrefix),
                managedType,
                (int)(field.Handle.OffsetOfField / 8),
                XmlDocComment.Member(field.Handle.RawCommentText.ToString())));
        }

        ValidateNaturalRecordLayout(record, "primary");
        return built;
    }

    private string? MapStructFieldType(FieldDecl field, string parentNativeName)
    {
        var canonical = field.Type.Handle.CanonicalType;
        if (canonical.kind == CXTypeKind.CXType_ConstantArray)
            return MapInlineArray(canonical, parentNativeName);
        if (canonical.kind == CXTypeKind.CXType_Record)
            return MapRecordField(field, parentNativeName);
        return MapNativeType(field.Type.Handle);
    }

    private string? MapRecordField(FieldDecl field, string parentNativeName)
    {
        var nativeTypeName = CleanTypeSpelling(field.Type.Handle);
        if (recordByNativeName.ContainsKey(nativeTypeName))
            return ResolveStruct(nativeTypeName)?.ManagedName;

        if (field.Type.CanonicalType is not RecordType { Decl.Definition: RecordDecl definition })
            return null;

        var synthesizedName = $"{parentNativeName}_{field.Name}";
        if (structByNativeName.TryGetValue(synthesizedName, out var existing))
            return existing.ManagedName;
        if (failedStructs.Contains(synthesizedName))
            return null;
        return BuildStruct(definition, synthesizedName)?.ManagedName;
    }

    private string? MapInlineArray(CXType arrayType, string parentNativeName)
    {
        var count = (int)arrayType.ArraySize;
        var element = arrayType.ElementType;
        string? elementType;
        if (element.CanonicalType.kind == CXTypeKind.CXType_ConstantArray)
            elementType = MapInlineArray(element.CanonicalType, parentNativeName);
        else if (element.CanonicalType.kind == CXTypeKind.CXType_Record)
            elementType = ResolveStruct(CleanTypeSpelling(element))?.ManagedName;
        else
            elementType = MapNativeType(element);
        if (elementType is null)
            return null;

        var key = (elementType, count);
        if (!inlineBuffers.TryGetValue(key, out var buffer))
        {
            var elementName = char.ToUpperInvariant(elementType[0]) + elementType[1..];
            buffer = new($"{elementName}Buffer{count}", elementType, count);
            inlineBuffers[key] = buffer;
        }
        return buffer.ManagedName;
    }

    private void DiscoverFunctions(List<Decl> declarations)
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        foreach (var declaration in declarations)
        {
            if (declaration is not FunctionDecl function)
                continue;

            var matchingPrefix = prefixes.FirstOrDefault(function.Name.StartsWith);
            if (matchingPrefix is null)
                continue;
            if (config.Skip.TryGetValue(function.Name, out var skipReason))
            {
                skippedFunctions.Add($"{function.Name} ({skipReason})");
                continue;
            }
            if (function.IsVariadic)
            {
                skippedFunctions.Add($"{function.Name} (variadic)");
                continue;
            }
            if (function.StorageClass == CX_StorageClass.CX_SC_Static)
                continue;

            var boundFunction = TryBindFunction(function, matchingPrefix);
            if (boundFunction is not null)
            {
                functions.Add(boundFunction);
                TrackSizeofCandidate(function);
            }
        }
    }

    private BindingFunction? TryBindFunction(FunctionDecl function, string matchingPrefix)
    {
        var returnType = MapNativeType(function.ReturnType.Handle, isReturn: true);
        if (returnType is null)
        {
            skippedFunctions.Add($"{function.Name} (return type: {function.ReturnType.AsString})");
            return null;
        }

        var parameters = new List<BindingParameter>();
        foreach (var parameter in function.Parameters)
        {
            var parameterBinding = TryBindParameter(function, parameter, parameters.Count);
            if (parameterBinding is null)
                return null;
            parameters.Add(parameterBinding);
        }

        return new(
            function.Name,
            CSharpName.FromNativeIdentifier(function.Name, matchingPrefix, config.DigitNamePrefix),
            returnType,
            returnType == "bool" ? BoolMarshaller(function.ReturnType.Handle) : null,
            parameters,
            XmlDocComment.Parse(function.Handle.RawCommentText.ToString()));
    }

    private BindingParameter? TryBindParameter(FunctionDecl function, ParmVarDecl parameter, int index)
    {
        var modifier = ParameterModifier(function.Name, parameter.Name);
        var managedType = modifier.Length > 0
            ? MapNativeType(parameter.Type.Handle.PointeeType)
            : MapNativeType(parameter.Type.Handle, isParam: true);
        if (managedType is null)
        {
            skippedFunctions.Add($"{function.Name} (param {parameter.Name}: {parameter.Type.AsString})");
            return null;
        }

        var nativeName = parameter.Name.Length > 0 ? parameter.Name : $"arg{index}";
        return new(
            CSharpName.Parameter(nativeName),
            managedType,
            modifier,
            RequiresUtf8StringMarshalling: modifier.Length == 0 && managedType == "string",
            BoolMarshaller: managedType == "bool" && modifier.Length == 0 ? BoolMarshaller(parameter.Type.Handle) : null);
    }

    private string ParameterModifier(string functionName, string parameterName)
    {
        if (config.OutParams.GetValueOrDefault(functionName, []).Contains(parameterName))
            return "out";
        if (config.InParams.GetValueOrDefault(functionName, []).Contains(parameterName))
            return "in";
        return "";
    }

    private static string BoolMarshaller(CXType type) => type.CanonicalType.SizeOf == 4 ? "I4" : "U1";

    private void TrackSizeofCandidate(FunctionDecl function)
    {
        var lastParameter = function.Parameters.LastOrDefault();
        if (lastParameter is null)
            return;

        var canonical = lastParameter.Type.Handle.CanonicalType;
        if (canonical.kind != CXTypeKind.CXType_Pointer || canonical.PointeeType.CanonicalType.kind != CXTypeKind.CXType_Record)
            return;

        var nativeTypeName = CleanTypeSpelling(lastParameter.Type.Handle.PointeeType);
        if (recordByNativeName.ContainsKey(nativeTypeName) && function.Name.StartsWith(nativeTypeName + "_init"))
            sizeofTypes.Add(nativeTypeName);
    }

    private void DiscoverMacroConstants()
    {
        string[] prefixes = [config.Prefix, .. config.ExtraPrefixes];
        var valuesByNativeName = new Dictionary<string, long>();
        var nativeNamesInOrder = new List<string>();

        foreach (var cursor in translationUnit!.TranslationUnitDecl.CursorChildren)
        {
            if (cursor.Handle.Kind != CXCursorKind.CXCursor_MacroDefinition
                || cursor.Handle.IsMacroFunctionLike
                || !IsInScope(cursor.Handle.Location))
                continue;

            var nativeName = cursor.Handle.Spelling.ToString();
            if (!prefixes.Any(prefix => nativeName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                || valuesByNativeName.ContainsKey(nativeName)
                || config.SkipConstants.ContainsKey(nativeName))
                continue;

            var tokens = translationUnitHandle.Tokenize(cursor.Handle.Extent).ToArray()
                .Skip(1)
                .Select(token => token.GetSpelling(translationUnitHandle).ToString())
                .ToList();
            if (tokens.Count == 0)
                continue;

            var value = ConstantExpressionEvaluator.Evaluate(tokens, valuesByNativeName);
            if (value is null)
                continue;

            valuesByNativeName[nativeName] = value.Value;
            nativeNamesInOrder.Add(nativeName);
        }

        var usedManagedNames = functions.Select(function => function.ManagedName)
            .Concat(config.Constants.Keys)
            .ToHashSet();
        foreach (var nativeName in nativeNamesInOrder)
        {
            var prefix = prefixes.First(p => nativeName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            var managedName = CSharpName.FromNativeIdentifier(nativeName, prefix, config.DigitNamePrefix);
            if (usedManagedNames.Add(managedName))
                constants.Add(new(managedName, valuesByNativeName[nativeName]));
        }
        foreach (var (name, value) in config.Constants)
            constants.Add(new(name, value));
        constants.Sort((a, b) => string.Compare(a.ManagedName, b.ManagedName, StringComparison.Ordinal));
    }

    private string TypeName(string nativeName) =>
        config.TypeRenames.GetValueOrDefault(nativeName)
        ?? CSharpName.FromNativeTypeName(nativeName, config.Prefix, managedTypePrefix, config.DigitNamePrefix);

    private string? MapNativeType(CXType type, bool isParam = false, bool isReturn = false)
    {
        var spelling = CleanTypeSpelling(type);

        if (spelling == $"{config.Prefix}bool" || ((isParam || isReturn) && config.BoolTypes.Contains(spelling)))
            return "bool";
        if (enumByNativeName.TryGetValue(spelling, out var enumType))
            return enumType.ManagedName;
        if (spelling == "size_t")
            return "nuint";

        var canonical = type.CanonicalType;
        switch (canonical.kind)
        {
            case CXTypeKind.CXType_Void: return "void";
            case CXTypeKind.CXType_Bool: return "bool";
            case CXTypeKind.CXType_UChar or CXTypeKind.CXType_Char_U: return "byte";
            case CXTypeKind.CXType_SChar or CXTypeKind.CXType_Char_S: return "sbyte";
            case CXTypeKind.CXType_UShort: return "ushort";
            case CXTypeKind.CXType_Short: return "short";
            case CXTypeKind.CXType_UInt: return "uint";
            case CXTypeKind.CXType_Int: return "int";
            case CXTypeKind.CXType_ULong: return "CULong";
            case CXTypeKind.CXType_Long: return "CLong";
            case CXTypeKind.CXType_ULongLong: return "ulong";
            case CXTypeKind.CXType_LongLong: return "long";
            case CXTypeKind.CXType_Float: return "float";
            case CXTypeKind.CXType_Double: return "double";
        }

        if (canonical.kind == CXTypeKind.CXType_Pointer)
        {
            var pointee = canonical.PointeeType;
            if (pointee.CanonicalType.kind is CXTypeKind.CXType_FunctionProto or CXTypeKind.CXType_FunctionNoProto)
                return "nint";
            if (isParam && pointee.kind is CXTypeKind.CXType_Char_S or CXTypeKind.CXType_Char_U && pointee.IsConstQualified)
                return "string";
            return "nint";
        }

        if (isParam && canonical.kind is CXTypeKind.CXType_ConstantArray or CXTypeKind.CXType_IncompleteArray)
            return "nint";

        if (canonical.kind == CXTypeKind.CXType_Record)
            return ResolveStruct(spelling)?.ManagedName;

        return null;
    }

    private static string MapIntegerType(ClangType underlyingType) => underlyingType.CanonicalType.Handle.kind switch
    {
        CXTypeKind.CXType_UChar or CXTypeKind.CXType_Char_U => "byte",
        CXTypeKind.CXType_SChar or CXTypeKind.CXType_Char_S => "sbyte",
        CXTypeKind.CXType_UShort => "ushort",
        CXTypeKind.CXType_Short => "short",
        CXTypeKind.CXType_UInt => "uint",
        CXTypeKind.CXType_ULongLong => "ulong",
        CXTypeKind.CXType_LongLong => "long",
        _ => "int"
    };

    private void ValidateNaturalRecordLayout(RecordDecl record, string targetName)
    {
        var isUnion = record.Handle.Kind == CXCursorKind.CXCursor_UnionDecl;
        var nextNaturalOffset = 0L;

        foreach (var field in record.Fields)
        {
            var bitOffset = field.Handle.OffsetOfField;
            if (bitOffset % 8 != 0)
                throw new InvalidOperationException($"{targetName}: {record.Name}.{field.Name} is a bitfield");

            var actualOffset = bitOffset / 8;
            var fieldSize = field.Type.Handle.SizeOf;
            var fieldAlignment = field.Type.Handle.AlignOf;
            var expectedOffset = isUnion ? 0 : Align(nextNaturalOffset, fieldAlignment);
            if (actualOffset != expectedOffset)
                throw new InvalidOperationException(
                    $"{targetName}: {record.Name}.{field.Name} at offset {actualOffset}, natural layout expects {expectedOffset} - needs manual handling");
            nextNaturalOffset = expectedOffset + fieldSize;

            if (field.Type.CanonicalType.Handle.kind == CXTypeKind.CXType_Record
                && recordByNativeName.TryGetValue(CleanTypeSpelling(field.Type.Handle), out var nestedRecord))
                ValidateNaturalRecordLayout(nestedRecord, targetName);
        }
    }

    private static long Align(long offset, long alignment) => (offset + alignment - 1) / alignment * alignment;

    private bool ShouldEmitFlagsAttribute(string nativeName, List<BindingEnumMember> members)
    {
        if (config.FlagsEnums.Contains(nativeName))
            return true;

        var nonZeroValues = members.Select(member => member.Value).Where(value => value > 0).Distinct().ToList();
        var powerOfTwoValues = nonZeroValues.Where(value => (value & (value - 1)) == 0).Distinct().ToList();
        if (powerOfTwoValues.Count < 3 || powerOfTwoValues.Count < nonZeroValues.Count * 0.6 || nonZeroValues.Max() <= nonZeroValues.Count)
            return false;

        var combinedBits = powerOfTwoValues.Aggregate(0L, (bits, value) => bits | value);
        return nonZeroValues.All(value => (value & ~combinedBits) == 0);
    }

    private static string CleanTypeSpelling(CXType type) => type.Spelling.ToString()
        .Replace("const ", "")
        .Replace("struct ", "")
        .Replace("union ", "")
        .Replace("enum ", "")
        .Trim();
}
