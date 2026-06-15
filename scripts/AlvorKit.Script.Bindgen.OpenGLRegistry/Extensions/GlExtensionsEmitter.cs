namespace AlvorKit.Script.Bindgen;

/// <summary>Emits ergonomic OpenGL convenience overloads from registry metadata.</summary>
/// <param name="config">Bindgen configuration that controls generated API names and span hints.</param>
public sealed class GlExtensionsEmitter(BindgenConfig config)
{
    /// <summary>Emits the generated extension partial class, or null when no overloads are available.</summary>
    public string? Emit(GlBindingModel model, StringBuilder sourceHeader)
    {
        var state = new GlExtensionEmissionState(config, model.Commands.Select(command => command.ManagedName).ToHashSet(StringComparer.Ordinal));
        var body = new StringBuilder();
        var emitters = new IGlOverloadEmitter[]
        {
            new GlCombinedOverloadEmitter(state),
            new GlSingularOverloadEmitter(state),
            new GlOutScalarOverloadEmitter(state),
            new GlInfoLogOverloadEmitter(state),
            new GlSingleSourceOverloadEmitter(state),
            new GlStringGetterOverloadEmitter(state)
        };

        foreach (var command in model.Commands)
            foreach (var emitter in emitters)
                emitter.Append(body, command);
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
        GlExtensionHelperEmitter.Append(output);
        output.AppendLine("}");
        return output.ToString();
    }
}
