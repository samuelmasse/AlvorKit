namespace AlvorKit.ECS.Test;

[TestClass]
public class EntArenaConcurrencyTest
{
    /// <summary>Verifies EntArena HeavyThreads.</summary>
    [TestMethod]
    public void EntArena_HeavyThreads()
    {
        var rng = new Random(353);

        var plans = new List<List<bool>>();
        var arenas = new List<EntArena>();
        var rngs = new List<Random>();

        int count = 43;
        for (int i = 0; i < count; i++)
        {
            var plan = new List<bool>();
            int steps = rng.Next(10000);
            int miss = 0;

            for (int j = 0; j < steps; j++)
            {
                if (miss <= 0)
                {
                    plan.Add(true);
                    miss++;
                }
                else
                {
                    var val = rng.Next(3) == 0;
                    miss += val ? 1 : -1;
                    plan.Add(val);
                }
            }

            while (miss > 0)
            {
                plan.Add(false);
                miss--;
            }

            plans.Add(plan);
            rngs.Add(new(i));
            arenas.Add(new());
        }

        var threads = new List<Thread>();

        for (int i = 0; i < count; i++)
        {
            int index = i;
            var thread = new Thread(() =>
            {
                var set = new HashSet<EntPtr>();

                foreach (var p in plans[index])
                {
                    if (p)
                    {
                        var ptr = arenas[index].Alloc()
                            .Mutate()
                            .First(index)
                            .Second(353)
                            .Third("yy")
                            .Ent;

                        set.Add(ptr);
                    }
                    else
                    {
                        var ptr = set.First();
                        set.Remove(ptr);

                        Assert.AreEqual(index, ptr.First);
                        Assert.AreEqual(353, ptr.Second);
                        Assert.AreEqual("yy", ptr.Third);

                        ptr.Dispose();
                    }
                }
            });
            thread.Start();
            threads.Add(thread);
        }

        foreach (var thread in threads)
            thread.Join();

        foreach (var arena in arenas)
            Assert.AreEqual(0, arena.Allocated);
    }
}
