namespace AlvorKit.Script.Workspace.Test;

/// <summary>Tests repository-root template loading and simple placeholder rendering.</summary>
[TestClass]
public sealed class RepositoryTemplatesTest
{
    /// <summary>Rendering replaces placeholders from a template under the root res directory.</summary>
    [TestMethod]
    public void Render_ReplacesPlaceholdersFromRootResTemplate()
    {
        var output = RepositoryTemplates.Render(
            typeof(RepositoryTemplatesTest),
            "res/templates/native-build/windows/verify.ps1.tmpl",
            ("VisualStudioDevShell", "Launch-VsDevShell.ps1"),
            ("OutputFile", "sample.dll"));

        StringAssert.Contains(output, "Launch-VsDevShell.ps1");
        StringAssert.Contains(output, "dumpbin /nologo /dependents sample.dll");
    }

    /// <summary>Template sets render files relative to one template area.</summary>
    [TestMethod]
    public void ForArea_RendersTemplatesRelativeToArea()
    {
        var templates = RepositoryTemplates.ForArea(typeof(RepositoryTemplatesTest), "native-build/windows");

        var output = templates.Render(
            "verify.ps1.tmpl",
            ("VisualStudioDevShell", "Launch-VsDevShell.ps1"),
            ("OutputFile", "sample.dll"));

        StringAssert.Contains(output, "Launch-VsDevShell.ps1");
        StringAssert.Contains(output, "dumpbin /nologo /dependents sample.dll");
    }

    /// <summary>Template areas must be non-empty relative paths.</summary>
    [TestMethod]
    public void ForArea_RejectsInvalidTemplateAreas()
    {
        Assert.ThrowsException<ArgumentException>(() => RepositoryTemplates.ForArea(typeof(RepositoryTemplatesTest), ""));
        Assert.ThrowsException<ArgumentException>(() => RepositoryTemplates.ForArea(typeof(RepositoryTemplatesTest), "/templates"));
    }

    /// <summary>Template sets reject relative paths that escape the selected template area.</summary>
    [TestMethod]
    public void ForArea_RejectsTemplateNamesOutsideArea()
    {
        var templates = RepositoryTemplates.ForArea(typeof(RepositoryTemplatesTest), "native-build/windows");

        Assert.ThrowsException<ArgumentException>(() => templates.Read(""));
        Assert.ThrowsException<ArgumentException>(() => templates.Read("../maths/field.csfrag.tmpl"));
        Assert.ThrowsException<ArgumentException>(() => templates.Read("./verify.ps1.tmpl"));
    }

    /// <summary>Fragment rendering trims template padding and returns exactly one trailing blank line.</summary>
    [TestMethod]
    public void RenderFragment_NormalizesTrailingNewlines()
    {
        var output = RepositoryTemplates.RenderFragment(
            typeof(RepositoryTemplatesTest),
            "res/templates/maths/field.csfrag.tmpl",
            ("Summary", "A field."),
            ("Offset", "0"),
            ("ScalarType", "int"),
            ("Name", "X"),
            ("Initializer", ""));

        Assert.IsTrue(output.EndsWith(Environment.NewLine + Environment.NewLine, StringComparison.Ordinal));
        Assert.IsFalse(output.EndsWith(Environment.NewLine + Environment.NewLine + Environment.NewLine, StringComparison.Ordinal));
    }

    /// <summary>Missing placeholder values fail loudly instead of leaking template syntax into generated output.</summary>
    [TestMethod]
    public void Render_MissingPlaceholderValueThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => RepositoryTemplates.Render(
            typeof(RepositoryTemplatesTest),
            "res/templates/native-build/windows/verify.ps1.tmpl",
            ("VisualStudioDevShell", "Launch-VsDevShell.ps1")));

        StringAssert.Contains(exception.Message, "{{OutputFile}}");
    }

    /// <summary>Duplicate placeholder values fail before rendering to avoid ambiguous generated output.</summary>
    [TestMethod]
    public void Render_DuplicatePlaceholderValueThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateRenderer.Render(
            "{{Name}}",
            "fixture.tmpl",
            "\n",
            ("Name", "first"),
            ("Name", "second")));

        StringAssert.Contains(exception.Message, "{{Name}}");
    }

    /// <summary>Malformed placeholder syntax fails loudly before output is generated.</summary>
    [TestMethod]
    public void Render_MalformedPlaceholderThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateRenderer.Render(
            "{{Bad Name}}",
            "fixture.tmpl",
            "\n"));

        StringAssert.Contains(exception.Message, "malformed placeholder");
    }

    /// <summary>Unterminated placeholders fail loudly before output is generated.</summary>
    [TestMethod]
    public void Render_UnterminatedPlaceholderThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateRenderer.Render(
            "{{Name",
            "fixture.tmpl",
            "\n"));

        StringAssert.Contains(exception.Message, "unterminated placeholder");
    }

    /// <summary>Template paths must stay under the repository root res directory.</summary>
    [TestMethod]
    public void Read_RejectsPathOutsideRootResDirectory()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => RepositoryTemplates.Read(
            typeof(RepositoryTemplatesTest),
            "scripts/AlvorKit.Script.Workspace/RepositoryTemplates.cs"));

        StringAssert.Contains(exception.Message, "outside the repository root res directory");
    }

    /// <summary>Missing root res files report the unresolved path and concrete filename.</summary>
    [TestMethod]
    public void Read_MissingRootResFileThrows()
    {
        var exception = Assert.ThrowsException<FileNotFoundException>(() => RepositoryTemplates.Read(
            typeof(RepositoryTemplatesTest),
            "res/templates/native-build/missing-template.tmpl"));

        StringAssert.Contains(exception.Message, "res/templates/native-build/missing-template.tmpl");
    }
}
