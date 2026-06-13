namespace AlvorKit.Script.Bindgen;

/// <summary>
/// Emits the convenience overloads for the generated GL contract, derived from the registry len
/// attributes: spans over counted arrays (count inferred), generic spans over sized or configured
/// void buffers, UTF-8 strings over GLchar pointers, singular forms of the Gen/Create/Delete
/// commands, out scalars for the single-value getters and string returns for the info-log shape.
/// </summary>
public sealed class GlExtensionsEmitter(BindgenConfig config)
{
    private enum LenKind { None, Literal, ParamRef, CompSize, Unknown }

    /// <summary>A parsed len attribute: "n" or "count*3" (ParamRef), "4" (Literal), "COMPSIZE(pname)".</summary>
    private readonly record struct LenInfo(LenKind Kind, int ParamIndex, int Divisor, string[] CompSizeArgs)
    {
        public static readonly LenInfo None = new(LenKind.None, -1, 1, []);
    }

    private readonly HashSet<string> signatures = [];
    private HashSet<string> commandNames = [];

    public string? Emit(GlBindingModel model, StringBuilder sourceHeader)
    {
        commandNames = [.. model.Commands.Select(command => command.ManagedName)];
        var body = new StringBuilder();
        foreach (var command in model.Commands)
        {
            AppendCombined(body, command);
            AppendSingular(body, command);
            AppendOutScalar(body, command);
            AppendInfoLog(body, command);
            AppendSingleSource(body, command);
        }
        if (body.Length == 0)
            return null;

        var output = sourceHeader;
        output.AppendLine("using System.Text;");
        output.AppendLine();
        output.AppendLine($"namespace {config.Namespace};");
        output.AppendLine();
        output.AppendLine("/// <summary>");
        output.AppendLine($"/// Convenience overloads for the <see cref=\"{config.ApiClass}\"/> commands, derived from the registry buffer");
        output.AppendLine("/// metadata: counted pointers become spans with the count inferred, sized void buffers become");
        output.AppendLine("/// generic spans, GLchar pointers become UTF-8 marshalled strings, the Gen/Create/Delete");
        output.AppendLine("/// families gain singular forms, single-value getters gain out overloads and the info-log");
        output.AppendLine("/// shape returns a string. Spans are pinned for the duration of the call.");
        output.AppendLine("/// </summary>");
        output.AppendLine($"public static unsafe class {config.ApiClass}Extensions");
        output.AppendLine("{");
        output.Append(body);
        output.AppendLine("    // A span holds at most int.MaxValue elements, so the product fits 64-bit nuint for any unmanaged T;");
        output.AppendLine("    // checked() covers 32-bit processes, where a span describing more memory than the address space");
        output.AppendLine("    // would otherwise wrap into a silently wrong length.");
        output.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        output.AppendLine("    private static nint ByteLength<T>(ReadOnlySpan<T> span) where T : unmanaged =>");
        output.AppendLine("        checked((nint)((nuint)span.Length * (nuint)sizeof(T)));");
        output.AppendLine("}");
        return output.ToString();
    }

    private LenInfo ParseLen(GlCommand command, GlParameter parameter)
    {
        if (parameter.Len is null)
            return LenInfo.None;
        if (int.TryParse(parameter.Len, out var literal))
            return new(LenKind.Literal, -1, literal, []);
        if (parameter.Len.StartsWith("COMPSIZE(") && parameter.Len.EndsWith(")"))
            return new(LenKind.CompSize, -1, 1, parameter.Len["COMPSIZE(".Length..^1].Split(',', StringSplitOptions.RemoveEmptyEntries));

        var match = Regex.Match(parameter.Len, @"^(\w+)(?:\*(\d+))?$");
        if (!match.Success)
            return new(LenKind.Unknown, -1, 1, []);
        var index = command.Parameters.FindIndex(candidate => candidate.NativeName == match.Groups[1].Value);
        if (index < 0 || command.Parameters[index].PointerDepth != 0)
            return new(LenKind.Unknown, -1, 1, []);
        return new(LenKind.ParamRef, index, match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 1, []);
    }

