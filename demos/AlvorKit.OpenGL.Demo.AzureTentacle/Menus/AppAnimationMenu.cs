namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppAnimationMenu(
    AppStyle style,
    AppSession session)
{
    public void Create(EntMut root)
    {
        root.Mutate(style.Panel);

        Node(root, out var header)
            .InnerLayoutV(InnerLayout.HorizontalList)
            .InnerSizingV(InnerSizing.HorizontalWeight)
            .InnerSpacingV(style.SpacingS)
            .SizeRelativeV((1, 0))
            .SizeV((0, style.ButtonHeight));
        {
            Node(header)
                .Mutate(style.Heading)
                .TextV("Animations")
                .SizeRelativeV((1, 0));

            Button(header, 54f, "Prev", session.SelectPreviousAnimation);
            Button(header, 54f, "Next", session.SelectNextAnimation);
        }

        for (var index = 0; index < session.AnimationLineCount; index++)
        {
            var animationIndex = index;
            Node(root, out var row)
                .Mutate(node => style.AnimationRow(node, () => animationIndex == session.SelectedAnimationIndex))
                .OnPressF(() => session.SelectAnimation(animationIndex));
            {
                Node(row)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                    .SizeRelativeV((0, 1))
                    .SizeV((3f, 0))
                    .ColorF(() => animationIndex == session.SelectedAnimationIndex ? style.WarmAccentColor : default);

                Node(row)
                    .Mutate(style.Label)
                    .TextF(() => session.AnimationLineAt(animationIndex))
                    .TextColorF(() => style.AnimationTextColor(animationIndex == session.SelectedAnimationIndex))
                    .OffsetV((style.Spacing, 0))
                    .AlignmentV(Alignment.Left | Alignment.Vertical);
            }
        }

        void Button(EntMut parent, float width, string label, Action action)
        {
            Node(parent)
                .Mutate(style.Button)
                .SizeV((width, style.ButtonHeight))
                .TextV(label)
                .OnPressF(action);
        }
    }
}
