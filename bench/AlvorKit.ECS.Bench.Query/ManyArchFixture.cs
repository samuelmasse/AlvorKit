namespace AlvorKit;

internal static class ManyArchFixture
{
    private const int FieldCount = 11;
    private const int ArchCount = (1 << FieldCount) - 1;
    internal static void MaterializeArchs(EntMut ent)
    {
        int previousMask = 0;

        for (int index = 1; index <= ArchCount; index++)
        {
            int mask = index ^ (index >> 1);
            int changed = previousMask ^ mask;
            int bit = BitOperations.TrailingZeroCount((uint)changed);
            SetBit(ent, bit, (mask & changed) != 0);
            previousMask = mask;
        }

        for (int bit = 0; bit < FieldCount; bit++)
        {
            if ((previousMask & (1 << bit)) != 0)
                SetBit(ent, bit, false);
        }
    }

    internal static void EnterMeasuredArch(EntMut ent)
    {
        ent.SetArchetypal<int, C0, ManyArch>(1);
        ent.SetArchetypal<int, C1, ManyArch>(1);
        ent.SetArchetypal<int, C2, ManyArch>(1);
        ent.SetArchetypal<int, C3, ManyArch>(1);
    }

    private static void SetBit(EntMut ent, int bit, bool value)
    {
        switch (bit)
        {
            case 0:
                Set<C0>(ent, value);
                break;
            case 1:
                Set<C1>(ent, value);
                break;
            case 2:
                Set<C2>(ent, value);
                break;
            case 3:
                Set<C3>(ent, value);
                break;
            case 4:
                Set<C4>(ent, value);
                break;
            case 5:
                Set<C5>(ent, value);
                break;
            case 6:
                Set<C6>(ent, value);
                break;
            case 7:
                Set<C7>(ent, value);
                break;
            case 8:
                Set<C8>(ent, value);
                break;
            case 9:
                Set<C9>(ent, value);
                break;
            case 10:
                Set<C10>(ent, value);
                break;
        }
    }

    private static void Set<N>(EntMut ent, bool value)
    {
        if (value)
            ent.SetArchetypal<int, N, ManyArch>(0);
        else
            ent.UnsetArchetypal<int, N, ManyArch>();
    }

    internal readonly record struct ManyArch;
    internal readonly record struct C0;
    internal readonly record struct C1;
    internal readonly record struct C2;
    internal readonly record struct C3;
    internal readonly record struct C4;
    internal readonly record struct C5;
    internal readonly record struct C6;
    internal readonly record struct C7;
    internal readonly record struct C8;
    internal readonly record struct C9;
    internal readonly record struct C10;
}
