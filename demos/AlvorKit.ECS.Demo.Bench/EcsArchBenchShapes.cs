namespace AlvorKit.ECS.Demo.Bench;

/// <summary>Creates exact arch shapes outside timed point-operation loops.</summary>
internal static class EcsArchBenchShapes
{
    internal static void RegisterFields<A>(int width)
    {
        _ = EntArchColumn<int, F00, A>.FieldId;
        if (width == 1)
            return;
        _ = EntArchColumn<int, F01, A>.FieldId;
        _ = EntArchColumn<int, F02, A>.FieldId;
        _ = EntArchColumn<int, F03, A>.FieldId;
        if (width == 4)
            return;
        _ = EntArchColumn<int, F04, A>.FieldId;
        _ = EntArchColumn<int, F05, A>.FieldId;
        _ = EntArchColumn<int, F06, A>.FieldId;
        _ = EntArchColumn<int, F07, A>.FieldId;
        if (width == 8)
            return;
        _ = EntArchColumn<int, F08, A>.FieldId;
        _ = EntArchColumn<int, F09, A>.FieldId;
        _ = EntArchColumn<int, F10, A>.FieldId;
        _ = EntArchColumn<int, F11, A>.FieldId;
        _ = EntArchColumn<int, F12, A>.FieldId;
        _ = EntArchColumn<int, F13, A>.FieldId;
        _ = EntArchColumn<int, F14, A>.FieldId;
        _ = EntArchColumn<int, F15, A>.FieldId;
        if (width == 16)
            return;
        _ = EntArchColumn<int, F16, A>.FieldId;
        _ = EntArchColumn<int, F17, A>.FieldId;
        _ = EntArchColumn<int, F18, A>.FieldId;
        _ = EntArchColumn<int, F19, A>.FieldId;
        _ = EntArchColumn<int, F20, A>.FieldId;
        _ = EntArchColumn<int, F21, A>.FieldId;
        _ = EntArchColumn<int, F22, A>.FieldId;
        _ = EntArchColumn<int, F23, A>.FieldId;
        _ = EntArchColumn<int, F24, A>.FieldId;
        _ = EntArchColumn<int, F25, A>.FieldId;
        _ = EntArchColumn<int, F26, A>.FieldId;
        _ = EntArchColumn<int, F27, A>.FieldId;
        _ = EntArchColumn<int, F28, A>.FieldId;
        _ = EntArchColumn<int, F29, A>.FieldId;
        _ = EntArchColumn<int, F30, A>.FieldId;
        _ = EntArchColumn<int, F31, A>.FieldId;
    }

    internal static void SetWidth<A>(EntMut ent, int width)
    {
        ent.SetArchetypal<int, F00, A>(0);
        if (width == 1)
            return;

        ent.SetArchetypal<int, F01, A>(1);
        ent.SetArchetypal<int, F02, A>(2);
        ent.SetArchetypal<int, F03, A>(3);
        if (width == 4)
            return;

        ent.SetArchetypal<int, F04, A>(4);
        ent.SetArchetypal<int, F05, A>(5);
        ent.SetArchetypal<int, F06, A>(6);
        ent.SetArchetypal<int, F07, A>(7);
        if (width == 8)
            return;

        ent.SetArchetypal<int, F08, A>(8);
        ent.SetArchetypal<int, F09, A>(9);
        ent.SetArchetypal<int, F10, A>(10);
        ent.SetArchetypal<int, F11, A>(11);
        ent.SetArchetypal<int, F12, A>(12);
        ent.SetArchetypal<int, F13, A>(13);
        ent.SetArchetypal<int, F14, A>(14);
        ent.SetArchetypal<int, F15, A>(15);
        if (width == 16)
            return;

        ent.SetArchetypal<int, F16, A>(16);
        ent.SetArchetypal<int, F17, A>(17);
        ent.SetArchetypal<int, F18, A>(18);
        ent.SetArchetypal<int, F19, A>(19);
        ent.SetArchetypal<int, F20, A>(20);
        ent.SetArchetypal<int, F21, A>(21);
        ent.SetArchetypal<int, F22, A>(22);
        ent.SetArchetypal<int, F23, A>(23);
        ent.SetArchetypal<int, F24, A>(24);
        ent.SetArchetypal<int, F25, A>(25);
        ent.SetArchetypal<int, F26, A>(26);
        ent.SetArchetypal<int, F27, A>(27);
        ent.SetArchetypal<int, F28, A>(28);
        ent.SetArchetypal<int, F29, A>(29);
        ent.SetArchetypal<int, F30, A>(30);
        ent.SetArchetypal<int, F31, A>(31);
    }

    internal static void SetSevenFillers<A>(EntMut ent)
    {
        ent.SetArchetypal<int, F00, A>(0);
        ent.SetArchetypal<int, F01, A>(1);
        ent.SetArchetypal<int, F02, A>(2);
        ent.SetArchetypal<int, F03, A>(3);
        ent.SetArchetypal<int, F04, A>(4);
        ent.SetArchetypal<int, F05, A>(5);
        ent.SetArchetypal<int, F06, A>(6);
    }

    internal static void SetMask<A>(EntMut ent, uint mask)
    {
        while (mask != 0)
        {
            int field = BitOperations.TrailingZeroCount(mask);
            SetField<A>(ent, field);
            mask &= mask - 1;
        }
    }

