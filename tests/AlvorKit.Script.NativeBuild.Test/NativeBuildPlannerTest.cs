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

    /// <summary>Linux package planning skips package commands when no dependencies are configured.</summary>
    [TestMethod]
    public void LinuxInstallCommands_NoPackages_ReturnsEmpty()
    {
        var commands = NativeBuildPlanner.LinuxInstallCommands(TargetRid.Parse("linux-x64"), new());

        Assert.AreEqual(0, commands.Count);
    }

    /// <summary>Linux package planning does not add a foreign architecture for native ARM64 builds.</summary>
    [TestMethod]
    public void LinuxInstallCommands_Arm64Target_UsesPackageListOnly()
    {
        var platform = new PlatformBuildConfig { Packages = ["base"], ArmArchitecture = "armhf", ArmPackages = ["cross"] };

        var commands = NativeBuildPlanner.LinuxInstallCommands(TargetRid.Parse("linux-arm64"), platform);

        Assert.AreEqual(2, commands.Count);
        Assert.AreEqual("apt-get,install,-y,-qq,base", string.Join(",", commands[1].Arguments));
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

    /// <summary>Linux CMake planning emits configure, build, and strip commands.</summary>
    [TestMethod]
    public void CMakeLinuxCommands_IncludesCompilerOptionsAndStrip()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var commands = NativeBuildPlanner.CMakeLinuxCommands(context, TargetRid.Parse("linux-arm"), context.Build.Linux);

            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DCMAKE_C_COMPILER=arm-linux-gnueabihf-gcc");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DCMAKE_CXX_COMPILER=arm-linux-gnueabihf-g++");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DBUILD_SHARED_LIBS=ON");
            Assert.AreEqual("cmake", commands[1].FileName);
            Assert.AreEqual("arm-linux-gnueabihf-strip", commands[2].FileName);
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
            Assert.IsFalse(script.Contains("-prerelease", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Windows CMake script planning configures CMake and copies the configured output file.</summary>
    [TestMethod]
    public void CMakeWindowsScript_ContainsCMakeOptionsAndCopy()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var script = WindowsBuildScripts.CMake(context, TargetRid.Parse("win-arm64"), context.Build.Linux);

            StringAssert.Contains(script, "Launch-VsDevShell.ps1");
            StringAssert.Contains(script, "-DBUILD_SHARED_LIBS=ON");
            StringAssert.Contains(script, "sample.dll");
            StringAssert.Contains(script, "Copy-Item");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Linux CMake planning carries RID-specific options after common options.</summary>
    [TestMethod]
    public void CMakeLinuxCommands_IncludesRidCMakeOptions()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var platform = new PlatformBuildConfig
            {
                CMakeOutput = "src/sample.so",
                CMakeOptions = ["-DCOMMON=1"],
                RidCMakeOptions = new() { ["linux-arm64"] = ["-DARM64=1"] }
            };

            var commands = NativeBuildPlanner.CMakeLinuxCommands(context, TargetRid.Parse("linux-arm64"), platform);

            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DCOMMON=1");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-DARM64=1");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Linux CMake planning merges common and RID-specific child-process environment values.</summary>
    [TestMethod]
    public void CMakeLinuxCommands_IncludesMergedRidEnvironment()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var platform = new PlatformBuildConfig
            {
                CMakeOutput = "src/sample.so",
                Environment = new() { ["COMMON"] = "base", ["TARGET"] = "common" },
                RidEnvironment = new() { ["linux-arm"] = new() { ["TARGET"] = "arm", ["ARM_ONLY"] = "yes" } }
            };

            var commands = NativeBuildPlanner.CMakeLinuxCommands(context, TargetRid.Parse("linux-arm"), platform);

            Assert.AreEqual("base", commands[0].Environment!["COMMON"]);
            Assert.AreEqual("arm", commands[0].Environment!["TARGET"]);
            Assert.AreEqual("yes", commands[1].Environment!["ARM_ONLY"]);
            Assert.IsNull(commands[2].Environment);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Windows CMake script requires the VS Clang component when a manifest requests ClangCL.</summary>
    [TestMethod]
    public void CMakeWindowsScript_WhenClangClRequested_RequiresClangComponent()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var platform = new PlatformBuildConfig
            {
                CMakeOutput = "src/sample.dll",
                CMakeOptions = ["-DCMAKE_CXX_COMPILER=clang-cl"]
            };

            var script = WindowsBuildScripts.CMake(context, TargetRid.Parse("win-x64"), platform);

            StringAssert.Contains(script, "Microsoft.VisualStudio.Component.VC.Llvm.Clang");
            StringAssert.Contains(script, "Tools\\Llvm\\x64\\bin\\clang-cl.exe");
            StringAssert.Contains(script, "\"-DCMAKE_C_COMPILER=$ClangCl\"");
            Assert.IsFalse(script.Contains("-prerelease", StringComparison.Ordinal));
            Assert.IsFalse(script.Contains("-DCMAKE_C_COMPILER=clang-cl", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Windows x86 ClangCL CMake scripts use the x64-host Visual Studio LLVM compiler with an explicit x86 target.</summary>
    [TestMethod]
    public void CMakeWindowsScript_WhenX86ClangClRequested_UsesX64HostCompilerAndX86Target()
    {
        var context = LoadCMakeContext(out var root);
        try
        {
            var platform = new PlatformBuildConfig
            {
                CMakeOutput = "src/sample.dll",
                CMakeOptions = ["-DCMAKE_C_COMPILER=clang-cl", "-DCMAKE_CXX_COMPILER=clang-cl"]
            };

            var script = WindowsBuildScripts.CMake(context, TargetRid.Parse("win-x86"), platform);

            StringAssert.Contains(script, "Tools\\Llvm\\x64\\bin\\clang-cl.exe");
            StringAssert.Contains(script, "-DCMAKE_C_COMPILER_TARGET=i686-pc-windows-msvc");
            StringAssert.Contains(script, "-DCMAKE_CXX_COMPILER_TARGET=i686-pc-windows-msvc");
            StringAssert.Contains(script, "\"-DCMAKE_CXX_COMPILER=$ClangCl\"");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>macOS single-C planning uses clang, deployment target, install name, and strip.</summary>
    [TestMethod]
    public void SingleCMacCommands_UsesClangAndStrip()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var commands = NativeBuildPlanner.SingleCMacCommands(context, TargetRid.Parse("osx-x64"), context.Build.MacOS);

            Assert.AreEqual("clang", commands[0].FileName);
            Assert.IsNotNull(commands[0].WorkingDirectory);
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-arch");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "x86_64");
            CollectionAssert.Contains(commands[0].Arguments.ToList(), "-mmacosx-version-min=11.0");
            Assert.AreEqual("strip", commands[1].FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>linux-arm verification planning uses the cross compiler and an optional emulator prefix.</summary>
    [TestMethod]
    public void VerifyCommands_LinuxArm_UsesCrossCompilerAndRunPrefix()
    {
        var context = LoadXxHashContext(out var root);
        try
        {
            var prefix = new NativeVerifyRunPrefix("qemu-arm", ["-L", "/usr/arm-linux-gnueabihf"]);
            var plan = NativeVerifyPlanner.Create(context, TargetRid.Parse("linux-arm"), prefix);

            Assert.AreEqual("arm-linux-gnueabihf-gcc", plan.CompileCommand.FileName);
            CollectionAssert.Contains(plan.CompileCommand.Arguments.ToList(), "-ldl");
            Assert.AreEqual("qemu-arm", plan.RunCommand.FileName);
            CollectionAssert.AreEqual(
                new[] { "-L", "/usr/arm-linux-gnueabihf", plan.ExecutablePath, plan.LibraryPath, plan.ReportPath, "linux-arm" },
                plan.RunCommand.Arguments.ToArray());
            StringAssert.EndsWith(plan.ReportPath, Path.Combine("out", "native-verify", "xxhash", "linux-arm", "report.json"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>xxhash verification planning keeps platform compiler names and executable suffixes.</summary>
    [TestMethod]
    public void VerifyCommands_XxHashPlatformCompilers_KeepTargetConventions()
    {
        var context = LoadXxHashContext(out var root);
        try
        {
            var windows = NativeVerifyPlanner.Create(context, TargetRid.Parse("win-x64"));
            var mac = NativeVerifyPlanner.Create(context, TargetRid.Parse("osx-arm64"));
            var linux = NativeVerifyPlanner.Create(context, TargetRid.Parse("linux-x64"));

            Assert.AreEqual("cl", windows.CompileCommand.FileName);
            StringAssert.EndsWith(windows.SourcePath, Path.Combine("native", "xxhash", "verify", "verify-xxhash.c"));
            StringAssert.EndsWith(windows.ExecutablePath, "verify-xxhash.exe");
            Assert.AreEqual("clang", mac.CompileCommand.FileName);
            CollectionAssert.Contains(mac.CompileCommand.Arguments.ToList(), "arm64");
            Assert.AreEqual("gcc", linux.CompileCommand.FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>FastNoise2 verification planning uses FastNoise2-specific verifier paths.</summary>
    [TestMethod]
    public void VerifyCommands_FastNoise2_UsesLibrarySpecificPaths()
    {
        var context = LoadVerifyContext("fastnoise2", out var root);
        try
        {
            var windows = NativeVerifyPlanner.Create(context, TargetRid.Parse("win-x64"));
            var linux = NativeVerifyPlanner.Create(context, TargetRid.Parse("linux-x64"));

            StringAssert.EndsWith(windows.SourcePath, Path.Combine("native", "fastnoise2", "verify", "verify-fastnoise2.c"));
            StringAssert.EndsWith(windows.ExecutablePath, "verify-fastnoise2.exe");
            StringAssert.EndsWith(linux.ExecutablePath, "verify-fastnoise2");
            StringAssert.EndsWith(linux.ReportPath, Path.Combine("out", "native-verify", "fastnoise2", "linux-x64", "report.json"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Verification planning rejects libraries without a native verifier contract.</summary>
    [TestMethod]
    public void VerifyCommands_UnsupportedLibrary_Throws()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var error = Assert.ThrowsExactly<NotSupportedException>(
                () => NativeVerifyPlanner.Create(context, TargetRid.Parse("linux-x64")));

            StringAssert.Contains(error.Message, "xxhash and fastnoise2");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Verification planning rejects impossible target platforms through the compiler switch guard.</summary>
    [TestMethod]
    public void VerifyCommands_InvalidTargetOperatingSystem_Throws()
    {
        var context = LoadXxHashContext(out var root);
        try
        {
            var invalid = new TargetRid("fixture", (TargetOperatingSystem)999, TargetArchitecture.X64);

            Assert.ThrowsException<PlatformNotSupportedException>(() => NativeVerifyPlanner.Create(context, invalid));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Linux single-C commands create the build directory before invoking the compiler.</summary>
    [TestMethod]
    public void SingleCLinuxCommands_RequestsWorkingDirectoryCreation()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var command = NativeBuildPlanner.SingleCLinuxCommands(context, TargetRid.Parse("linux-x64"), context.Build.Linux)[0];

            Assert.IsTrue(command.CreateWorkingDirectory);
            StringAssert.EndsWith(command.WorkingDirectory!, Path.Combine("build-linux-x64"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Missing manifest paths for required build inputs are rejected with actionable messages.</summary>
    [TestMethod]
    public void RequiredInputs_MissingConfig_Throw()
    {
        var context = LoadSingleCContext(out var root, out _);
        try
        {
            var noImpl = context with
            {
                Metadata = new BindgenMetadata
                {
                    NativeLibrary = context.Metadata.NativeLibrary,
                    WorkDir = context.Metadata.WorkDir,
                    SourceDir = context.Metadata.SourceDir
                }
            };
            var noOutput = new PlatformBuildConfig();

            StringAssert.Contains(
                Assert.ThrowsException<InvalidOperationException>(() => NativeBuildPlanner.RequiredImplFile(noImpl)).Message,
                "single-c builds require implFile");
            StringAssert.Contains(
                Assert.ThrowsException<InvalidOperationException>(() => NativeBuildPlanner.RequiredCMakeOutput(context, noOutput)).Message,
                "cmake builds require cmakeOutput");
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

    /// <summary>Loads a temporary xxhash context for verifier planning tests.</summary>
    private static LibraryBuildContext LoadXxHashContext(out string root)
    {
        return LoadVerifyContext("xxhash", out root);
    }

    /// <summary>Loads a temporary verifier context for planning tests.</summary>
    private static LibraryBuildContext LoadVerifyContext(string name, out string root)
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        root = TestRepositoryFactory.CreateSingleCLibrary(name, workDir);
        return LibraryBuildContext.Load(new(root), name);
    }
}
