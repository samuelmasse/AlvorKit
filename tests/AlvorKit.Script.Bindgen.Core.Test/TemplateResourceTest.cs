namespace AlvorKit.Script.Bindgen.Core.Test;

/// <summary>Tests repository-root template file loading for bindgen emitters.</summary>
[TestClass]
public sealed class TemplateResourceTest
{
    /// <summary>Rendering replaces placeholders from a template under the root res directory.</summary>
    [TestMethod]
    public void Render_ReplacesPlaceholdersFromRootResTemplate()
    {
        var output = TemplateResource.Render(
            typeof(TemplateResourceTest),
            "res/templates/bindgen/c-headers/api-project.csproj.tmpl",
            ("XmlBanner", "<!-- generated -->"),
            ("Version", "1.2.3"),
            ("UnsafeBlocks", "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>"));

        StringAssert.Contains(output, "<!-- generated -->");
        StringAssert.Contains(output, "<Version>1.2.3</Version>");
        StringAssert.Contains(output, "<AllowUnsafeBlocks>true</AllowUnsafeBlocks>");
    }

    /// <summary>Rendering a declaration fragment normalizes template padding to one trailing blank line.</summary>
    [TestMethod]
    public void RenderFragment_NormalizesTrailingNewlines()
    {
        var output = TemplateResource.RenderFragment(
            typeof(TemplateResourceTest),
            "res/templates/maths/field.csfrag.tmpl",
            ("Summary", "A field."),
            ("Offset", "0"),
            ("ScalarType", "int"),
            ("Name", "X"),
            ("Initializer", ""));

        Assert.IsTrue(output.EndsWith(Environment.NewLine + Environment.NewLine, StringComparison.Ordinal));
        Assert.IsFalse(output.EndsWith(Environment.NewLine + Environment.NewLine + Environment.NewLine, StringComparison.Ordinal));
    }

    /// <summary>Area-scoped rendering uses short template names under the bindgen template directory.</summary>
    [TestMethod]
    public void ForArea_RendersTemplateRelativeToBindgenArea()
    {
        var templates = TemplateResource.ForArea(typeof(TemplateResourceTest), "c-headers");

        var output = templates.Render(
            "api-project.csproj.tmpl",
            ("XmlBanner", "<!-- generated -->"),
            ("Version", "1.2.3"),
            ("UnsafeBlocks", ""));

        StringAssert.Contains(output, "<!-- generated -->");
        StringAssert.Contains(output, "<Version>1.2.3</Version>");
    }

    /// <summary>Missing placeholder values fail loudly instead of leaking template syntax into generated output.</summary>
    [TestMethod]
    public void Render_MissingPlaceholderValueThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateResource.Render(
            typeof(TemplateResourceTest),
            "res/templates/bindgen/c-headers/api-project.csproj.tmpl",
            ("XmlBanner", ""),
            ("Version", "1.2.3")));

        StringAssert.Contains(exception.Message, "{{UnsafeBlocks}}");
    }

    /// <summary>Template paths must stay under the repository root res directory.</summary>
    [TestMethod]
    public void Read_RejectsPathOutsideRootResDirectory()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateResource.Read(
            typeof(TemplateResourceTest),
            "scripts/AlvorKit.Script.Bindgen.Core/TemplateResource.cs"));

        StringAssert.Contains(exception.Message, "outside the repository root res directory");
    }

    /// <summary>Missing root res files report the unresolved path and concrete filename.</summary>
    [TestMethod]
    public void Read_MissingRootResFileThrows()
    {
        var exception = Assert.ThrowsException<FileNotFoundException>(() => TemplateResource.Read(
            typeof(TemplateResourceTest),
            "res/templates/bindgen/missing-template.tmpl"));

        StringAssert.Contains(exception.Message, "res/templates/bindgen/missing-template.tmpl");
    }
}
