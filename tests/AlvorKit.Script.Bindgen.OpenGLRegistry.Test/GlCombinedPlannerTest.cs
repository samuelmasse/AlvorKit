namespace AlvorKit.Script.Bindgen.OpenGLRegistry.Test;

/// <summary>Tests for combined OpenGL overload planning edge cases.</summary>
[TestClass]
public sealed class GlCombinedPlannerTest
{
    /// <summary>Shared counts restore planned spans when another pointer still needs the raw count.</summary>
    [TestMethod]
    public void CountResolver_WithUnplannedSharedPointer_RestoresSpannedPointer()
    {
        var state = new GlExtensionEmissionState(OpenGlRegistryTestConfig.Create(), new HashSet<string>(StringComparer.Ordinal));
        var plan = new GlCombinedOverloadPlan(Command(
            "glSharedCount",
            [
                Parameter("count", "count", "int", "int"),
                Parameter("values", "values", "nint", "nint", "count", 1, "float", true, false),
                Parameter("raw", "raw", "nint", "nint", "count", 1, null, true, false)
            ]));

        new GlCombinedSpanPlanner(state).Apply(plan);
        Assert.AreEqual(GlExtensionPlanKind.SpanTyped, plan.Plans[1]);
        CollectionAssert.Contains(plan.SpannedPointers.ToArray(), 1);

        new GlCombinedCountResolver(state).Apply(plan);

        Assert.AreEqual(GlExtensionPlanKind.Keep, plan.Plans[0]);
        Assert.AreEqual(GlExtensionPlanKind.Keep, plan.Plans[1]);
        Assert.IsFalse(plan.SpannedPointers.Contains(1));
    }

    /// <summary>Creates a command with the standard void return and OpenGL 1.0 availability.</summary>
    private static GlCommand Command(string nativeName, IReadOnlyList<GlParameter> parameters) =>
        new(nativeName, "SharedCount", "void", "void", parameters, new("1.0", null), Documentation: null, ReturnsCString: false);

    /// <summary>Creates an OpenGL command parameter.</summary>
    private static GlParameter Parameter(
        string nativeName,
        string managedName,
        string managedType,
        string interopType,
        string? len = null,
        int pointerDepth = 0,
        string? pointeeType = null,
        bool pointeeIsConst = false,
        bool pointeeIsChar = false) =>
        new(nativeName, managedName, managedType, interopType, len, pointerDepth, pointeeType, pointeeIsConst, pointeeIsChar);
}
