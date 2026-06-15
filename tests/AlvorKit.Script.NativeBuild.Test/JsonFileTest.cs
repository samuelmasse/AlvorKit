namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for manifest JSON loading behavior.</summary>
[TestClass]
public sealed class JsonFileTest
{
    /// <summary>JSON loading accepts trailing commas and case-insensitive property names.</summary>
    [TestMethod]
    public void Read_AllowsRepositoryJsonStyle()
    {
        var path = Path.Combine(Path.GetTempPath(), "alvorkit-json-" + Guid.NewGuid().ToString("N") + ".json");
        File.WriteAllText(path, """
            {
                "NATIVELIBRARY": "sample",
                "workDir": "work",
                "sourceDir": "src",
            }
            """);
        try
        {
            var metadata = JsonFile.Read<BindgenMetadata>(path);

            Assert.AreEqual("sample", metadata.NativeLibrary);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
