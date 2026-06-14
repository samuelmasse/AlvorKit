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

Native package revisions live in `native/<lib>/REVISION`; generated binding package revisions live
in `native/<lib>/BINDING_REVISION`.
