namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests repository-root template file loading for native-build script emitters.</summary>
[TestClass]
public sealed class TemplateResourceTest
{
    /// <summary>Rendering replaces placeholders from a native-build template under the root res directory.</summary>
    [TestMethod]
    public void Render_ReplacesPlaceholdersFromRootResTemplate()
    {
        var output = TemplateResource.Render(
            "res/templates/native-build/windows/verify.ps1.tmpl",
            ("VisualStudioDevShell", "Launch-VsDevShell.ps1"),
            ("OutputFile", "sample.dll"));

        StringAssert.Contains(output, "Launch-VsDevShell.ps1");
        StringAssert.Contains(output, "dumpbin /nologo /dependents sample.dll");
    }

    /// <summary>Missing placeholder values fail loudly instead of leaking template syntax into generated scripts.</summary>
    [TestMethod]
    public void Render_MissingPlaceholderValueThrows()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateResource.Render(
            "res/templates/native-build/windows/verify.ps1.tmpl",
            ("VisualStudioDevShell", "Launch-VsDevShell.ps1")));

        StringAssert.Contains(exception.Message, "{{OutputFile}}");
    }

    /// <summary>Template paths must stay under the repository root res directory.</summary>
    [TestMethod]
    public void Read_RejectsPathOutsideRootResDirectory()
    {
        var exception = Assert.ThrowsException<InvalidOperationException>(() => TemplateResource.Read(
            "scripts/AlvorKit.Script.NativeBuild/TemplateResource.cs"));

        StringAssert.Contains(exception.Message, "outside the repository root res directory");
    }

    /// <summary>Missing root res files report the unresolved path and concrete filename.</summary>
    [TestMethod]
    public void Read_MissingRootResFileThrows()
    {
        var exception = Assert.ThrowsException<FileNotFoundException>(() => TemplateResource.Read(
            "res/templates/native-build/missing-template.tmpl"));

        StringAssert.Contains(exception.Message, "res/templates/native-build/missing-template.tmpl");
    }
}
