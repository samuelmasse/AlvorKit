namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Shared visual language for the azure tentacle demo UI.</summary>
[App]
public class AppStyle(RootRoboto roboto, RootKeyboard keyboard)
{
    public Vec4 SceneClearColor => (0.004f, 0.006f, 0.009f, 1f);
    public Vec4 SidebarColor => (0.145f, 0.145f, 0.145f, 1f);
    public Vec4 PanelColor => (0.18f, 0.18f, 0.18f, 1f);
    public Vec4 PanelRaisedColor => (0.235f, 0.235f, 0.235f, 1f);
    public Vec4 TextColor => (0.82f, 0.82f, 0.82f, 1f);
    public Vec4 MutedTextColor => (0.58f, 0.61f, 0.62f, 1f);
    public Vec4 AccentColor => (0.46f, 0.68f, 0.78f, 1f);
    public Vec4 WarmAccentColor => (0.97f, 0.66f, 0.28f, 1f);

    public float SidebarWidth => 300f;
    public float SidebarPadding => 8f;
    public float PanelPadding => 7f;
    public float SpacingS => 3f;
    public float Spacing => 6f;
    public float ButtonHeight => 20f;
    public float RowHeight => 19f;

    public int FontSizeSmall => 8;
    public int FontSizeBody => 9;
    public int FontSizeHeader => 10;
    public int FontSizeTitle => 11;

    public void Root(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1))
        .InnerLayoutV(InnerLayout.HorizontalList)
        .InnerSizingV(InnerSizing.HorizontalWeight)
        .InnerAlignmentSnapV(1f);

    public void SceneArea(EntMut ent) => ent.Mutate()
        .SizeRelativeV((1, 1));

    public void Sidebar(EntMut ent) => ent.Mutate()
        .ColorV(SidebarColor)
        .PaddingV((SidebarPadding, SidebarPadding, SidebarPadding, SidebarPadding))
        .SizeWeightTypeV(SizeWeightType.Self)
        .SizeRelativeV((0, 1))
        .SizeV((SidebarWidth, 0))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(SpacingS)
        .IsSelectableV(true);

    public void Panel(EntMut ent) => ent.Mutate()
        .ColorV(PanelColor)
        .PaddingV((PanelPadding, PanelPadding, PanelPadding, PanelPadding))
        .SizeRelativeV((1, 0))
        .SizeInnerSumRelativeV((0, 1))
        .InnerLayoutV(InnerLayout.VerticalList)
        .InnerSpacingV(SpacingS)
        .IsSelectableV(true);

    public void Text(EntMut ent) => ent.Mutate()
        .FontV(roboto.Font)
        .FontSizeV(FontSizeBody)
        .TextColorV(TextColor)
        .TextAlignmentV(Alignment.Left | Alignment.Vertical)
        .TextAlignmentSnapV(1f);

    public void Label(EntMut ent) => ent.Mutate()
        .Mutate(Text)
        .SizeRelativeV((0, 0))
        .SizeTextRelativeV((1, 1));

    public void MutedLabel(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeSmall)
        .TextColorV(MutedTextColor);

    public void Heading(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeHeader)
        .TextColorV(AccentColor);

    public void Title(EntMut ent) => ent.Mutate()
        .Mutate(Label)
        .FontSizeV(FontSizeTitle)
        .TextColorV(TextColor);

    public void Button(EntMut ent)
    {
        var enterWasDown = false;
        ent.Mutate()
            .Mutate(Text)
            .SizeRelativeV((0, 0))
            .SizeV((58f, ButtonHeight))
            .TextAlignmentV(Alignment.Center)
            .TextPaddingV((SpacingS, 0, SpacingS, 0))
            .IsSelectableV(true)
            .IsFocusableV(true)
            .CursorF(() => CursorShape.Hand)
            .ColorF(() => ButtonFill(ent))
            .OnUpdateF(() =>
            {
                var enterDown = keyboard.IsKeyDown(Keys.Enter);
                if (ent.IsFocusedR && enterDown && !enterWasDown)
                    ent.OnPressFV.Resolve()?.Invoke();

                enterWasDown = enterDown;
            });
    }

    public void AnimationRow(EntMut ent, Func<bool> selected) => ent.Mutate()
        .SizeRelativeV((1, 0))
        .SizeV((0, RowHeight))
        .ColorF(() => AnimationRowFill(ent, selected()))
        .IsSelectableV(true)
        .IsFocusableV(true)
        .CursorF(() => CursorShape.Hand);

    public Vec4 AnimationTextColor(bool selected) => selected ? WarmAccentColor : MutedTextColor;

    private Vec4 ButtonFill(EntMut ent)
    {
        if (ent.IsPressedR)
            return (0.18f, 0.29f, 0.34f, 1f);

        if (ent.IsFocusedR || ent.IsHoveredR)
            return (0.28f, 0.28f, 0.28f, 1f);

        return PanelRaisedColor;
    }

    private Vec4 AnimationRowFill(EntMut ent, bool selected)
    {
        if (selected)
            return (0.32f, 0.24f, 0.11f, 1f);

        if (ent.IsFocusedR || ent.IsHoveredR)
            return (0.235f, 0.235f, 0.235f, 1f);

        return default;
    }
}
