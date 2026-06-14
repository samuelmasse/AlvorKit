# AlvorKit

AlvorKit is a C# game development kit

## Binding development mode

Projects use published binding packages by default. To develop against generated bindings in
`out/bindgen`, add an ignored `AlvorKit.Local.props` at the repository root:

```xml
<Project>
    <PropertyGroup>
        <UseLocalBindings>true</UseLocalBindings>
    </PropertyGroup>
</Project>
```

Generate the local projects with:

```powershell
dotnet run --project scripts\AlvorKit.Script.Bindgen -- all
```

## Native package builds

Native packages are built by the shared native build runner. To see the configured native packages:

```powershell
dotnet run --project scripts\AlvorKit.Script.NativeBuild -- list
```

Build one RID with:

```powershell
dotnet run --project scripts\AlvorKit.Script.NativeBuild -- build xxhash --rid win-x64
```

Then pack the native project:

```powershell
dotnet build -c Release native\xxhash\AlvorKit.XxHash.Native.csproj
```

Native package revisions live in `native/<lib>/version/REVISION`; generated binding package
revisions live in `native/<lib>/version/BINDING_REVISION`. Native build settings live in
`native/<lib>/conf/native-build.json`.