    private enum Plan { Keep, SpanTyped, SpanGenericSized, SpanGenericUnsized, StringIn, Dropped }

    /// <summary>
    /// The single combined overload: every applicable parameter transform applied at once.
    /// Typed counted pointers become spans and their count parameter is dropped when every
    /// referrer is spanned; sized void buffers become generic spans dropping the byte size;
    /// const GLchar pointers become strings, dropping a paired length parameter when the
    /// registry names one.
    /// </summary>
    private void AppendCombined(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        var plans = new Plan[parameters.Count];
        var argument = new string?[parameters.Count];
        var checks = new List<(string Name, string Against)>();
        var configured = config.SpanParams.GetValueOrDefault(command.NativeName, []);

        // Strings first: a paired length parameter named by COMPSIZE is replaced by the UTF-8 byte count.
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            if (parameter is not { PointerDepth: 1, PointeeIsChar: true, PointeeIsConst: true })
                continue;
            plans[i] = Plan.StringIn;
            argument[i] = $"(nint){Local(parameter)}Ptr";
            foreach (var lengthArg in ParseLen(command, parameter) is { Kind: LenKind.CompSize } len ? len.CompSizeArgs : [])
            {
                var paired = parameters.FindIndex(candidate => candidate.NativeName == lengthArg && candidate is { PointerDepth: 0, ManagedType: "int" or "uint" });
                if (paired < 0)
                    continue;
                plans[paired] = Plan.Dropped;
                argument[paired] = CountExpression(parameters[paired], $"{Local(parameter)}Utf8.Length - 1");
            }
        }

        // Typed spans, collecting the counted references to decide which count parameters drop.
        var referencesByCount = new Dictionary<int, List<(int Pointer, int Divisor)>>();
        var spannedPointers = new HashSet<int>();
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            if (plans[i] != Plan.Keep)
                continue;
            var len = ParseLen(command, parameter);

