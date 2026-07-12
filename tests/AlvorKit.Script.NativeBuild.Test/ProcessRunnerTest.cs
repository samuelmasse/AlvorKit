namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests shell-free process configuration without starting external tools.</summary>
[TestClass]
public sealed class ProcessRunnerTest
{
    /// <summary>Command environment overrides and literal arguments reach ProcessStartInfo unchanged.</summary>
    [TestMethod]
    public void CreateStartInfo_AppliesEnvironmentWithoutShellExpansion()
    {
        var runner = new ProcessRunner();
        var startInfo = runner.CreateStartInfo(new(
            "cmake",
            ["value with spaces"],
            Environment: new Dictionary<string, string> { ["PKG_CONFIG_LIBDIR"] = "/target/pkgconfig" }),
            redirect: true);

        Assert.IsFalse(startInfo.UseShellExecute);
        Assert.IsTrue(startInfo.RedirectStandardOutput);
        Assert.IsTrue(startInfo.RedirectStandardError);
        Assert.AreEqual("value with spaces", startInfo.ArgumentList.Single());
        Assert.AreEqual("/target/pkgconfig", startInfo.Environment["PKG_CONFIG_LIBDIR"]);
    }
}
