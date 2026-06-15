namespace AlvorKit.Script.BindgenReview.Test;

/// <summary>Tests path generation and safety checks for bindgen review sessions.</summary>
[TestClass]
public sealed class BindgenReviewPathsTest
{
    /// <summary>Create appends a five-character suffix and normalizes human case names.</summary>
    [TestMethod]
    public void Create_WithCaseName_ReturnsSuffixedSlug()
    {
        using var workspace = TempWorkspace.Create();

        var session = BindgenReviewPaths.Create(workspace.Root, "xxHash", "Typed overloads!", () => "a1b2c");

        Assert.AreEqual("out/bindgen-review/xxhash-typed-overloads-a1b2c", session.RelativeRoot);
        Assert.AreEqual("out/bindgen-review/xxhash-typed-overloads-a1b2c/before", session.BeforeRelativeRoot);
        Assert.AreEqual("out/bindgen-review/xxhash-typed-overloads-a1b2c/after", session.AfterRelativeRoot);
        StringAssert.EndsWith(session.BeforeRoot, Path.Combine("xxhash-typed-overloads-a1b2c", "before"));
        StringAssert.EndsWith(session.AfterRoot, Path.Combine("xxhash-typed-overloads-a1b2c", "after"));
    }

    /// <summary>Create omits the case segment when the case slug matches the library slug.</summary>
    [TestMethod]
    public void Create_WithoutCaseName_ReturnsLibrarySlug()
    {
        using var workspace = TempWorkspace.Create();

        var session = BindgenReviewPaths.Create(workspace.Root, "xxhash", null, () => "a1b2c");

        Assert.AreEqual("out/bindgen-review/xxhash-a1b2c", session.RelativeRoot);
    }

    /// <summary>Create falls back to a generic case segment when human text has no safe characters.</summary>
    [TestMethod]
    public void Create_CaseNameWithoutSafeCharacters_UsesFallbackSlug()
    {
        using var workspace = TempWorkspace.Create();

        var session = BindgenReviewPaths.Create(workspace.Root, "xxhash", "!!", () => "a1b2c");

        Assert.AreEqual("out/bindgen-review/xxhash-case-a1b2c", session.RelativeRoot);
    }

    /// <summary>Existing accepts the disposable review base directory itself.</summary>
    [TestMethod]
    public void Existing_ReviewBase_ReturnsSession()
    {
        using var workspace = TempWorkspace.Create();

        var session = BindgenReviewPaths.Existing(workspace.Root, "out/bindgen-review");

        Assert.AreEqual("out/bindgen-review", session.RelativeRoot);
    }

    /// <summary>Existing rejects paths outside the disposable review directory.</summary>
    [TestMethod]
    public void Existing_OutsideReviewRoot_Throws()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsExactly<InvalidOperationException>(() => BindgenReviewPaths.Existing(workspace.Root, "out/not-review/case"));
    }

    /// <summary>Create rejects suffix factories that do not satisfy the random suffix contract.</summary>
    [TestMethod]
    public void Create_InvalidSuffix_Throws()
    {
        using var workspace = TempWorkspace.Create();

        Assert.ThrowsExactly<InvalidOperationException>(() => BindgenReviewPaths.Create(workspace.Root, "xxhash", null, () => "abc"));
        Assert.ThrowsExactly<InvalidOperationException>(() => BindgenReviewPaths.Create(workspace.Root, "xxhash", null, () => "AB12C"));
    }

    /// <summary>RandomSuffix returns a lowercase alphanumeric five-character string.</summary>
    [TestMethod]
    public void RandomSuffix_ReturnsFiveSafeCharacters()
    {
        var suffix = BindgenReviewPaths.RandomSuffix();

        Assert.AreEqual(5, suffix.Length);
        Assert.IsTrue(suffix.All(character => character is >= 'a' and <= 'z' or >= '0' and <= '9'));
    }
}
