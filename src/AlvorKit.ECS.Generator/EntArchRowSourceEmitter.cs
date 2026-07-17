namespace AlvorKit.ECS.Generator;

internal static class EntArchRowSourceEmitter
{
    internal static string Emit(EntArchRowModel model) =>
        ComponentTemplate.Render(
            "ent-arch-row-source.cs.tmpl",
            ("NamespaceBlock", NamespaceBlock(model.Namespace)),
            ("ExtensionType", model.ExtensionType),
            ("RowType", model.RowType),
            ("RowsType", model.RowsType),
            ("QueryType", model.QueryType),
            ("RowFields", RowFields(model.Fields)),
            ("ConstructorParameters", ConstructorParameters(model.Fields)),
            ("ConstructorAssignments", ConstructorAssignments(model.Fields)),
            ("Properties", Properties(model.Fields)),
            ("RowsFields", RowsFields(model.Fields)),
            ("NullAssignments", NullAssignments(model.Fields)),
            ("CurrentArguments", CurrentArguments(model.Fields)),
            ("BindAssignments", BindAssignments(model.Fields)));

    private static string NamespaceBlock(string value) =>
        string.IsNullOrEmpty(value)
            ? ""
            : ComponentTemplate.RenderFragment("namespace.csfrag.tmpl", ("Namespace", value));

    private static string RowFields(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) => $"    private readonly ref {field.ValueType} field{index};\n");

    private static string ConstructorParameters(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) => $"        ref {field.ValueType} field{index},\n");

    private static string ConstructorAssignments(EntArchRowFieldModel[] fields) =>
        Join(fields, static (_, index) => $"        this.field{index} = ref field{index};\n");

    private static string Properties(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) => ComponentTemplate.RenderFragment(
            "ent-arch-row-properties.csfrag.tmpl",
            ("Access", field.Access),
            ("Type", field.ValueType),
            ("Name", field.Name),
            ("Index", index.ToString())));

    private static string RowsFields(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) => $"    private ref {field.ValueType} field{index};\n");

    private static string NullAssignments(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) =>
            $"        field{index} = ref global::System.Runtime.CompilerServices.Unsafe.NullRef<{field.ValueType}>();\n");

    private static string CurrentArguments(EntArchRowFieldModel[] fields) =>
        Join(fields, static (_, index) => $"            ref field{index},\n");

    private static string BindAssignments(EntArchRowFieldModel[] fields) =>
        Join(fields, static (field, index) =>
            $"            field{index} = ref global::System.Runtime.InteropServices.MemoryMarshal.GetReference(" +
            $"chunk.Get<{field.ValueType}, {field.MarkerType}>());\n");

    private static string Join(
        EntArchRowFieldModel[] fields,
        Func<EntArchRowFieldModel, int, string> render) =>
        string.Concat(fields.Select(render));
}
