namespace AlvorKit.Script.Bindgen;

/// <summary>Emits helpers that copy returned C string arrays into managed string arrays.</summary>
internal sealed class BindingStringArrayReturnEmitter(BindingEmitterContext context)
{
    /// <summary>Emits configured string-array-return overloads for matching native functions.</summary>
    public void StringArrayReturns(StringBuilder output, BindingModel model)
    {
        foreach (var function in model.Functions)
        {
            if (!context.Config.StringArrayReturns.Contains(function.NativeName, StringComparer.Ordinal))
                continue;

            var countParameter = function.Parameters.LastOrDefault(parameter => parameter.Modifier == "out");
            if (function.ReturnType != "nint" || countParameter is null)
                continue;

            var leading = function.Parameters.Take(function.Parameters.Count - 1).ToList();
            var signature = string.Join(", ", leading.Select(BindingSignature.Parameter));
            var callArguments = string.Join(", ", leading.Select(BindingSignature.Argument).Append("out var count"));
            var documentation = new StringBuilder();
            BindingDocs.InheritedConvenience(
                documentation,
                $"{context.Config.ApiClass}.{function.ManagedName}({BindingSignature.Cref(function.Parameters)})",
                function.NativeName,
                "Copies the returned C string array to managed strings while supplying the count internally.");
            output.Append(TemplateResource.Render(
                typeof(BindingStringArrayReturnEmitter),
                "res/templates/bindgen/c-headers/csharp/string-array-return.csfrag.tmpl",
                ("Documentation", documentation.ToString()),
                ("ManagedName", function.ManagedName),
                ("Signature", signature),
                ("CallArguments", callArguments)));
        }
    }
}
