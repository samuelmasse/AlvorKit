namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated managed delegates for OpenGL callback typedefs.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlDelegateEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits a blittable callback delegate used by a rooted typed-setter overload.</summary>
    public string Emit(GlDelegate callback)
    {
        var output = context.SourceHeader();
        output.AppendLine($"namespace {context.Config.Namespace};");
        output.AppendLine();
        output.AppendLine(
            $"/// <summary>An OpenGL callback (<c>{callback.NativeName}</c>); install it through " +
            "the matching setter, which roots it on the instance.</summary>");
        output.AppendLine("[UnmanagedFunctionPointer(CallingConvention.Winapi)]");
        var signature = string.Join(", ", callback.Parameters.Select(parameter => $"{parameter.ManagedType} {parameter.ManagedName}"));
        output.AppendLine($"public delegate {callback.ReturnType} {callback.ManagedName}({signature});");
        return output.ToString();
    }
}
