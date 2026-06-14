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
            AppendStringGetter(body, command);
        }
        if (body.Length == 0)
            return null;

        var output = sourceHeader;
        output.AppendLine("using System.Text;");
        output.AppendLine();
        output.AppendLine($"namespace {config.Namespace};");
        output.AppendLine();
        output.AppendLine($"public unsafe partial class {config.ApiClass}");
        output.AppendLine("{");
        output.Append(body);
        output.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        output.AppendLine("    private static nint ByteLength<T>(ReadOnlySpan<T> span) where T : unmanaged =>");
        output.AppendLine("        checked((nint)((nuint)span.Length * (nuint)sizeof(T)));");
        output.AppendLine();
        output.AppendLine("    /// <summary>A NUL-terminated UTF-8 copy of a string for interop: stack-backed when short, otherwise native memory freed on Dispose - never on the GC heap.</summary>");
        output.AppendLine("    private readonly ref struct Utf8");
        output.AppendLine("    {");
        output.AppendLine("        private readonly void* native;");
        output.AppendLine("        /// <summary>Pointer to the NUL-terminated UTF-8 bytes.</summary>");
        output.AppendLine("        public readonly nint Pointer;");
        output.AppendLine("        /// <summary>Byte length excluding the NUL terminator.</summary>");
        output.AppendLine("        public readonly int Length;");
        output.AppendLine();
        output.AppendLine("        public Utf8(string text, Span<byte> stack)");
        output.AppendLine("        {");
        output.AppendLine("            Length = Encoding.UTF8.GetByteCount(text);");
        output.AppendLine("            native = Length < stack.Length ? null : NativeMemory.Alloc((nuint)(Length + 1));");
        output.AppendLine("            var buffer = native != null ? new Span<byte>(native, Length + 1) : stack;");
        output.AppendLine("            Encoding.UTF8.GetBytes(text, buffer);");
        output.AppendLine("            buffer[Length] = 0;");
        output.AppendLine("            Pointer = (nint)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));");
        output.AppendLine("        }");
        output.AppendLine();
        output.AppendLine("        public void Dispose() => NativeMemory.Free(native);");
        output.AppendLine("    }");
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
        var configured = config.SpanParams.GetValueOrDefault(command.NativeName, []);

        // Strings first: a paired length parameter named by COMPSIZE is replaced by the UTF-8 byte count.
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            if (parameter is not { PointerDepth: 1, PointeeIsChar: true, PointeeIsConst: true })
                continue;
            plans[i] = Plan.StringIn;
            argument[i] = $"{Local(parameter)}Utf8.Pointer";
            foreach (var lengthArg in ParseLen(command, parameter) is { Kind: LenKind.CompSize } len ? len.CompSizeArgs : [])
            {
                var paired = parameters.FindIndex(candidate => candidate.NativeName == lengthArg && candidate is { PointerDepth: 0, ManagedType: "int" or "uint" });
                if (paired < 0)
                    continue;
                plans[paired] = Plan.Dropped;
                argument[paired] = CountExpression(parameters[paired], $"{Local(parameter)}Utf8.Length");
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
        }

        if (!plans.Any(plan => plan != Plan.Keep && plan != Plan.Dropped))
            return;

        // Signature, generic type parameters and their constraints.
        var typeParameters = new List<string>();
        var signature = new List<string>();
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

        var pinnedSpans = new List<string>();
        var pinnedStrings = new List<string>();
        var dropped = new List<string>();
        for (var i = 0; i < parameters.Count; i++)
        {
            if (plans[i] is Plan.SpanTyped or Plan.SpanGenericSized or Plan.SpanGenericUnsized)
                pinnedSpans.Add(parameters[i].ManagedName);
            else if (plans[i] == Plan.StringIn)
                pinnedStrings.Add(parameters[i].ManagedName);
            else if (plans[i] == Plan.Dropped)
                dropped.Add(parameters[i].ManagedName);
        }
        var detailParts = new List<string>();
        if (pinnedSpans.Count > 0)
            detailParts.Add($"Pins {ParamRefs(pinnedSpans)} for the duration of the call.");
        if (pinnedStrings.Count > 0)
            detailParts.Add($"Marshals {ParamRefs(pinnedStrings)} to NUL-terminated UTF-8 on the stack, or native memory for long strings - never the GC heap.");
        if (dropped.Count > 0)
            detailParts.Add($"Supplies {CodeNames(dropped)} automatically from the span length{(dropped.Count > 1 ? "s" : "")}.");
        EmitOverloadDocs(output, command, string.Join(" ", detailParts));
        output.AppendLine($"    public virtual {command.ReturnType} {command.ManagedName}{generics}({string.Join(", ", signature)}){constraints}");
        output.AppendLine("    {");
        for (var i = 0; i < parameters.Count; i++)
            if (plans[i] == Plan.StringIn)
                output.AppendLine($"        using var {Local(parameters[i])}Utf8 = new Utf8({parameters[i].ManagedName}, stackalloc byte[256]);");
        var fixedCount = 0;
        for (var i = 0; i < parameters.Count; i++)
        {
            if (plans[i] == Plan.SpanTyped)
                output.AppendLine($"        fixed ({parameters[i].PointeeType}* {Local(parameters[i])}Ptr = {parameters[i].ManagedName})");
            else if (plans[i] is Plan.SpanGenericSized or Plan.SpanGenericUnsized)
                output.AppendLine($"        fixed ({TypeParameter(command, i)}* {Local(parameters[i])}Ptr = {parameters[i].ManagedName})");
            else
                continue;
            fixedCount++;
        }
        var call = $"this.{command.ManagedName}({string.Join(", ", argument.Where(value => value is not null))})";
        output.AppendLine($"        {(fixedCount > 0 ? "    " : "")}{(command.ReturnType == "void" ? call : "return " + call)};");
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
        var signature = new List<string>();
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        if (pointer.PointeeIsConst)
            signature.Add($"{pointer.PointeeType} {singular}");
        if (!signatures.Add($"{name}({string.Join(", ", signature)})"))
            return;

        var count = CountExpression(parameters[^2], "1");
        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append(count);
        EmitOverloadDocs(output, command, pointer.PointeeIsConst
            ? $"Passes the single <paramref name=\"{singular}\"/> with a count of 1, taking its address for the call."
            : $"Returns the single value written, calling with a count of 1 and a stack address for the out pointer.");
        if (pointer.PointeeIsConst)
        {
            output.AppendLine($"    public virtual void {name}({string.Join(", ", signature)})");
            output.AppendLine("    {");
            output.AppendLine($"        this.{command.ManagedName}({string.Join(", ", arguments.Append($"(nint)(&{singular})"))});");
        }
        else
        {
            output.AppendLine($"    public virtual {pointer.PointeeType} {name}({string.Join(", ", signature)})");
            output.AppendLine("    {");
            output.AppendLine($"        {pointer.PointeeType} {singular};");
            output.AppendLine($"        this.{command.ManagedName}({string.Join(", ", arguments.Append($"(nint)(&{singular})"))});");
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
        var signature = new List<string>();
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        signature.Add($"out {pointer.PointeeType} {pointer.ManagedName}");
        if (!signatures.Add($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName).Append("(nint)(&value)");
        EmitOverloadDocs(output, command, $"Reads a single value, returned through the <paramref name=\"{Local(pointer)}\"/> out parameter via a stack address.");
        output.AppendLine($"    public virtual void {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine($"        {pointer.PointeeType} value;");
        output.AppendLine($"        this.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine($"        {pointer.ManagedName} = value;");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>
    /// The trailing (bufSize, length, buffer) shape of the info-log and name family: a string return
    /// and a <c>Span&lt;char&gt;</c> overload. The string form probe-and-grows a UTF-8 work buffer
    /// (stack then native) so any length fits and only the returned string allocates; the span form
    /// decodes into the caller's buffer (truncating to fit) with no GC allocation.
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

        var leading = parameters.Take(parameters.Count - 3).Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").ToList();
        var coreArgs = string.Join(", ", parameters.Take(parameters.Count - 3).Select(parameter => parameter.ManagedName)
            .Append("buffer.Length").Append("(nint)(&written)").Append("(nint)bufferPtr"));

        if (signatures.Add($"{command.ManagedName}({string.Join(", ", leading)})"))
        {
            EmitOverloadDocs(output, command, "Returns the full text, growing the work buffer as needed (stack first, then native memory); the only allocation is the returned string.");
            output.AppendLine($"    public virtual string {command.ManagedName}({string.Join(", ", leading)})");
            EmitInfoLogProbe(output, command, coreArgs, "                    return Encoding.UTF8.GetString(buffer[..written]);");
        }

        var spanSignature = string.Join(", ", leading.Append("Span<char> destination"));
        if (signatures.Add($"{command.ManagedName}({spanSignature})"))
        {
            EmitOverloadDocs(output, command, "Decodes the UTF-8 text into <paramref name=\"destination\"/> (truncated if it does not fit) and returns the characters written. The UTF-8 staging buffer is on the stack, or native memory for a large destination; no GC allocation.");
            output.AppendLine($"    public virtual ReadOnlySpan<char> {command.ManagedName}({spanSignature})");
            output.AppendLine("    {");
            output.AppendLine("        void* native = destination.Length <= 1024 ? null : NativeMemory.Alloc((nuint)destination.Length);");
            output.AppendLine("        try");
            output.AppendLine("        {");
            output.AppendLine("            Span<byte> buffer = native != null ? new Span<byte>(native, destination.Length) : stackalloc byte[destination.Length];");
            output.AppendLine("            int written;");
            output.AppendLine("            fixed (byte* bufferPtr = buffer)");
            output.AppendLine($"                this.{command.ManagedName}({coreArgs});");
            output.AppendLine("            return destination[..Encoding.UTF8.GetChars(buffer[..written], destination)];");
            output.AppendLine("        }");
            output.AppendLine("        finally");
            output.AppendLine("        {");
            output.AppendLine("            NativeMemory.Free(native);");
            output.AppendLine("        }");
            output.AppendLine("    }");
            output.AppendLine();
        }
    }

    /// <summary>
    /// The probe-and-grow body for the string overload: a stack buffer that doubles into native
    /// memory until the GL call writes fewer characters than it offered (so the text is complete),
    /// running <paramref name="onComplete"/> at that point. Native memory is freed even if a call throws.
    /// </summary>
    private void EmitInfoLogProbe(StringBuilder output, GlCommand command, string coreArgs, params string[] onComplete)
    {
        output.AppendLine("    {");
        output.AppendLine("        Span<byte> buffer = stackalloc byte[1024];");
        output.AppendLine("        void* native = null;");
        output.AppendLine("        try");
        output.AppendLine("        {");
        output.AppendLine("            while (true)");
        output.AppendLine("            {");
        output.AppendLine("                int written;");
        output.AppendLine("                fixed (byte* bufferPtr = buffer)");
        output.AppendLine($"                    this.{command.ManagedName}({coreArgs});");
        output.AppendLine("                if (written < buffer.Length - 1)");
        output.AppendLine("                {");
        foreach (var line in onComplete)
            output.AppendLine(line);
        output.AppendLine("                }");
        output.AppendLine("                var size = buffer.Length * 2;");
        output.AppendLine("                NativeMemory.Free(native);");
        output.AppendLine("                native = NativeMemory.Alloc((nuint)size);");
        output.AppendLine("                buffer = new Span<byte>(native, size);");
        output.AppendLine("            }");
        output.AppendLine("        }");
        output.AppendLine("        finally");
        output.AppendLine("        {");
        output.AppendLine("            NativeMemory.Free(native);");
        output.AppendLine("        }");
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
        var signature = new List<string>();
        signature.AddRange(passthrough.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        signature.Add("string source");
        if (!signatures.Add($"{command.ManagedName}({string.Join(", ", signature)})"))
            return;

        var arguments = passthrough.Select(parameter => parameter.ManagedName)
            .Append("1").Append("(nint)(&sourcePointer)").Append("(nint)(&length)");
        EmitOverloadDocs(output, command, "Marshals <paramref name=\"source\"/> to UTF-8 on the stack, or native memory for long strings (never the GC heap), and passes it with its byte length.");
        output.AppendLine($"    public virtual void {command.ManagedName}({string.Join(", ", signature)})");
        output.AppendLine("    {");
        output.AppendLine("        using var sourceUtf8 = new Utf8(source, stackalloc byte[256]);");
        output.AppendLine("        var sourcePointer = sourceUtf8.Pointer;");
        output.AppendLine("        var length = sourceUtf8.Length;");
        output.AppendLine($"        this.{command.ManagedName}({string.Join(", ", arguments)});");
        output.AppendLine("    }");
        output.AppendLine();
    }

    /// <summary>
    /// For a command that returns a C string pointer (glGetString/glGetStringi), overloads that keep
    /// the raw pointer method intact: an <c>out string</c> form that decodes it, and a span form that
    /// decodes into the caller's buffer and outs the written slice. The extra parameters make these
    /// distinct from the pointer-returning core method.
    /// </summary>
    private void AppendStringGetter(StringBuilder output, GlCommand command)
    {
        if (!command.ReturnsCString)
            return;
        var leading = command.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}").ToList();
        var callArgs = string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedName));

        var outStringSignature = string.Join(", ", leading.Append("out string? value"));
        if (signatures.Add($"{command.ManagedName}({outStringSignature})"))
        {
            EmitOverloadDocs(output, command, "Decodes the returned C string into <paramref name=\"value\"/>, or null when GL returns no string.");
            output.AppendLine($"    public virtual void {command.ManagedName}({outStringSignature})");
            output.AppendLine("    {");
            output.AppendLine($"        value = Marshal.PtrToStringUTF8(this.{command.ManagedName}({callArgs}));");
            output.AppendLine("    }");
            output.AppendLine();
        }

        var spanSignature = string.Join(", ", leading.Append("Span<char> destination").Append("out ReadOnlySpan<char> result"));
        if (signatures.Add($"{command.ManagedName}({spanSignature})"))
        {
            EmitOverloadDocs(output, command, "Decodes the returned C string into <paramref name=\"destination\"/> (truncated to fit) and sets <paramref name=\"result\"/> to the slice written. No allocation.");
            output.AppendLine($"    public virtual void {command.ManagedName}({spanSignature})");
            output.AppendLine("    {");
            output.AppendLine($"        var bytes = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((byte*)this.{command.ManagedName}({callArgs}));");
            output.AppendLine("        System.Text.Unicode.Utf8.ToUtf16(bytes, destination, out _, out var written);");
            output.AppendLine("        result = destination[..written];");
            output.AppendLine("    }");
            output.AppendLine();
        }
    }

    /// <summary>The pinnable local name for a parameter, without the keyword escape.</summary>
    private static string Local(GlParameter parameter) => parameter.ManagedName.TrimStart('@');

    /// <summary>
    /// A cref to the core command, qualified by its parameter types. The overloads now live on the
    /// same class as the command they wrap, so a bare name would be an ambiguous reference.
    /// </summary>
    private string CoreCref(GlCommand command) =>
        $"{config.ApiClass}.{command.ManagedName}({string.Join(", ", command.Parameters.Select(parameter => parameter.ManagedType))})";

    /// <summary>
    /// Emits the inherited summary and a convenience-overload remark whose <paramref name="detail"/>
    /// spells out the marshalling this overload performs (which arguments it pins, marshals or fills in).
    /// </summary>
    private void EmitOverloadDocs(StringBuilder output, GlCommand command, string detail)
    {
        output.AppendLine($"    /// <inheritdoc cref=\"{CoreCref(command)}\"/>");
        output.AppendLine($"    /// <remarks>Convenience overload. Calls <see cref=\"{CoreCref(command)}\"/>. {detail}</remarks>");
    }

    /// <summary>Renders parameter names as <c>paramref</c> references (these name parameters of the overload).</summary>
    private static string ParamRefs(IEnumerable<string> names) =>
        string.Join(", ", names.Select(name => $"<paramref name=\"{name.TrimStart('@')}\"/>"));

    /// <summary>Renders dropped argument names in code font (they are not parameters of the overload).</summary>
    private static string CodeNames(IEnumerable<string> names) =>
        string.Join(", ", names.Select(name => $"<c>{name.TrimStart('@')}</c>"));

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
