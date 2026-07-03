namespace AlvorKit.UI.Generator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
internal class PropGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "AlvorKit.ECS.Generator.ComponentsAttribute",
            static (node, _) => node is InterfaceDeclarationSyntax,
            static (ctx, ct) =>
            {
                var ifaceSymbol = (INamedTypeSymbol)ctx.TargetSymbol;

                var ns = ifaceSymbol.ContainingNamespace?.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                        .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) ?? "";

                var propList = new System.Collections.Generic.List<PropInfo>();
                foreach (var member in ifaceSymbol.GetMembers())
                {
                    if (member is not IPropertySymbol p) continue;
                    if (p.IsStatic || p.Parameters.Length != 0) continue;
                    if (p.GetMethod is null || p.SetMethod is null) continue;
                    if (p.Type is not INamedTypeSymbol named) continue;

                    var getAccess = ToAccessString(p.GetMethod!.DeclaredAccessibility);
                    var setAccess = ToAccessString(p.SetMethod!.DeclaredAccessibility);

                    PropKind kind;
                    string innerType;
                    if (named.Name == "UiText" && named.TypeArguments.Length == 0)
                    {
                        kind = PropKind.Text;
                        innerType = "";
                    }
                    else if (named.TypeArguments.Length == 1)
                    {
                        if (named.Name == "UiProp") kind = PropKind.Prop;
                        else if (named.Name == "UiCallback") kind = PropKind.Callback;
                        else if (named.Name == "UiValue") kind = PropKind.Value;
                        else continue;

                        var typeArg = named.TypeArguments[0];
                        innerType = typeArg.ToDisplayString(
                            SymbolDisplayFormat.FullyQualifiedFormat
                                .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)
                                .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier));
                    }
                    else continue;

                    var fullName = p.Name;
                    var baseName = fullName.EndsWith("FV") ? fullName.Substring(0, fullName.Length - 2) : fullName;

                    string? doc = null;
                    var xml = p.GetDocumentationCommentXml();
                    if (xml != null && xml.Length > 0)
                    {
                        var start = xml.IndexOf("<summary>");
                        var end = xml.IndexOf("</summary>");
                        if (start >= 0 && end > start)
                            doc = xml.Substring(start + 9, end - start - 9).Trim();
                    }

                    propList.Add(new PropInfo(fullName, baseName, innerType, kind, doc, getAccess, setAccess));
                }
                var props = propList.ToArray();
                var ifaceAccess = ToAccessString(ifaceSymbol.DeclaredAccessibility);

                return new Model(ns, ifaceSymbol.Name, props, ifaceAccess);
            });

        context.RegisterSourceOutput(provider, static (spc, model) =>
        {
            if (model.Props.Length == 0)
                return;

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(model.Namespace))
            {
                sb.AppendLine($"namespace {model.Namespace};");
                sb.AppendLine();
            }

            sb.AppendLine("using AlvorKit.ECS;");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            var className = model.InterfaceName;
            if (className.Length >= 2 && className[0] == 'I' && char.IsUpper(className[1]))
                className = className.Substring(1);

            sb.AppendLine($"/// <summary>Provides fluent UI property mutator extensions for <see cref=\"{model.InterfaceName}\" />.</summary>");
            sb.AppendLine($"{model.InterfaceAccess} static class {className}PropExtensions");
            sb.AppendLine("{");
            sb.AppendLine("    extension<T>(EntMutator<T> mut) where T : IEntMut");
            sb.AppendLine("    {");

            for (int i = 0; i < model.Props.Length; i++)
            {
                if (i != 0)
                    sb.AppendLine();

                var prop = model.Props[i];

                if (prop.Kind == PropKind.Callback)
                {
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}F(in {prop.InnerType} value)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = new(value);");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");
                }
                else if (prop.Kind == PropKind.Text)
                {
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}V(string value)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = new(value, null);");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");

                    sb.AppendLine();
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}F(global::System.Func<global::System.ReadOnlySpan<char>>? func)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = new(null!, func);");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");
                }
                else if (prop.Kind == PropKind.Value)
                {
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}V(in {prop.InnerType} value)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = value;");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");
                }
                else
                {
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}V(in {prop.InnerType} value)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = new(value, null);");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");

                    sb.AppendLine();
                    if (prop.Doc != null)
                        sb.AppendLine($"        /// <summary>{prop.Doc}</summary>");
                    sb.AppendLine($"        {prop.SetAccess} EntMutator<T> {prop.BaseName}F(global::System.Func<{prop.InnerType}>? func)");
                    sb.AppendLine("        {");
                    sb.AppendLine($"            mut.Ent.{prop.FullName} = new(default!, func);");
                    sb.AppendLine("            return mut;");
                    sb.AppendLine("        }");
                }
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            spc.AddSource($"{className}Prop.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        });
    }

    private enum PropKind { Prop, Callback, Text, Value }
    private record Model(string Namespace, string InterfaceName, PropInfo[] Props, string InterfaceAccess);
    private record PropInfo(string FullName, string BaseName, string InnerType, PropKind Kind, string? Doc, string GetAccess, string SetAccess);

    private static string ToAccessString(Accessibility a) => a switch
    {
        Accessibility.Internal => "internal",
        Accessibility.Protected => "protected",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.Private => "private",
        _ => "public"
    };
}
