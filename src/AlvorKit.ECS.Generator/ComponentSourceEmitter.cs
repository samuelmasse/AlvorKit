namespace AlvorKit.ECS.Generator;

internal static class ComponentSourceEmitter
{
    internal static string Emit(InterfaceModel model) =>
    ComponentTemplate.Render(
        "component-source.cs.tmpl",
        ("NamespaceBlock", NamespaceBlock(model)),
        ("ComponentGroup", ComponentGroup(model)),
        ("ReadExtensions", ReadExtensions(model)),
        ("MutativeExtensions", MutativeExtensions(model)));

    private static string NamespaceBlock(InterfaceModel model) =>
    string.IsNullOrEmpty(model.Namespace)
        ? ""
        : ComponentTemplate.RenderFragment("namespace.csfrag.tmpl", ("Namespace", model.Namespace));

    private static string ComponentGroup(InterfaceModel model) =>
    ComponentTemplate.RenderFragment(
        "component-group.csfrag.tmpl",
        ("Access", model.Access),
        ("ClassName", model.ClassName),
        ("InterfaceName", model.InterfaceName),
        ("Components", Join(model.Properties.Select(ComponentMarker))));

    private static string ComponentMarker(PropertyModel property) =>
    ComponentTemplate.RenderFragment(
        "component-marker.csfrag.tmpl",
        ("Comment", Comment(property, "    ")),
        ("ToStringAttribute", property.AddToString ? "    [ComponentToString]\n" : ""),
        ("Access", ComponentAccess.WiderAccess(property.GetAccess, property.SetAccess)),
        ("Name", property.Name),
        ("ValueType", property.ValueType));

    private static string ReadExtensions(InterfaceModel model) =>
    ComponentTemplate.RenderFragment(
        "read-extensions.csfrag.tmpl",
        ("Access", model.Access),
        ("ClassName", model.ClassName),
        ("HasProperties", Join(model.Properties.Select(property => HasProperty(model, property)))),
        ("GetProperties", Join(model.Properties.Select(property => GetProperty(model, property)))));

    private static string HasProperty(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
        property.Archetypal ? "has-property-archetypal.csfrag.tmpl" : "has-property.csfrag.tmpl",
        ("Comment", Comment(property, "        ")),
        ("Access", property.GetAccess),
        ("Name", property.Name),
        ("ClassName", model.ClassName),
        ("Type", property.NullableType));

    private static string GetProperty(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
        property.Archetypal ? "get-property-archetypal.csfrag.tmpl" : "get-property.csfrag.tmpl",
        ("Comment", Comment(property, "        ")),
        ("Access", property.GetAccess),
        ("Type", property.NullableType),
        ("AccessorName", ComponentNames.AccessorName(property)),
        ("ClassName", model.ClassName),
        ("Name", property.Name));

    private static string MutativeExtensions(InterfaceModel model) =>
    ComponentTemplate.RenderFragment(
        "mutative-extensions.csfrag.tmpl",
        ("Access", model.Access),
        ("ClassName", model.ClassName),
        ("Properties", Join(model.Properties.Select(property => MutatingProperty(model, property)))),
        ("UnsetMethods", Join(model.Properties.Select(property => UnsetMethod(model, property)))),
        ("BuilderBlock", model.SkipBuilder ? "" : BuilderBlock(model)));

    private static string MutatingProperty(InterfaceModel model, PropertyModel property)
    {
        var access = ComponentAccess.WiderAccess(property.GetAccess, property.SetAccess);
        string template = property.Archetypal
            ? property.LazyInitialize
                ? "mutating-property-lazy-archetypal.csfrag.tmpl"
                : "mutating-property-archetypal.csfrag.tmpl"
            : property.LazyInitialize
                ? "mutating-property-lazy.csfrag.tmpl"
                : "mutating-property.csfrag.tmpl";
        return ComponentTemplate.RenderFragment(
            template,
            ("Comment", Comment(property, "        ")),
            ("Access", access),
            ("Type", property.NullableType),
            ("AccessorName", ComponentNames.AccessorName(property)),
            ("ClassName", model.ClassName),
            ("Name", property.Name),
            ("GetPrefix", property.GetAccess == access ? "" : property.GetAccess + " "),
            ("SetPrefix", property.SetAccess == access ? "" : property.SetAccess + " "));
    }

    private static string UnsetMethod(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
        property.Archetypal ? "unset-method-archetypal.csfrag.tmpl" : "unset-method.csfrag.tmpl",
        ("Comment", Comment(property, "        ")),
        ("Access", property.SetAccess),
        ("Name", property.Name),
        ("Type", property.NullableType),
        ("ClassName", model.ClassName));

    private static string BuilderBlock(InterfaceModel model) =>
    ComponentTemplate.RenderFragment(
        "builder-block.csfrag.tmpl",
        ("Methods", Join(model.Properties.Select(BuilderMethod))));

    private static string BuilderMethod(PropertyModel property) =>
    ComponentTemplate.RenderFragment(
        "builder-method.csfrag.tmpl",
        ("Comment", Comment(property, "        ")),
        ("SetAccess", property.SetAccess),
        ("GetAccess", property.GetAccess),
        ("Name", property.Name),
        ("AccessorName", ComponentNames.AccessorName(property)),
        ("Type", property.NullableType));

    private static string Comment(PropertyModel property, string indent) =>
    property.Comment is null ? "" : string.Join("", property.Comment.Split('\n').Select(line => indent + line + "\n"));

    private static string Join(IEnumerable<string> parts) => string.Join("", parts);
}
