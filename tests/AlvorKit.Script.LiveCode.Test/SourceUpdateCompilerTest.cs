namespace AlvorKit;

/// <summary>Proves strict diff handling and a real public-Roslyn two-generation delta chain.</summary>
[TestClass]
public sealed class SourceUpdateCompilerTest
{
    /// <summary>A strict one-file diff applies to the acknowledged source and rejects stale context.</summary>
    [TestMethod]
    public void DiffAppliesExactSingleFileChange()
    {
        using var workspace = TempWorkspace.Create();
        var sourcePath = workspace.Write("Game/Service.cs", "class Service\n{\n    int Value() => 1;\n}\n");
        var relative = Path.GetRelativePath(workspace.Root, sourcePath).Replace('\\', '/');
        var diff = $$"""
            --- a/{{relative}}
            +++ b/{{relative}}
            @@ -1,4 +1,4 @@
             class Service
             {
            -    int Value() => 1;
            +    int Value() => 2;
             }
            """;

        var result = SourceUpdateDiff.Apply(
            File.ReadAllText(sourcePath),
            diff,
            sourcePath,
            workspace.Root);

        StringAssert.Contains(result.Source, "Value() => 2");
        Assert.ThrowsExactly<InvalidDataException>(() =>
            SourceUpdateDiff.Apply(
                result.Source,
                diff,
                sourcePath,
                workspace.Root));
    }

    /// <summary>The exact built fixture emits two ordered method-only deltas from one retained baseline chain.</summary>
    [TestMethod]
    public async Task ProjectBaselineEmitsTwoChainedMethodUpdates()
    {
        var repositoryRoot = FindRepositoryRoot();
        var project = Path.Combine(
            repositoryRoot,
            "tests",
            "AlvorKit.Script.LiveCode.Fixture",
            "AlvorKit.Script.LiveCode.Fixture.csproj");
        var sourcePath = Path.Combine(
            Path.GetDirectoryName(project)!,
            "EditableService.cs");
        var assemblyPath = typeof(EditableService).Assembly.Location;
        var launch = Launch(project, assemblyPath);
        using var baseline = await SourceUpdateProjectBaseline.Create(
            launch,
            CancellationToken.None);
        var original = File.ReadAllText(sourcePath);
        var firstSource = original.Replace(
            "var next = value + delta;",
            "var next = value + (delta * 4);",
            StringComparison.Ordinal);
        var first = await baseline.Prepare(
            sourcePath,
            Diff(repositoryRoot, sourcePath, original, firstSource),
            firstSource,
            repositoryRoot,
            "generation-1",
            CancellationToken.None);

        Assert.AreEqual(0, first.PreviousGeneration);
        Assert.AreEqual(
            typeof(EditableService).GetMethod(nameof(EditableService.Update))!.MetadataToken,
            first.Request.ExpectedMethodToken);
        CollectionAssert.AreEqual(
            new[] { typeof(EditableService).MetadataToken },
            first.Request.ChangedTypeTokens);
        Assert.IsTrue(first.Request.MetadataDelta.Length > 0);
        Assert.IsTrue(first.Request.IlDelta.Length > 0);
        baseline.Commit(first);

        var secondSource = firstSource.Replace(
            "delta * 4",
            "delta * 7",
            StringComparison.Ordinal);
        var second = await baseline.Prepare(
            sourcePath,
            Diff(repositoryRoot, sourcePath, firstSource, secondSource),
            secondSource,
            repositoryRoot,
            "generation-2",
            CancellationToken.None);

        Assert.AreEqual(1, second.PreviousGeneration);
        Assert.AreEqual(1, second.Request.ExpectedGeneration);
        Assert.AreNotEqual(
            first.Request.MetadataDeltaHash,
            second.Request.MetadataDeltaHash);
    }

    /// <summary>A method edit cannot start capturing a previously uncaptured primary-constructor parameter.</summary>
    [TestMethod]
    public async Task ProjectBaselineRejectsNewPrimaryConstructorCapture()
    {
        var repositoryRoot = FindRepositoryRoot();
        var project = Path.Combine(
            repositoryRoot,
            "tests",
            "AlvorKit.Script.LiveCode.Fixture",
            "AlvorKit.Script.LiveCode.Fixture.csproj");
        var sourcePath = Path.Combine(
            Path.GetDirectoryName(project)!,
            "EditableService.cs");
        using var baseline = await SourceUpdateProjectBaseline.Create(
            Launch(project, typeof(EditableService).Assembly.Location),
            CancellationToken.None);
        var original = File.ReadAllText(sourcePath);
        const string oldLine =
            "        return $\"{reference.Identity}:{dependency.Identity}:{value}\";";
        const string newLine =
            "        return $\"{reference.Identity}:{dependency.Identity}:{uncaptured}:{value}\";";
        var updated = original.Replace(oldLine, newLine, StringComparison.Ordinal);

        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => baseline.Prepare(
                sourcePath,
                DiffLine(repositoryRoot, sourcePath, original, oldLine, newLine),
                updated,
                repositoryRoot,
                "new-capture",
                CancellationToken.None));

        StringAssert.Contains(exception.Message, "newly captures");
    }

    private static SourceUpdateCompilerLaunch Launch(
        string project,
        string assemblyPath)
    {
        var pdbPath = Path.ChangeExtension(assemblyPath, ".pdb");
        using var stream = File.OpenRead(assemblyPath);
        using var pe = new PEReader(stream);
        var metadata = pe.GetMetadataReader();
        var codeView = pe.ReadDebugDirectory()
            .Single(static entry => entry.Type == DebugDirectoryEntryType.CodeView);
        return new(
            1,
            project,
            assemblyPath,
            pdbPath,
            HashFile(assemblyPath),
            HashFile(pdbPath),
            metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString("N"),
            "fixture-project",
            "fixture-sdk",
            pe.ReadCodeViewDebugDirectoryData(codeView).Path);
    }

    private static string Diff(
        string repositoryRoot,
        string sourcePath,
        string oldSource,
        string newSource)
    {
        var oldLine = oldSource.Split('\n')
            .Single(static line => line.Contains("var next =", StringComparison.Ordinal))
            .TrimEnd('\r');
        var newLine = newSource.Split('\n')
            .Single(static line => line.Contains("var next =", StringComparison.Ordinal))
            .TrimEnd('\r');
        return DiffLine(repositoryRoot, sourcePath, oldSource, oldLine, newLine);
    }

    private static string DiffLine(
        string repositoryRoot,
        string sourcePath,
        string oldSource,
        string oldLine,
        string newLine)
    {
        var relative = Path.GetRelativePath(repositoryRoot, sourcePath).Replace('\\', '/');
        var lineNumber = Array.FindIndex(
            oldSource.Split('\n'),
            line => line.TrimEnd('\r') == oldLine) + 1;
        if (lineNumber == 0)
            throw new InvalidOperationException("Expected diff line was not found.");
        return $$"""
            --- a/{{relative}}
            +++ b/{{relative}}
            @@ -{{lineNumber}},1 +{{lineNumber}},1 @@
            -{{oldLine}}
            +{{newLine}}
            """;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AlvorKit.slnx")))
            directory = directory.Parent;
        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find the AlvorKit repository root.");
    }

    private static string HashFile(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }
}
