namespace AlvorKit.ECS.Test;

[Components]
public interface IEntArchRowComponents
{
    [Archetypal] int C0 { get; set; }
    [Archetypal] int C1 { get; set; }
    [Archetypal] int C2 { get; set; }
    [Archetypal] int C3 { get; set; }
    [Archetypal] int C4 { get; set; }
    [Archetypal] int C5 { get; set; }
    [Archetypal] int C6 { get; set; }
    [Archetypal] int C7 { get; set; }
    [Archetypal] string? RowTag { get; set; }
}

[TestClass]
public sealed class EntArchetypalRowTest
{
    /// <summary>An empty one-field row query stays empty and can be reused after a legal structural addition.</summary>
    [TestMethod]
    public void Rows_EmptyThenPopulated_ReusesQueryDescriptor()
    {
        using var arena = new EntArena();
        var query = arena.QueryArchetypal<EntArchRowComponents>().WithC0();

        int count = 0;
        foreach (var _ in query.Rows())
            count++;
        Assert.AreEqual(0, count);

        EntMut ent = arena.Alloc();
        ent.C0 = 7;
        foreach (var row in query.Rows())
        {
            Assert.AreEqual(ent, row.Ent);
            Assert.AreEqual(7, row.C0);
            count++;
        }

        Assert.AreEqual(1, count);
    }

    /// <summary>An eight-field row binds every selected column once and remains correct across matching archs.</summary>
    [TestMethod]
    public void Rows_EightFields_ReadsAndWritesAcrossMatchingArchs()
    {
        using var arena = new EntArena();
        EntMut plain = CreateWide(arena, 10);
        EntMut labelled = CreateWide(arena, 20);
        labelled.RowTag = "labelled";
        EntMut excluded = arena.Alloc();
        excluded.C0 = 30;

        var query = arena.QueryArchetypal<EntArchRowComponents>()
            .WithC0()
            .WithC1()
            .WithC2()
            .WithC3()
            .WithC4()
            .WithC5()
            .WithC6()
            .WithC7();

        int count = 0;
        int sum = 0;
        foreach (var row in query.Rows())
        {
            sum += row.C0;
            sum += row.C1;
            sum += row.C2;
            sum += row.C3;
            sum += row.C4;
            sum += row.C5;
            sum += row.C6;
            sum += row.C7;
            row.C0++;
            count++;
        }

        Assert.AreEqual(2, count);
        Assert.AreEqual((10 + 20) + 2 * (1 + 2 + 3 + 4 + 5 + 6 + 7), sum);
        Assert.AreEqual(11, plain.C0);
        Assert.AreEqual(21, labelled.C0);
        Assert.AreEqual(30, excluded.C0);
    }

    /// <summary>Completed row enumeration may be followed by compaction before the descriptor is enumerated again.</summary>
    [TestMethod]
    public void Rows_CompactionBetweenEnumerations_ObservesCurrentRows()
    {
        using var arena = new EntArena();
        EntMut first = CreateWide(arena, 10);
        EntMut removed = CreateWide(arena, 20);
        EntMut moved = CreateWide(arena, 30);
        Assert.AreEqual(60, SumEdges(arena));
        removed.UnsetC7();
        Assert.AreEqual(40, SumEdges(arena));
        Assert.AreEqual(10, first.C0);
        Assert.AreEqual(30, moved.C0);
    }

    /// <summary>Equivalent row queries remain isolated to their originating alloc.</summary>
    [TestMethod]
    public void Rows_MultipleAllocs_ReturnOnlyOwnedRows()
    {
        using var firstArena = new EntArena();
        using var secondArena = new EntArena();
        CreateWide(firstArena, 10);
        CreateWide(secondArena, 20);
        Assert.AreEqual(10, SumFirst(firstArena));
        Assert.AreEqual(20, SumFirst(secondArena));
    }

    private static EntMut CreateWide(EntArena arena, int first)
    {
        EntMut ent = arena.Alloc();
        ent.C0 = first;
        ent.C1 = 1;
        ent.C2 = 2;
        ent.C3 = 3;
        ent.C4 = 4;
        ent.C5 = 5;
        ent.C6 = 6;
        ent.C7 = 7;
        return ent;
    }

    private static int SumEdges(EntArena arena)
    {
        var query = arena.QueryArchetypal<EntArchRowComponents>()
            .WithC0()
            .WithC7();
        int sum = 0;
        foreach (var row in query.Rows())
            sum += row.C0;
        return sum;
    }

    private static int SumFirst(EntArena arena)
    {
        var query = arena.QueryArchetypal<EntArchRowComponents>().WithC0();
        int sum = 0;
        foreach (var row in query.Rows())
            sum += row.C0;
        return sum;
    }
}
