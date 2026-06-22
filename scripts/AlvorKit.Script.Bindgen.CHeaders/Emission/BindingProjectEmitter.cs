namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated API and backend project files.</summary>
internal sealed class BindingProjectEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the generated API project file.</summary>
    public string ApiProject(BindingModel model, string version)
    {
        var unsafeBlocks = context.Config.SpanOverloads
            || context.Config.SpanReturns.Count > 0
            || context.Config.StringArrayReturns.Length > 0
            || context.Config.CountedSpanParams.Count > 0
            || context.Config.XxHashConvenience
            || context.Config.FastNoise2Convenience
            || model.Functions.Any(function => function.ReturnsCString || function.Parameters.Any(parameter => parameter.HasStringConvenience));
        return TemplateResource.Render(
            typeof(BindingProjectEmitter),
            "res/templates/bindgen/c-headers/api-project.csproj.tmpl",
            ("XmlBanner", context.XmlBanner()),
            ("Version", version),
            ("UnsafeBlocks", unsafeBlocks ? Environment.NewLine + "        <AllowUnsafeBlocks>True</AllowUnsafeBlocks>" : ""));
    }

    /// <summary>Emits the generated backend project file.</summary>
    public string BackendProject(string bindingVersion, string nativeVersion, string apiProjectName) =>
        TemplateResource.Render(
            typeof(BindingProjectEmitter),
            "res/templates/bindgen/c-headers/backend-project.csproj.tmpl",
            ("XmlBanner", context.XmlBanner()),
            ("BindingVersion", bindingVersion),
            ("NativePackageId", context.Config.Namespace + ".Native"),
            ("NativeVersion", nativeVersion),
            ("ApiProjectName", apiProjectName));
}
