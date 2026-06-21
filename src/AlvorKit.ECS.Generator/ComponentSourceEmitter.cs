namespace AlvorKit.ECS.Generator;

/// <summary>Renders generated C# source for component interface models.</summary>
internal static class ComponentSourceEmitter
{
    /// <summary>Renders a complete generated source file.</summary>
    internal static string Emit(InterfaceModel model) =>
        ComponentTemplate.Render(
            "component-source.cs.tmpl",
            ("NamespaceBlock", NamespaceBlock(model)),
            ("ComponentGroup", ComponentGroup(model)),
            ("ReadExtensions", ReadExtensions(model)),
            ("MutativeExtensions", MutativeExtensions(model)));

    /// <summary>Renders the namespace declaration when the source interface belongs to a namespace.</summary>
    private static string NamespaceBlock(InterfaceModel model) =>
        string.IsNullOrEmpty(model.Namespace)
            ? ""
            : ComponentTemplate.RenderFragment("namespace.csfrag.tmpl", ("Namespace", model.Namespace));

    /// <summary>Renders the component marker group and nested component marker classes.</summary>
    private static string ComponentGroup(InterfaceModel model) =>
        ComponentTemplate.RenderFragment(
            "component-group.csfrag.tmpl",
            ("Access", model.Access),
            ("ClassName", model.ClassName),
            ("InterfaceName", model.InterfaceName),
            ("Components", Join(model.Properties.Select(ComponentMarker))));

    /// <summary>Renders one nested component marker class.</summary>
    private static string ComponentMarker(PropertyModel property) =>
        ComponentTemplate.RenderFragment(
            "component-marker.csfrag.tmpl",
            ("Comment", Comment(property, "    ")),
            ("ToStringAttribute", property.AddToString ? "    [ComponentToString]\n" : ""),
            ("Access", ComponentAccess.WiderAccess(property.GetAccess, property.SetAccess)),
            ("Name", property.Name),
            ("ValueType", property.ValueType));

    /// <summary>Renders read-only extension accessors for the generated component group.</summary>
    private static string ReadExtensions(InterfaceModel model) =>
        ComponentTemplate.RenderFragment(
            "read-extensions.csfrag.tmpl",
            ("Access", model.Access),
            ("ClassName", model.ClassName),
            ("HasProperties", Join(model.Properties.Select(property => HasProperty(model, property)))),
            ("GetProperties", Join(model.Properties.Select(property => GetProperty(model, property)))));

    /// <summary>Renders one generated component presence accessor.</summary>
    private static string HasProperty(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
            "has-property.csfrag.tmpl",
            ("Comment", Comment(property, "        ")),
            ("Access", property.GetAccess),
            ("Name", property.Name),
            ("ClassName", model.ClassName),
            ("Type", property.NullableType));

    /// <summary>Renders one generated read-only component value accessor.</summary>
    private static string GetProperty(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
            "get-property.csfrag.tmpl",
            ("Comment", Comment(property, "        ")),
            ("Access", property.GetAccess),
            ("Type", property.NullableType),
            ("AccessorName", ComponentNames.AccessorName(property)),
            ("ClassName", model.ClassName),
            ("Name", property.Name));

    /// <summary>Renders mutating extension accessors and optional builder-style extensions.</summary>
    private static string MutativeExtensions(InterfaceModel model) =>
        ComponentTemplate.RenderFragment(
            "mutative-extensions.csfrag.tmpl",
            ("Access", model.Access),
            ("ClassName", model.ClassName),
            ("Properties", Join(model.Properties.Select(property => MutatingProperty(model, property)))),
            ("UnsetMethods", Join(model.Properties.Select(property => UnsetMethod(model, property)))),
            ("BuilderBlock", model.SkipBuilder ? "" : BuilderBlock(model)));

    /// <summary>Renders one generated mutating component property.</summary>
    private static string MutatingProperty(InterfaceModel model, PropertyModel property)
    {
        var access = ComponentAccess.WiderAccess(property.GetAccess, property.SetAccess);
        return ComponentTemplate.RenderFragment(
            property.LazyInitialize ? "mutating-property-lazy.csfrag.tmpl" : "mutating-property.csfrag.tmpl",
            ("Comment", Comment(property, "        ")),
            ("Access", access),
            ("Type", property.NullableType),
            ("AccessorName", ComponentNames.AccessorName(property)),
            ("ClassName", model.ClassName),
            ("Name", property.Name),
            ("GetPrefix", property.GetAccess == access ? "" : property.GetAccess + " "),
            ("SetPrefix", property.SetAccess == access ? "" : property.SetAccess + " "));
    }

    /// <summary>Renders one generated component unset method.</summary>
    private static string UnsetMethod(InterfaceModel model, PropertyModel property) =>
        ComponentTemplate.RenderFragment(
            "unset-method.csfrag.tmpl",
            ("Comment", Comment(property, "        ")),
            ("Access", property.SetAccess),
            ("Name", property.Name),
            ("Type", property.NullableType),
            ("ClassName", model.ClassName));

    /// <summary>Renders builder-style mutator extensions for a component group.</summary>
    private static string BuilderBlock(InterfaceModel model) =>
        ComponentTemplate.RenderFragment(
            "builder-block.csfrag.tmpl",
            ("Methods", Join(model.Properties.Select(BuilderMethod))));

    /// <summary>Renders one generated builder-style mutator pair.</summary>
    private static string BuilderMethod(PropertyModel property) =>
        ComponentTemplate.RenderFragment(
            "builder-method.csfrag.tmpl",
            ("Comment", Comment(property, "        ")),
            ("SetAccess", property.SetAccess),
            ("GetAccess", property.GetAccess),
            ("Name", property.Name),
            ("AccessorName", ComponentNames.AccessorName(property)),
            ("Type", property.NullableType));

    /// <summary>Indents copied XML documentation for a generated member.</summary>
    private static string Comment(PropertyModel property, string indent) =>
        property.Comment is null ? "" : string.Join("", property.Comment.Split('\n').Select(line => indent + line + "\n"));

    /// <summary>Joins generated fragments without adding extra separators.</summary>
    private static string Join(IEnumerable<string> parts) => string.Join("", parts);
}
