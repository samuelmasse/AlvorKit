namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for native verification orchestration without invoking real compilers.</summary>
[TestClass]
public sealed class NativeVerifyRunnerTest
{
    /// <summary>Verification compiles the xxhash verifier and runs it against the RID runtime library.</summary>
    [TestMethod]
    public async Task VerifyAsync_XxHashLinux_RunsCompileAndVerifierCommands()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        try
        {
            CreateVerifierInputs(context, TargetRid.Parse("linux-x64"));
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(
                context,
                TargetRid.Parse("linux-x64"),
                processRunner,
                new HostInfo(false, true, false, Architecture.X64));

            await runner.VerifyAsync();

            Assert.AreEqual("gcc", processRunner.RunCommands[0].FileName);
            Assert.AreEqual(processRunner.RunCommands[0].Arguments[2], processRunner.RunCommands[1].FileName);
            Assert.AreEqual(context.OutputFile(TargetRid.Parse("linux-x64")), processRunner.RunCommands[1].Arguments[0]);
            Assert.IsTrue(Directory.Exists(Path.Combine(root, "out", "native-verify", "xxhash", "linux-x64")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Verification uses FastNoise2-specific verifier files and output directories.</summary>
    [TestMethod]
    public async Task VerifyAsync_FastNoise2Linux_RunsLibrarySpecificVerifier()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("fastnoise2", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "fastnoise2");
        var target = TargetRid.Parse("linux-x64");
        try
        {
            CreateVerifierInputs(context, target);
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(context, target, processRunner, new HostInfo(false, true, false, Architecture.X64));

            await runner.VerifyAsync();

            StringAssert.EndsWith(processRunner.RunCommands[0].Arguments[2], "verify-fastnoise2");
            StringAssert.EndsWith(
                processRunner.RunCommands[0].Arguments[3],
                Path.Combine("native", "fastnoise2", "verify", "verify-fastnoise2.c"));
            Assert.AreEqual(context.OutputFile(target), processRunner.RunCommands[1].Arguments[0]);
            Assert.AreEqual(Path.Combine(root, "out", "native-verify", "fastnoise2", "linux-x64", "report.json"),
                processRunner.RunCommands[1].Arguments[1]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Verification reports missing native runtime binaries before invoking external tools.</summary>
    [TestMethod]
    public async Task VerifyAsync_MissingRuntimeLibrary_ThrowsBeforeRunningCommands()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        try
        {
            Directory.CreateDirectory(Path.Combine(context.LibraryDirectory, "verify"));
            File.WriteAllText(Path.Combine(context.LibraryDirectory, "verify", "verify-xxhash.c"), "int main(void) { return 0; }");
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(
                context,
                TargetRid.Parse("linux-x64"),
                processRunner,
                new HostInfo(false, true, false, Architecture.X64));

            await Assert.ThrowsExceptionAsync<FileNotFoundException>(() => runner.VerifyAsync());

            Assert.AreEqual(0, processRunner.RunCommands.Count);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>linux-arm verification can run through an environment-provided launcher and sysroot.</summary>
    [TestMethod]
    public async Task VerifyAsync_LinuxArmWithRunnerEnvironment_UsesLauncherPrefix()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        var oldRunner = Environment.GetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER");
        var oldSysroot = Environment.GetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT");
        try
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", "qemu-arm");
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT", "/usr/arm-linux-gnueabihf");
            var target = TargetRid.Parse("linux-arm");
            CreateVerifierInputs(context, target);
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(context, target, processRunner, new HostInfo(false, true, false, Architecture.X64));

            await runner.VerifyAsync();

            var run = processRunner.RunCommands[1];
            Assert.AreEqual("qemu-arm", run.FileName);
            CollectionAssert.AreEqual(
                new[]
                {
                    "-L",
                    "/usr/arm-linux-gnueabihf",
                    processRunner.RunCommands[0].Arguments[2],
                    context.OutputFile(target),
                    Path.Combine(root, "out", "native-verify", "xxhash", "linux-arm", "report.json"),
                    "linux-arm"
                },
                run.Arguments.ToArray());
        }
        finally
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", oldRunner);
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT", oldSysroot);
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>linux-arm launcher planning omits the qemu sysroot option when no sysroot is configured.</summary>
    [TestMethod]
    public async Task VerifyAsync_LinuxArmRunnerWithoutSysroot_UsesLauncherOnly()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        var oldRunner = Environment.GetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER");
        var oldSysroot = Environment.GetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT");
        try
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", "qemu-arm");
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT", null);
            var target = TargetRid.Parse("linux-arm");
            CreateVerifierInputs(context, target);
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(context, target, processRunner, new HostInfo(false, true, false, Architecture.X64));

            await runner.VerifyAsync();

            Assert.AreEqual("qemu-arm", processRunner.RunCommands[1].FileName);
            Assert.AreEqual(processRunner.RunCommands[0].Arguments[2], processRunner.RunCommands[1].Arguments[0]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", oldRunner);
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_SYSROOT", oldSysroot);
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>linux-arm verification on a non-ARM host names the required launcher hook when it is missing.</summary>
    [TestMethod]
    public async Task VerifyAsync_LinuxArmWithoutRunnerEnvironment_ThrowsActionableError()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        var oldRunner = Environment.GetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER");
        try
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", null);
            var runner = new NativeVerifyRunner(
                context,
                TargetRid.Parse("linux-arm"),
                new RecordingProcessRunner(),
                new HostInfo(false, true, false, Architecture.X64));

            var error = await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(() => runner.VerifyAsync());

            StringAssert.Contains(error.Message, "ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ALVORKIT_NATIVE_VERIFY_LINUX_ARM_RUNNER", oldRunner);
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Windows verification compiles the verifier through a generated Visual Studio shell script.</summary>
    [TestMethod]
    public async Task VerifyAsync_XxHashWindows_RunsCompilerThroughPowerShell()
    {
        var root = TestRepositoryFactory.CreateSingleCLibrary("xxhash", "alvorkit-native-test-" + Guid.NewGuid().ToString("N"));
        var context = LibraryBuildContext.Load(new(root), "xxhash");
        try
        {
            var target = TargetRid.Parse("win-x64");
            CreateVerifierInputs(context, target);
            var processRunner = new RecordingProcessRunner();
            var runner = new NativeVerifyRunner(context, target, processRunner, new HostInfo(true, false, false, Architecture.X64));

            await runner.VerifyAsync();

            Assert.AreEqual("pwsh", processRunner.RunCommands[0].FileName);
            Assert.AreEqual("-File", processRunner.RunCommands[0].Arguments[^2]);
            Assert.AreEqual(Path.Combine(root, "out", "native-verify", "xxhash", "win-x64", "verify-xxhash.exe"),
                processRunner.RunCommands[1].FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Creates the source and runtime binary files that the verifier runner requires.</summary>
    private static void CreateVerifierInputs(LibraryBuildContext context, TargetRid target)
    {
        Directory.CreateDirectory(Path.Combine(context.LibraryDirectory, "verify"));
        Directory.CreateDirectory(context.OutputDirectory(target));
        File.WriteAllText(Path.Combine(context.LibraryDirectory, "verify", "verify-" + context.Name + ".c"), "int main(void) { return 0; }");
        File.WriteAllText(context.OutputFile(target), "");
    }
}
