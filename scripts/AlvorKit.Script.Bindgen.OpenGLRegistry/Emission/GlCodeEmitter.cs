namespace AlvorKit.Script.Bindgen;

/// <summary>Writes the generated OpenGL API, proc-loading backend, and convenience surface.</summary>
/// <param name="config">Bindgen configuration that controls output names and paths.</param>
/// <param name="tag">OpenGL registry source tag or commit used in generated attribution.</param>
/// <param name="docTag">OpenGL refpage source tag or commit used in generated attribution.</param>
public sealed class GlCodeEmitter(BindgenConfig config, string tag, string docTag)
{
    /// <summary>Writes every generated OpenGL source and project file.</summary>
    public void Emit(GlBindingModel model, string repoRoot, string version)
    {
        var context = new GlCodeEmissionContext(config, tag, docTag);
        var apiDirectory = Path.Combine(repoRoot, config.ApiProject);
        var backendDirectory = Path.Combine(repoRoot, config.BackendProject);
        GeneratedOutput.RecreateDirectory(apiDirectory);
        GeneratedOutput.RecreateDirectory(backendDirectory);

        var apiProjectName = Path.GetFileName(config.ApiProject);
        var backendProjectName = Path.GetFileName(config.BackendProject);
        var projects = new GlProjectEmitter(context);
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(apiDirectory)!, "Directory.Build.props"), GeneratedOutput.EmitSharedProps());
        File.WriteAllText(Path.Combine(apiDirectory, apiProjectName + ".csproj"), projects.EmitApiProject(version));
        File.WriteAllText(Path.Combine(backendDirectory, backendProjectName + ".csproj"), projects.EmitBackendProject(version, apiProjectName));

        var enums = new GlEnumEmitter(context);
        foreach (var group in model.Groups)
            File.WriteAllText(Path.Combine(apiDirectory, group.ManagedName + ".cs"), enums.Emit(group, catchAll: false));
        File.WriteAllText(Path.Combine(apiDirectory, model.AllTokens.ManagedName + ".cs"), enums.Emit(model.AllTokens, catchAll: true));
        if (model.HandleTypes.Count > 0)
            File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + "Handles.cs"), new GlHandleEmitter(context).Emit(model));
        foreach (var callback in model.Delegates)
            File.WriteAllText(Path.Combine(apiDirectory, callback.ManagedName + ".cs"), new GlDelegateEmitter(context).Emit(callback));

        File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + ".cs"), new GlApiContractEmitter(context).Emit(model));
        File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + "Wrapper.cs"), new GlWrapperEmitter(context).Emit(model));
        File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + "Noop.cs"), new GlNoopEmitter(context).Emit(model));
        if (model.WideConstants.Count > 0)
            File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + "Constants.cs"), new GlConstantEmitter(context).Emit(model));
        if (new GlExtensionsEmitter(config).Emit(model, context.SourceHeader()) is { } extensions)
            File.WriteAllText(Path.Combine(apiDirectory, config.ApiClass + "Extensions.cs"), extensions);
        File.WriteAllText(Path.Combine(apiDirectory, "THIRD-PARTY-NOTICES.txt"), new GlThirdPartyNoticeEmitter(context).Emit(model));
        File.WriteAllText(Path.Combine(backendDirectory, config.BackendClass + ".cs"), new GlBackendEmitter(context).Emit(model));
    }
}
