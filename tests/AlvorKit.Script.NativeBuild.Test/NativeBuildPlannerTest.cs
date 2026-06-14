using AlvorKit.Script.NativeBuild;

namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for pure native build command planning.</summary>
[TestClass]
public sealed class NativeBuildPlannerTest
{
    /// <summary>Linux package planning adds dpkg architecture setup for arm cross builds.</summary>
    [TestMethod]
    public void LinuxInstallCommands_ArmTarget_AddsArchitecture()
    {
        var platform = new PlatformBuildConfig { Packages = ["base"], ArmArchitecture = "armhf", ArmPackages = ["cross"] };

        var commands = NativeBuildPlanner.LinuxInstallCommands(TargetRid.Parse("linux-arm"), platform);

        Assert.AreEqual("dpkg", commands[0].Arguments[0]);
        Assert.AreEqual("apt-get,install,-y,-qq,base,cross", string.Join(",", commands[2].Arguments));
    }

    /// <summary>Linux single-C planning selects the target compiler and link libraries.</summary>
    [TestMethod]
    public void SingleCLinuxCommands_UsesCompilerAndLibraries()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var commands = NativeBuildPlanner.SingleCLinuxCommands(context, TargetRid.Parse("linux-x64"), context.Build.Linux);

            Assert.AreEqual("gcc", commands[0].FileName);
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-lm");
            Assert.AreEqual("strip", commands[1].FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>macOS CMake planning carries architecture, deployment target and manifest options.</summary>
    [TestMethod]
    public void CMakeMacCommands_IncludesArchitectureAndOptions()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var commands = NativeBuildPlanner.CMakeMacCommands(context, TargetRid.Parse("osx-arm64"), context.Build.MacOS);

            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DCMAKE_OSX_ARCHITECTURES=arm64");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DBUILD_SHARED_LIBS=ON");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Windows script planning uses PowerShell quoting for paths and MSVC options.</summary>
    [TestMethod]
    public void SingleCWindowsScript_ContainsMsvcSetupAndOutput()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var script = WindowsBuildScripts.SingleC(context, TargetRid.Parse("win-x64"), context.Build.Windows);

            StringAssert.Contains(script, "Launch-VsDevShell.ps1");
            StringAssert.Contains(script, "/MT /LD");
            StringAssert.Contains(script, "sample.dll");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Loads a temporary single-C context for planner tests.</summary>
    private static LibraryBuildContext LoadSingleCContext(out string root, out string workDir)
    {
        workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        return LibraryBuildContext.Load(new(root), "sample");
    }

    /// <summary>Loads a temporary CMake context for planner tests.</summary>
    private static LibraryBuildContext LoadCMakeContext(out string root)
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        root = TestRepositoryFactory.CreateCMakeLibrary("sample", workDir);
        return LibraryBuildContext.Load(new(root), "sample");
    }
}
