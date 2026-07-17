namespace AlvorKit.ECS.Generator;

internal sealed record EntArchRowFieldModel(
    string Name,
    string ValueType,
    string MarkerType,
    string Access);

internal sealed record EntArchRowModel(
    string Namespace,
    string ExtensionType,
    string RowType,
    string RowsType,
    string QueryType,
    EntArchRowFieldModel[] Fields);
