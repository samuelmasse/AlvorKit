namespace AlvorStarter;

/// <summary>Builds the starter menu with a click counter.</summary>
[App]
public class AppMainMenu(RootText text, BlendUi bl, AppCounter counter)
{
    /// <summary>Creates the starter menu under the caller-owned root.</summary>
    public void Create(EntMut root)
    {
        const float panelWidth = 420f;
        const float panelHeight = 220f;

        Node(root, out var layer)
            .Mutate(bl.S.Board);
        {
            Node(layer, out var dock)
                .Mutate(bl.S.Board)
                .AlignmentV(Alignment.Right | Alignment.Vertical)
                .SizeRelativeV((0.45f, 1f));
            {
                Node(dock, out var panel)
                    .Mutate(bl.S.ModalPanel)
                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                    .OffsetV((48, 0))
                    .SizeV((panelWidth, panelHeight));
                {
                    Node(panel, out var title)
                        .Mutate(bl.S.PanelTitle);
                    {
                        Node(title)
                            .Mutate(bl.S.EmphasisLabel)
                            .AlignmentV(Alignment.Left | Alignment.Vertical)
                            .OffsetV((12, 0))
                            .TextV("Alvor Starter");
                    }

                    Node(panel, out var body)
                        .Mutate(bl.S.ModalContent)
                        .InnerSpacingV(bl.S.Metrics.LooseSpacing);
                    {
                        Node(body)
                            .Mutate(bl.S.Label)
                            .TextV("A tiny AlvorKit game scaffold.");

                        Node(body)
                            .Mutate(bl.S.MutedLabel)
                            .TextV("Raw GL, RootSprites, and Blend UI on one path.");

                        Node(body)
                            .Mutate(bl.S.MutedLabel)
                            .TextF(() => text.Format("Clicks: {0}", counter.Value));

                        Node(body, out var buttons)
                            .Mutate(bl.S.HorizontalList)
                            .InnerSpacingV(bl.S.Metrics.LooseSpacing);
                        {
                            Node(buttons)
                                .Mutate(bl.S.Button)
                                .TextV("Count +1")
                                .OnClickF(counter.Increment);

                            Node(buttons)
                                .Mutate(bl.S.Button)
                                .TextV("Reset")
                                .OnClickF(counter.Reset);
                        }
                    }
                }
            }
        }
    }
}
