namespace AlvorKit;

[TestClass]
public class EntIdxChangeViewTest
{
    /// <summary>Large change values remain stable across nested unrelated mutation and a moving collection.</summary>
    [TestMethod]
    public void ChangeView_PreservesBeforeAndCommittedAfter()
    {
        using var context = new EntIdxContext();
        var copies = new List<(EntIdxWideValue Before, EntIdxWideValue After, bool WasPresent, bool IsPresent)>();
        context.OnChange<EntIdxWideValue, EntIdxWideComponent>((ent, in change) =>
        {
            var before = change.Before;
            var after = change.After;
            ent.Name = "nested";
            Assert.ThrowsExactly<InvalidOperationException>(() => ent.Unset<EntIdxWideValue, EntIdxWideComponent>());
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            Assert.AreEqual(before, change.Before);
            Assert.AreEqual(after, change.After);
            copies.Add((change.Before, change.After, change.WasPresent, change.IsPresent));
        });
        using var arena = new EntIdxArena(context);
        var ent = arena.Alloc();
        EntIdxWideValue first = new(1, 2, 3, 4, 5, 6, 7, 8);
        EntIdxWideValue second = first with { H = 9 };
        ent.Set<EntIdxWideValue, EntIdxWideComponent>(first);
        ent.Set<EntIdxWideValue, EntIdxWideComponent>(second);
        ent.Set<EntIdxWideValue, EntIdxWideComponent>(second);
        ent.Unset<EntIdxWideValue, EntIdxWideComponent>();
        CollectionAssert.AreEqual(new[]
        {
            (default(EntIdxWideValue), first, false, true),
            (first, second, true, true),
            (second, default(EntIdxWideValue), true, false),
        }, copies);
    }
}
