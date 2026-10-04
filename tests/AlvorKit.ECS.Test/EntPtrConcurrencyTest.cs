namespace AlvorKit;

[TestClass]
public class EntPtrConcurrencyTest
{
    /// <summary>Standalone pointers retain distinct sparse storage during concurrent allocation and release.</summary>
    [TestMethod]
    public void ConcurrentAllocateAndDispose_PreservesIndependentStorage()
    {
        var ents = new EntPtr[1024];
        try
        {
            Parallel.For(0, ents.Length, index =>
            {
                var ent = new EntPtr();
                ents[index] = ent;
                Assert.IsFalse(ent.HasFirst);
                ent.First = index;
                ent.Third = $"Ent {index}";
            });

            Assert.AreEqual(ents.Length, ents.Select(ent => ent.Handle).Distinct().Count());
            Parallel.For(0, ents.Length, index =>
            {
                var ent = ents[index];
                Assert.AreEqual(index, ent.First);
                Assert.AreEqual($"Ent {index}", ent.Third);
                ent.Dispose();
                Assert.IsFalse(ent.IsAlive);
                Assert.IsFalse(ent.HasFirst);
                Assert.IsNull(ent.Third);
            });
        }
        finally
        {
            foreach (var ent in ents)
                ent.Dispose();
        }
    }

    /// <summary>Independent sparse components can be attached concurrently to one explicitly owned pointer.</summary>
    [TestMethod]
    public void ConcurrentSetDifferentComponents_PreservesAllValues()
    {
        using var ent = new EntPtr();
        Parallel.Invoke(
            () => ent.Set<int, FirstComponent>(42),
            () => ent.Set<float, SecondComponent>(3.14f),
            () => ent.Set<string, ThirdComponent>("ECS"));

        Assert.AreEqual(42, ent.Get<int, FirstComponent>());
        Assert.AreEqual(3.14f, ent.Get<float, SecondComponent>());
        Assert.AreEqual("ECS", ent.Get<string, ThirdComponent>());
    }
}
