namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Verifies XML documentation contracts on the layer surface.
/// </summary>
[TestClass]
public class GlLayerDocumentationTest
{
    /// <summary>Every generated convenience overload targeting a layer override carries an explicit layer-only remark.</summary>
    [TestMethod]
    public void ConvenienceOverloads_TargetingLayerOverrides_CarryOnlyLayerRemarks()
    {
        var openGlDocs = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "AlvorKit.OpenGL.xml"));
        var layerDocs = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "AlvorKit.OpenGL.Layer.xml"));
        var layerMembers = layerDocs.Descendants("member").ToDictionary(e => e.Attribute("name")!.Value);
        var expected = LayerConvenienceMemberIds(openGlDocs).ToArray();

        var missing = expected
            .Where(id => !layerMembers.TryGetValue(id, out var member) || !Remarks(member).Contains("Layer:", StringComparison.Ordinal))
            .ToArray();
        var copiedConvenienceRemarks = expected
            .Where(id => layerMembers.TryGetValue(id, out var member) && Remarks(member).Contains("Convenience overload", StringComparison.Ordinal))
            .ToArray();

        Assert.IsTrue(expected.Length > 0, "Expected at least one generated convenience overload targeting a layer override.");
        Assert.AreEqual(0, missing.Length, "Missing layer remarks:" + Environment.NewLine + string.Join(Environment.NewLine, missing));
        Assert.AreEqual(
            0,
            copiedConvenienceRemarks.Length,
            "Copied convenience remarks:" + Environment.NewLine + string.Join(Environment.NewLine, copiedConvenienceRemarks));
    }

    private static IEnumerable<string> LayerConvenienceMemberIds(XDocument openGlDocs)
    {
        var layerOverrideNames = typeof(GlLayer)
            .GetMethods(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly)
            .Where(m => m.IsVirtual && m.GetBaseDefinition().DeclaringType != m.DeclaringType)
            .Select(m => m.Name)
            .ToHashSet();

        return openGlDocs
            .Descendants("member")
            .Where(IsConvenienceOverload)
            .Select(e => new
            {
                Member = e.Attribute("name")!.Value,
                Target = e.Element("inheritdoc")?.Attribute("cref")?.Value,
            })
            .Where(e => e.Target is not null && e.Target.StartsWith("M:AlvorKit.OpenGL.Gl.", StringComparison.Ordinal))
            .Where(e => layerOverrideNames.Contains(DocSimpleName(e.Target!)))
            .Select(e => e.Member.Replace("M:AlvorKit.OpenGL.Gl.", "M:AlvorKit.OpenGL.Layer.GlLayer."))
            .Distinct()
            .Order();
    }

    private static bool IsConvenienceOverload(XElement member) =>
        Remarks(member).Contains("Convenience overload", StringComparison.Ordinal);

    private static string Remarks(XElement member) => member.Element("remarks")?.Value ?? string.Empty;

    private static string DocSimpleName(string id)
    {
        var name = id.StartsWith("M:", StringComparison.Ordinal) ? id[2..] : id;
        var paren = name.IndexOf('(');
        if (paren >= 0)
            name = name[..paren];
        var generic = name.IndexOf("``", StringComparison.Ordinal);
        if (generic >= 0)
            name = name[..generic];
        return name.Split('.').Last();
    }
}
