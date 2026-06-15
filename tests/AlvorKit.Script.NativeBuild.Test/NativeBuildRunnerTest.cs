namespace AlvorKit.Script.NativeBuild.Test;

/// <summary>Tests for native build runner orchestration without invoking real tools.</summary>
[TestClass]
public sealed class NativeBuildRunnerTest
{
    /// <summary>Linux single-C builds install packages, compile, strip and verify dependencies.</summary>
    [TestMethod]
    public async Task BuildAsync_LinuxSingleC_RunsExpectedCommands()
    {
        var workDir = "alvorkit-native-test-" + Guid.NewGuid().ToString("N");
        var root = TestRepositoryFactory.CreateSingleCLibrary("sample", workDir);
        var context = LibraryBuildContext.Load(new(root), "sample");
        var processRunner = new RecordingProcessRunner
        {
            CaptureOutput = "0x0001 (NEEDED) Shared library: [libc.so.6]\n"
        };
        try
        {
            Directory.CreateDirectory(context.SourceDirectory);
            var runner = new NativeBuildRunner(
                context,
                TargetRid.Parse("linux-x64"),
                processRunner,
                new HostInfo(false, true, false, Architecture.X64));

            await runner.BuildAsync();

            Assert.AreEqual("sudo,sudo,gcc,strip", string.Join(",", processRunner.RunCommands.Select(command => command.FileName)));
            Assert.AreEqual("readelf", processRunner.CaptureCommands.Single().FileName);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
            if (Directory.Exists(context.WorkRoot))
                Directory.Delete(context.WorkRoot, recursive: true);
        }
    }
}