    internal static void ToggleField<A>(EntMut ent, int field, bool set)
    {
        if (set)
            SetField<A>(ent, field);
        else
            UnsetField<A>(ent, field);
    }

    private static void SetField<A>(EntMut ent, int field)
    {
        switch (field)
        {
            case 0: ent.SetArchetypal<int, F00, A>(field); break;
            case 1: ent.SetArchetypal<int, F01, A>(field); break;
            case 2: ent.SetArchetypal<int, F02, A>(field); break;
            case 3: ent.SetArchetypal<int, F03, A>(field); break;
            case 4: ent.SetArchetypal<int, F04, A>(field); break;
            case 5: ent.SetArchetypal<int, F05, A>(field); break;
            case 6: ent.SetArchetypal<int, F06, A>(field); break;
            case 7: ent.SetArchetypal<int, F07, A>(field); break;
            case 8: ent.SetArchetypal<int, F08, A>(field); break;
            case 9: ent.SetArchetypal<int, F09, A>(field); break;
            case 10: ent.SetArchetypal<int, F10, A>(field); break;
            case 11: ent.SetArchetypal<int, F11, A>(field); break;
            case 12: ent.SetArchetypal<int, F12, A>(field); break;
            case 13: ent.SetArchetypal<int, F13, A>(field); break;
            case 14: ent.SetArchetypal<int, F14, A>(field); break;
            case 15: ent.SetArchetypal<int, F15, A>(field); break;
            case 16: ent.SetArchetypal<int, F16, A>(field); break;
            case 17: ent.SetArchetypal<int, F17, A>(field); break;
            case 18: ent.SetArchetypal<int, F18, A>(field); break;
            case 19: ent.SetArchetypal<int, F19, A>(field); break;
            case 20: ent.SetArchetypal<int, F20, A>(field); break;
            case 21: ent.SetArchetypal<int, F21, A>(field); break;
            case 22: ent.SetArchetypal<int, F22, A>(field); break;
            case 23: ent.SetArchetypal<int, F23, A>(field); break;
            case 24: ent.SetArchetypal<int, F24, A>(field); break;
            case 25: ent.SetArchetypal<int, F25, A>(field); break;
            case 26: ent.SetArchetypal<int, F26, A>(field); break;
            case 27: ent.SetArchetypal<int, F27, A>(field); break;
            case 28: ent.SetArchetypal<int, F28, A>(field); break;
            case 29: ent.SetArchetypal<int, F29, A>(field); break;
            case 30: ent.SetArchetypal<int, F30, A>(field); break;
            case 31: ent.SetArchetypal<int, F31, A>(field); break;
        }
    }

    private static void UnsetField<A>(EntMut ent, int field)
    {
        switch (field)
        {
            case 0: ent.UnsetArchetypal<int, F00, A>(); break;
            case 1: ent.UnsetArchetypal<int, F01, A>(); break;
            case 2: ent.UnsetArchetypal<int, F02, A>(); break;
            case 3: ent.UnsetArchetypal<int, F03, A>(); break;
            case 4: ent.UnsetArchetypal<int, F04, A>(); break;
            case 5: ent.UnsetArchetypal<int, F05, A>(); break;
            case 6: ent.UnsetArchetypal<int, F06, A>(); break;
            case 7: ent.UnsetArchetypal<int, F07, A>(); break;
            case 8: ent.UnsetArchetypal<int, F08, A>(); break;
            case 9: ent.UnsetArchetypal<int, F09, A>(); break;
            case 10: ent.UnsetArchetypal<int, F10, A>(); break;
            case 11: ent.UnsetArchetypal<int, F11, A>(); break;
            case 12: ent.UnsetArchetypal<int, F12, A>(); break;
            case 13: ent.UnsetArchetypal<int, F13, A>(); break;
            case 14: ent.UnsetArchetypal<int, F14, A>(); break;
            case 15: ent.UnsetArchetypal<int, F15, A>(); break;
            case 16: ent.UnsetArchetypal<int, F16, A>(); break;
            case 17: ent.UnsetArchetypal<int, F17, A>(); break;
            case 18: ent.UnsetArchetypal<int, F18, A>(); break;
            case 19: ent.UnsetArchetypal<int, F19, A>(); break;
            case 20: ent.UnsetArchetypal<int, F20, A>(); break;
            case 21: ent.UnsetArchetypal<int, F21, A>(); break;
            case 22: ent.UnsetArchetypal<int, F22, A>(); break;
            case 23: ent.UnsetArchetypal<int, F23, A>(); break;
            case 24: ent.UnsetArchetypal<int, F24, A>(); break;
            case 25: ent.UnsetArchetypal<int, F25, A>(); break;
            case 26: ent.UnsetArchetypal<int, F26, A>(); break;
            case 27: ent.UnsetArchetypal<int, F27, A>(); break;
            case 28: ent.UnsetArchetypal<int, F28, A>(); break;
            case 29: ent.UnsetArchetypal<int, F29, A>(); break;
            case 30: ent.UnsetArchetypal<int, F30, A>(); break;
            case 31: ent.UnsetArchetypal<int, F31, A>(); break;
        }
    }
}
