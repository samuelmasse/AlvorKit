namespace AlvorKit.ECS.Test;

[TestClass]
public sealed class EntArchCreateTest
{
    /// <summary>Verifies final-shape allocation materializes one arch and writes aligned values without intermediate transitions.</summary>
    [TestMethod]
    public void FinalShapeCreate_MultipleOrders_AppendDirectlyToOneArch()
    {
        using var arena = new EntArena();
        var firstRef = new RefBox(30);
        var secondRef = new RefBox(31);

        EntPtr first = arena
            .AllocArchetypal<FinalShapeArch>()
            .With<RefBox, C2>(firstRef)
            .With<int, C0>(10)
            .With<long, C1>(20L)
            .Create();

        EntPtr second = arena
            .AllocArchetypal<FinalShapeArch>()
            .With<long, C1>(21L)
            .With<RefBox, C2>(secondRef)
            .With<int, C0>(11)
            .Create();

        var firstLoc = first.Get<EntArchLoc, FinalShapeArch>();
        var secondLoc = second.Get<EntArchLoc, FinalShapeArch>();
        var metrics = EntArchDiagnostics<FinalShapeArch>.Capture();

        Assert.AreEqual(3, metrics.RegisteredFieldCount);
        Assert.AreEqual(1, metrics.MaterializedArchCount);
        Assert.AreEqual(1, metrics.ActiveStateCount);
        Assert.AreEqual(firstLoc.ArchId, secondLoc.ArchId);
        Assert.AreEqual(0, firstLoc.Row);
        Assert.AreEqual(1, secondLoc.Row);
        Assert.AreEqual(10, first.GetArchetypal<int, C0, FinalShapeArch>());
        Assert.AreEqual(20L, first.GetArchetypal<long, C1, FinalShapeArch>());
        Assert.AreSame(firstRef, first.GetArchetypal<RefBox, C2, FinalShapeArch>());
        Assert.AreEqual(11, second.GetArchetypal<int, C0, FinalShapeArch>());
        Assert.AreEqual(21L, second.GetArchetypal<long, C1, FinalShapeArch>());
        Assert.AreSame(secondRef, second.GetArchetypal<RefBox, C2, FinalShapeArch>());

        int queried = 0;
        foreach (var chunk in arena.QueryArchetypal<FinalShapeArch>()
            .With<int, C0>()
            .With<long, C1>()
            .With<RefBox, C2>())
        {
            queried += chunk.Ents.Length;
        }

        Assert.AreEqual(2, queried);
    }

    /// <summary>Verifies a one-field final shape participates in ordinary point access and lifecycle operations.</summary>
    [TestMethod]
    public void FinalShapeCreate_Singleton_UsesOrdinaryLifecycle()
    {
        using var arena = new EntArena();
        EntPtr ent = arena
            .AllocArchetypal<SingletonCreateArch>()
            .With<int, C0>(42)
            .Create();

        Assert.AreEqual(42, ent.GetArchetypal<int, C0, SingletonCreateArch>());
        ent.SetArchetypal<int, C0, SingletonCreateArch>(43);
        Assert.AreEqual(43, ent.GetArchetypal<int, C0, SingletonCreateArch>());
        Assert.IsTrue(ent.UnsetArchetypal<int, C0, SingletonCreateArch>());
        Assert.IsFalse(ent.Has<EntArchLoc, SingletonCreateArch>());
    }

    private sealed record RefBox(int Value);

    private readonly record struct C0;
    private readonly record struct C1;
    private readonly record struct C2;
    private readonly record struct FinalShapeArch;
    private readonly record struct SingletonCreateArch;
}
