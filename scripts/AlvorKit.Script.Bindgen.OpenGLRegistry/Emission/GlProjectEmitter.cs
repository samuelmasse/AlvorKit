namespace AlvorKit.Script.Bindgen;

/// <summary>Emits generated OpenGL API and backend project files.</summary>
/// <param name="context">Shared source-emission context.</param>
internal sealed class GlProjectEmitter(GlCodeEmissionContext context)
{
    /// <summary>Emits the generated public API project file.</summary>
    public string EmitApiProject(string version) => $"""
        {context.XmlBanner()}
        <Project Sdk="Microsoft.NET.Sdk">

            <PropertyGroup>
                <Version>{version}</Version>
                <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
            </PropertyGroup>

            <ItemGroup>
                <None Include="THIRD-PARTY-NOTICES.txt" Pack="true" PackagePath="\" />
            </ItemGroup>

        </Project>

        """;

    /// <summary>Emits the backend project file that references the generated API project.</summary>
    public string EmitBackendProject(string version, string apiProjectName) => $"""
        {context.XmlBanner()}
        <Project Sdk="Microsoft.NET.Sdk">

            <PropertyGroup>
                <Version>{version}</Version>
                <AllowUnsafeBlocks>True</AllowUnsafeBlocks>
            </PropertyGroup>

            <ItemGroup>
                <ProjectReference Include="..\{apiProjectName}\{apiProjectName}.csproj" />
            </ItemGroup>

        </Project>

        """;
}
