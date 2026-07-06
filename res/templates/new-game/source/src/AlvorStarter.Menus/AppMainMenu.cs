namespace AlvorStarter.Menus;

/// <summary>Builds the starter menu with a click counter.</summary>
[App]
public class AppMainMenu(RootText text, AppCounter counter, AppStyle s)
{
    /// <summary>Creates the starter menu under the caller-owned root.</summary>
    public void Create(EntMut root)
    {
        const float panelWidth = 420f;
        const float panelHeight = 220f;

        Node(root, out var layer)
            .Mutate(s.Board);
        {
            Node(layer, out var dock)
                .Mutate(s.Board)
                .AlignmentV(Alignment.Right | Alignment.Vertical)
                .SizeRelativeV((0.45f, 1f));
            {
                Node(dock, out var panel)
                    .Mutate(s.ModalPanel)
                    .AlignmentV(Alignment.Left | Alignment.Vertical)
                    .OffsetV((48, 0))
                    .SizeV((panelWidth, panelHeight));
                {
                    Node(panel, out var title)
                        .Mutate(s.PanelTitle);
                    {
                        Node(title)
                            .Mutate(s.EmphasisLabel)
                            .AlignmentV(Alignment.Left | Alignment.Vertical)
                            .OffsetV((12, 0))
                            .TextV("Alvor Starter");
                    }

                    Node(panel, out var body)
                        .Mutate(s.ModalContent)
                        .InnerSpacingV(s.Metrics.LooseSpacing);
                    {
                        Node(body)
                            .Mutate(s.Label)
                            .TextV("A tiny AlvorKit game scaffold.");

                        Node(body)
                            .Mutate(s.MutedLabel)
                            .TextV("Raw GL, RootSprites, and Blend UI on one path.");

                        Node(body)
                            .Mutate(s.MutedLabel)
                            .TextF(() => text.Format("Clicks: {0}", counter.Value));

                        Node(body, out var buttons)
                            .Mutate(s.HorizontalList)
                            .InnerSpacingV(s.Metrics.LooseSpacing);
                        {
                            Node(buttons)
                                .Mutate(s.Button)
                                .TextV("Count +1")
                                .OnClickF(counter.Increment);

                            Node(buttons)
                                .Mutate(s.Button)
                                .TextV("Reset")
                                .OnClickF(counter.Reset);
                        }
                    }
                }
            }
        }
    }
}
