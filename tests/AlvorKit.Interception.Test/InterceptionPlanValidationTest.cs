namespace AlvorKit.Interception.Test;

/// <summary>Verifies public plans cannot bypass their construction invariants.</summary>
[TestClass]
public sealed class InterceptionPlanValidationTest
{
    private static readonly MethodInfo TargetMethod =
        typeof(InterceptionPlanValidationTest).GetMethod(
            nameof(Target),
            BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly InterceptionMethodBody Body =
        InterceptionMethodBody.FromRaw([0x06, 0x2A]);

    /// <summary>A replacement plan rejects the invalid default target.</summary>
    [TestMethod]
    public void Constructor_DefaultTarget_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => new InterceptionPlan(default, Body));
    }

    /// <summary>A replacement plan rejects code-generation bits it does not define.</summary>
    [TestMethod]
    public void Constructor_UnknownFlags_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => new InterceptionPlan(
                InterceptionTarget.FromMethod(TargetMethod),
                Body,
                (InterceptionPatchFlags)uint.MaxValue));
    }

    /// <summary>A dispatch plan validates a target supplied without reflection.</summary>
    [TestMethod]
    public void ForTarget_DefaultTarget_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(
            () => InterceptionDispatchPlan.ForTarget(default, 1, 1));
    }

    private static void Target()
    {
    }
}
