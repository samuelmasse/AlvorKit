namespace AlvorKit.Script.Bindgen;

/// <summary>Emits typed convenience overloads for enums, booleans, and strings.</summary>
internal sealed class BindingTypedOverloadEmitter(BindingEmitterContext context)
{
    /// <summary>Emits all typed overloads for one generated function.</summary>
    public void TypedOverloads(StringBuilder output, BindingFunction function)
    {
        var perFunction = context.Config.EnumOverloads?.Functions.GetValueOrDefault(function.NativeName);
        foreach (var typed in BindingOverloadTypes.Variants(function, context.Config.EnumOverloads))
        {
            var signature = string.Join(", ", typed.Select(BindingOverloadTypes.SignatureParameter));
            EmitOverload(output, function, typed, perFunction?.Return, signature);
        }
    }

    /// <summary>Emits one overload body for a specific type combination.</summary>
    private void EmitOverload(
        StringBuilder output,
        BindingFunction function,
        List<(string Type, BindingParameter Parameter)> typed,
        string? returnEnum,
        string signature)
    {
        var strings = typed.Where(pair => pair.Type == "string").Select(pair => pair.Parameter.ManagedName).ToList();
        var arguments = string.Join(", ", typed.Select(BindingOverloadTypes.Argument));
        var returnType = returnEnum ?? function.ReturnType;
        var call = $"{function.ManagedName}({arguments})";
        var invoke = returnEnum is not null && returnEnum != function.ReturnType ? $"({returnEnum}){call}" : call;
        var remarks = strings.Count > 0
            ? "Marshals string arguments to UTF-8 on the stack when possible."
            : "Casts typed arguments and forwards to the underlying method.";

        BindingDocs.InheritedConvenience(output, $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})", remarks);
        if (strings.Count == 0)
        {
            output.Append(TemplateResource.Render(
                typeof(BindingTypedOverloadEmitter),
                "res/templates/bindgen/c-headers/csharp/typed-expression-overload.csfrag.tmpl",
                ("ReturnType", returnType),
                ("ManagedName", function.ManagedName),
                ("Signature", signature),
                ("Invoke", invoke)));
            return;
        }
        output.Append(TemplateResource.Render(
            typeof(BindingTypedOverloadEmitter),
            "res/templates/bindgen/c-headers/csharp/typed-string-overload.csfrag.tmpl",
            ("ReturnType", returnType),
            ("ManagedName", function.ManagedName),
            ("Signature", signature),
            ("StringLocals", BindingOverloadTypes.StringLocals(typed)),
            ("Invocation", returnType == "void" ? invoke : "return " + invoke)));
    }
}