            if (parameter is { PointeeType: not null, PointeeIsChar: false })
            {
                if (len.Kind is not (LenKind.ParamRef or LenKind.Literal or LenKind.CompSize))
                    continue;
                // A writable single-value pointer is the out overload's territory, not a span's.
                if (len is { Kind: LenKind.Literal, Divisor: 1 } && !parameter.PointeeIsConst)
                    continue;
                plans[i] = Plan.SpanTyped;
                argument[i] = $"(nint){Local(parameter)}Ptr";
                spannedPointers.Add(i);
                if (len.Kind == LenKind.ParamRef)
                {
                    if (!referencesByCount.TryGetValue(len.ParamIndex, out var references))
                        referencesByCount[len.ParamIndex] = references = [];
                    references.Add((i, len.Divisor));
                }
            }
            else if (parameter is { PointerDepth: 1, PointeeType: null, PointeeIsChar: false })
            {
                // A sized void buffer (len references a GLsizeiptr) or a configured length-less one.
                if (len.Kind == LenKind.ParamRef && parameters[len.ParamIndex].ManagedType == "nint")
                {
                    plans[i] = Plan.SpanGenericSized;
                    argument[i] = $"(nint){Local(parameter)}Ptr";
                    plans[len.ParamIndex] = Plan.Dropped;
                    argument[len.ParamIndex] = $"ByteLength<{TypeParameter(command, i)}>({parameter.ManagedName})";
                }
                else if (configured.Contains(parameter.NativeName))
                {
                    plans[i] = Plan.SpanGenericUnsized;
                    argument[i] = $"(nint){Local(parameter)}Ptr";
                }
            }
        }

        // A count drops only when everything referencing it is a span in this overload; otherwise
        // the spans tied to it revert, since a span alongside its own count parameter helps nobody.
        foreach (var (count, references) in referencesByCount)
        {
            var referrers = parameters
                .Select((parameter, index) => (parameter, index))
                .Where(candidate => ParseLen(command, candidate.parameter) is { Kind: LenKind.ParamRef } len && len.ParamIndex == count);
            if (plans[count] != Plan.Keep || !referrers.All(referrer => spannedPointers.Contains(referrer.index)))
            {
                foreach (var (pointer, _) in references)
                {
                    plans[pointer] = Plan.Keep;
                    spannedPointers.Remove(pointer);
                }
                continue;
            }

            var (firstPointer, firstDivisor) = references[0];
            var first = parameters[firstPointer];
            plans[count] = Plan.Dropped;
            argument[count] = CountExpression(parameters[count],
                firstDivisor == 1 ? $"{first.ManagedName}.Length" : $"{first.ManagedName}.Length / {firstDivisor}");
            foreach (var (pointer, divisor) in references.Skip(1).Where(reference => reference.Divisor == firstDivisor))
                checks.Add((parameters[pointer].ManagedName, first.ManagedName));
        }

        if (!plans.Any(plan => plan != Plan.Keep && plan != Plan.Dropped))
            return;

        // Signature, generic type parameters and their constraints.
        var typeParameters = new List<string>();
        var signature = new List<string> { $"this {config.ApiClass} gl" };
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            switch (plans[i])
            {
                case Plan.Keep:
                    signature.Add($"{parameter.ManagedType} {parameter.ManagedName}");
                    argument[i] = parameter.ManagedName;
                    break;
                case Plan.SpanTyped:
                    signature.Add($"{SpanType(parameter, parameter.PointeeType!)} {parameter.ManagedName}");
                    break;
                case Plan.SpanGenericSized or Plan.SpanGenericUnsized:
                    var typeParameter = TypeParameter(command, i);
                    typeParameters.Add(typeParameter);
                    signature.Add($"{SpanType(parameter, typeParameter)} {parameter.ManagedName}");
                    break;
                case Plan.StringIn:
                    signature.Add($"string {parameter.ManagedName}");
                    break;
            }
        }

        var generics = typeParameters.Count > 0 ? $"<{string.Join(", ", typeParameters)}>" : "";
        var constraints = string.Concat(typeParameters.Select(typeParameter => $" where {typeParameter} : unmanaged"));
        if (!signatures.Add($"{command.ManagedName}{generics}({string.Join(", ", signature)})"))
            return;

        output.AppendLine($"    /// <inheritdoc cref=\"{config.ApiClass}.{command.ManagedName}\"/>");
        output.AppendLine($"    public static {command.ReturnType} {command.ManagedName}{generics}({string.Join(", ", signature)}){constraints}");
        output.AppendLine("    {");
        foreach (var (name, against) in checks)
        {
            output.AppendLine($"        if ({name}.Length != {against}.Length)");
            output.AppendLine($"            throw new ArgumentException(\"Must have the same length as {against}.\", nameof({name}));");
        }
        for (var i = 0; i < parameters.Count; i++)
            if (plans[i] == Plan.StringIn)
                output.AppendLine($"        var {Local(parameters[i])}Utf8 = Encoding.UTF8.GetBytes({parameters[i].ManagedName} + '\\0');");
        for (var i = 0; i < parameters.Count; i++)
        {
            if (plans[i] == Plan.StringIn)
                output.AppendLine($"        fixed (byte* {Local(parameters[i])}Ptr = {Local(parameters[i])}Utf8)");
            else if (plans[i] == Plan.SpanTyped)
                output.AppendLine($"        fixed ({parameters[i].PointeeType}* {Local(parameters[i])}Ptr = {parameters[i].ManagedName})");
            else if (plans[i] is Plan.SpanGenericSized or Plan.SpanGenericUnsized)
                output.AppendLine($"        fixed ({TypeParameter(command, i)}* {Local(parameters[i])}Ptr = {parameters[i].ManagedName})");
        }
        var call = $"gl.{command.ManagedName}({string.Join(", ", argument.Where(value => value is not null))})";
        output.AppendLine($"            {(command.ReturnType == "void" ? call : "return " + call)};");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>GenBuffers(1, &amp;value) style singular forms when a lone trailing counted pointer remains.</summary>
    private void AppendSingular(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (parameters.Count == 0 || parameters[^1] is not { PointeeType: not null, PointeeIsChar: false } pointer)
            return;
        if (parameters.Count(parameter => parameter.PointerDepth > 0) != 1)
            return;
        if (ParseLen(command, pointer) is not { Kind: LenKind.ParamRef, Divisor: 1 } len || len.ParamIndex != parameters.Count - 2)
            return;

        var name = Depluralize(command.ManagedName);
        var singular = Depluralize(pointer.ManagedName.TrimStart('@'));
        if (name == command.ManagedName || commandNames.Contains(name))
            return;

        var passthrough = parameters.Take(parameters.Count - 2).ToList();
        var signature = new List<string> { $"this {config.ApiClass} gl" };
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        if (pointer.PointeeIsConst)
            signature.Add($"{pointer.PointeeType} {singular}");
        if (!signatures.Add($"{name}({string.Join(", ", signature)})"))
            return;

        var count = CountExpression(parameters[^2], "1");
        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append(count);
        output.AppendLine($"    /// <inheritdoc cref=\"{config.ApiClass}.{command.ManagedName}\"/>");
        if (pointer.PointeeIsConst)
        {
            output.AppendLine($"    public static void {name}({string.Join(", ", signature)})");
            output.AppendLine("    {");
            output.AppendLine($"        gl.{command.ManagedName}({string.Join(", ", arguments.Append($"(nint)(&{singular})"))});");
        }
        else
        {
            output.AppendLine($"    public static {pointer.PointeeType} {name}({string.Join(", ", signature)})");
            output.AppendLine("    {");
            output.AppendLine($"        {pointer.PointeeType} {singular};");
            output.AppendLine($"        gl.{command.ManagedName}({string.Join(", ", arguments.Append($"(nint)(&{singular})"))});");
            output.AppendLine($"        return {singular};");
        }
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>An out overload for the Get* commands whose trailing pointer holds a single value.</summary>
    private void AppendOutScalar(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (!command.ManagedName.StartsWith("Get") || parameters.Count == 0)
            return;
        if (parameters[^1] is not { PointeeType: not null, PointeeIsChar: false, PointeeIsConst: false } pointer)
            return;
        var len = ParseLen(command, pointer);
        var singleValued = len is { Kind: LenKind.Literal, Divisor: 1 }
            || (len.Kind == LenKind.CompSize && len.CompSizeArgs is ["pname"]);
        if (!singleValued)
            return;

        var passthrough = parameters.Take(parameters.Count - 1).ToList();
        var signature = new List<string> { $"this {config.ApiClass} gl" };
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        signature.Add($"out {pointer.PointeeType} {pointer.ManagedName}");
        if (!signatures.Add($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append("(nint)(&value)");
        output.AppendLine($"    /// <inheritdoc cref=\"{config.ApiClass}.{command.ManagedName}\"/>");
        output.AppendLine($"    public static void {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine($"        {pointer.PointeeType} value;");
        output.AppendLine($"        gl.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine($"        {pointer.ManagedName} = value;");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>
    /// A string return for the trailing (bufSize, length, buffer) shape of the info-log family,
    /// with a stack buffer.
    /// </summary>
    private void AppendInfoLog(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (parameters.Count < 3)
            return;
        var (bufSize, length, buffer) = (parameters[^3], parameters[^2], parameters[^1]);
        if (bufSize is not { PointerDepth: 0, ManagedType: "int" })
            return;
        if (length is not { PointeeType: "int" } || ParseLen(command, length) is not { Kind: LenKind.Literal, Divisor: 1 })
            return;
        if (buffer is not { PointerDepth: 1, PointeeIsChar: true, PointeeIsConst: false })
            return;
        if (ParseLen(command, buffer) is not { Kind: LenKind.ParamRef } bufferLen || bufferLen.ParamIndex != parameters.Count - 3)
            return;

        var passthrough = parameters.Take(parameters.Count - 3).ToList();
        var signature = new List<string> { $"this {config.ApiClass} gl" };
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        if (!signatures.Add($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName)
            .Append("buffer.Length").Append("(nint)(&length)").Append("(nint)bufferPtr");
        output.AppendLine($"    /// <inheritdoc cref=\"{config.ApiClass}.{command.ManagedName}\"/>");
        output.AppendLine($"    public static string {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine("        Span<byte> buffer = stackalloc byte[4096];");
        output.AppendLine("        int length;");
        output.AppendLine("        fixed (byte* bufferPtr = buffer)");
        output.AppendLine($"            gl.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine("        return Encoding.UTF8.GetString(buffer[..length]);");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>
    /// A single-string overload for the (count, GLchar* const*, GLint*) source-array shape
    /// (glShaderSource).
    /// </summary>
    private void AppendSingleSource(StringBuilder output, GlCommand command)
    {
        var parameters = command.Parameters;
        if (parameters.Count < 3)
            return;
        var (count, strings, lengths) = (parameters[^3], parameters[^2], parameters[^1]);
        if (count is not { PointerDepth: 0, ManagedType: "int" })
            return;
        if (strings is not { PointerDepth: 2, PointeeIsChar: true, PointeeIsConst: true })
            return;
        if (lengths is not { PointerDepth: 1, PointeeType: "int", PointeeIsConst: true })
            return;
        if (ParseLen(command, strings) is not { Kind: LenKind.ParamRef } stringsLen || stringsLen.ParamIndex != parameters.Count - 3)
            return;
        if (ParseLen(command, lengths) is not { Kind: LenKind.ParamRef } lengthsLen || lengthsLen.ParamIndex != parameters.Count - 3)
            return;

        var passthrough = parameters.Take(parameters.Count - 3).ToList();
        var signature = new List<string> { $"this {config.ApiClass} gl" };
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        signature.Add("string source");
        if (!signatures.Add($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName)
            .Append("1").Append("(nint)(&sourcePointer)").Append("(nint)(&length)");
        output.AppendLine($"    /// <inheritdoc cref=\"{config.ApiClass}.{command.ManagedName}\"/>");
        output.AppendLine($"    public static void {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine("        var sourceUtf8 = Encoding.UTF8.GetBytes(source);");
        output.AppendLine("        fixed (byte* sourcePtr = sourceUtf8)");
        output.AppendLine("        {");
        output.AppendLine("            var sourcePointer = (nint)sourcePtr;");
        output.AppendLine("            var length = sourceUtf8.Length;");
        output.AppendLine($"            gl.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine("        }");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>The pinnable local name for a parameter, without the keyword escape.</summary>
    private static string Local(GlParameter parameter) => parameter.ManagedName.TrimStart('@');

    private static string SpanType(GlParameter parameter, string elementType) =>
        $"{(parameter.PointeeIsConst ? "ReadOnlySpan" : "Span")}<{elementType}>";

    private string TypeParameter(GlCommand command, int pointerIndex)
    {
        var generic = command.Parameters
            .Where(parameter => parameter is { PointerDepth: 1, PointeeType: null, PointeeIsChar: false })
            .ToList();
        if (generic.Count == 1)
            return "T";
        var name = Local(command.Parameters[pointerIndex]);
        return "T" + char.ToUpperInvariant(name[0]) + name[1..];
    }

    /// <summary>Wraps a count expression in a cast when the count parameter is not int-typed.</summary>
    private static string CountExpression(GlParameter count, string expression) =>
        count.ManagedType == "int" ? expression : $"({count.ManagedType})({expression})";

    private static string Depluralize(string name) =>
        name.EndsWith("ies") ? name[..^3] + "y"
        : name.EndsWith("s") && !name.EndsWith("ss") ? name[..^1]
        : name;
}
