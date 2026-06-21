namespace AlvorKit.ECS.Test;

[TestClass]
public class EntArenaConcurrencyTest
{
    /// <summary>Verifies independent arenas can allocate and dispose entities concurrently.</summary>
    [TestMethod]
    public void EntArena_ConcurrentIndependentArenas_AreIsolated()
    {
        const int count = 6;
        const int entitiesPerArena = 128;
        var arenas = Enumerable.Range(0, count).Select(_ => new EntArena()).ToArray();

        Parallel.For(
            0,
            count,
            new ParallelOptions { MaxDegreeOfParallelism = count },
            index =>
            {
                var ents = new EntPtr[entitiesPerArena];
                for (var i = 0; i < ents.Length; i++)
                {
                    ents[i] = arenas[index].Alloc()
                        .Mutate()
                        .First(index)
                        .Second(i)
                        .Third("yy")
                        .Ent;
                }

                foreach (var ptr in ents)
                {
                    Assert.AreEqual(index, ptr.First);
                    Assert.AreEqual("yy", ptr.Third);
                    ptr.Dispose();
                }
            });

        foreach (var arena in arenas)
        {
            Assert.AreEqual(0, arena.Allocated);
            arena.Dispose();
        }
    }
}
