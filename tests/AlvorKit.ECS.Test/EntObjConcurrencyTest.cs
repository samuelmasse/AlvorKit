namespace AlvorKit.ECS.Test;

[TestClass]
public class EntObjConcurrencyTest
{
    /// <summary>Verifies EntObj Concurrency CreatingEntitiesInParallel.</summary>
    [TestMethod]
    public void EntObj_Concurrency_CreatingEntitiesInParallel()
    {
        int entCount = 1000;
        EntObj[] entities = new EntObj[entCount];

        Parallel.For(0, entCount, i =>
        {
            entities[i] = new EntObj();
            Assert.IsFalse(entities[i].Unset<int, FirstComponent>());
            Assert.IsFalse(entities[i].Has<int, FirstComponent>());
            entities[i].Set<int, FirstComponent>(i);
        });

        for (int i = 0; i < entCount; i++)
            Assert.AreEqual(i, entities[i].Get<int, FirstComponent>());
    }

    /// <summary>Verifies EntObj Concurrency SetDifferentComponents WorksCorrectly.</summary>
    [TestMethod]
    public void EntObj_Concurrency_SetDifferentComponents_WorksCorrectly()
    {
        var ent = new EntObj();

        Parallel.Invoke(
            () => ent.Set<int, FirstComponent>(42),
            () => ent.Set<float, SecondComponent>(3.14f),
            () => ent.Set<string, ThirdComponent>("ECS")
        );

        Assert.AreEqual(42, ent.Get<int, FirstComponent>());
        Assert.AreEqual(3.14f, ent.Get<float, SecondComponent>());
        Assert.AreEqual("ECS", ent.Get<string, ThirdComponent>());
    }

    /// <summary>Verifies EntObj Concurrency MultipleThreadsModifyComponents CorrectlyStoresValues.</summary>
    [TestMethod]
    public void EntObj_Concurrency_MultipleThreadsModifyComponents_CorrectlyStoresValues()
    {
        var ent = new EntObj();
        int iterations = 1000;

        Parallel.For(0, iterations, i =>
        {
            ent.Set<int, FirstComponent>(i);
            ent.Set<float, SecondComponent>(i * 1.1f);
            ent.Set<string, ThirdComponent>($"Value {i}");
        });

        int lastInt = ent.Get<int, FirstComponent>();
        float lastFloat = ent.Get<float, SecondComponent>();
        string? lastString = ent.Get<string, ThirdComponent>();

        Assert.IsTrue(lastInt >= 0 && lastInt < iterations);
        Assert.IsTrue(lastFloat >= 0 && lastFloat < iterations * 1.1f);
        Assert.IsTrue(lastString?.StartsWith("Value "));
    }

    /// <summary>Verifies EntObj Concurrency FreeAndReuseEntities.</summary>
    [TestMethod]
    public void EntObj_Concurrency_FreeAndReuseEntities()
    {
        int entCount = 1000;
        int[] indices = new int[1000];

        Parallel.For(0, entCount, i =>
        {
            var ent = new EntObj();
            ent.Set<int, FirstComponent>(i);
            indices[i] = ent.Index;
        });

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var uniques = new HashSet<int>(indices);
        Assert.AreEqual(indices.Length, uniques.Count); // ids were all different

        var list = new List<EntObj>();
        bool wasReused = false;
        for (int i = 0; i < 9000; i++)
        {
            var newEnt = new EntObj();
            list.Add(newEnt);

            if (uniques.Contains(newEnt.Index))
            {
                wasReused = true;
                break;
            }
        }

        Assert.IsTrue(wasReused); // but after GC it is reused
    }
}
