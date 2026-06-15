namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated API and backend project files.</summary>
internal sealed class BindingProjectEmitter(BindingEmitterContext context)
{
    /// <summary>Emits the generated API project file.</summary>
    public string ApiProject(BindingModel model, string version)
    {
        var unsafeBlocks = context.Config.SpanOverloads
            || context.Config.SpanReturns.Count > 0
            || model.Functions.Any(function => function.ReturnsCString || function.Parameters.Any(parameter => parameter.HasStringConvenience));
        return $"""
            {context.XmlBanner()}
            <Project Sdk="Microsoft.NET.Sdk">

                <PropertyGroup>
                    <Version>{version}</Version>{(unsafeBlocks ? "\n        <AllowUnsafeBlocks>True</AllowUnsafeBlocks>" : "")}
                </PropertyGroup>

            </Project>

            """;
    }

    /// <summary>Emits the generated backend project file.</summary>
    public string BackendProject(string bindingVersion, string nativeVersion, string apiProjectName) => $"""
        {context.XmlBanner()}
        <Project Sdk="Microsoft.NET.Sdk">

            <PropertyGroup>
                <Version>{bindingVersion}</Version>
                <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
            </PropertyGroup>

            <ItemGroup>
                <PackageReference Include="{context.Config.Namespace}.Native" Version="{nativeVersion}" />
            </ItemGroup>

            <ItemGroup>
                <ProjectReference Include="..\{apiProjectName}\{apiProjectName}.csproj" />
            </ItemGroup>

        </Project>

        """;
}
