namespace AlvorKit.UI.Blend.Demo;

/// <summary>Builds the stripped Blender-like editor shell from the current HTML reference.</summary>
[App]
public class EditorShellMenu(EditorShellStyle style)
{
    public void Create(EntMut root)
    {
        const float leftDockWidth = 264f;
        const float splitterWidth = 6f;
        const float rightDockWidth = 330f;
        const float bottomDockHeight = 233f;
        const float bottomDockTopInset = 2f;
        const float activeTabAccentOffset = 1.5f;
        const float centerLeft = leftDockWidth + splitterWidth;
        const float centerWidthOffset = -(leftDockWidth + splitterWidth + splitterWidth + rightDockWidth);

        var palette = style.Palette;
        var metrics = style.Metrics;

        root.Mutate(style.Board)
            .ColorV(palette.AppBackground);

        Node(root, out var menuBar).Mutate(style.MenuBar);
        {
            Node(menuBar, out var brand)
                .SizeRelativeV((0, 1))
                .SizeV((176f, 0))
                .ColorV(palette.Panel)
                .Mutate(node => style.RightRule(node, () => palette.Border));
            {
                Node(brand)
                    .OffsetV((10f, 5.5f))
                    .SizeRelativeV((0, 0))
                    .SizeV((13f, 13f))
                    .ColorV(palette.ActiveSurface)
                    .Mutate(node => style.Border(node, () => palette.Accent));

                StrongTextAt(brand, (24f, 0), (142f, metrics.MenuBarHeight), "AlvorKit Studio", palette.Text, 12);
            }

            MenuItem(menuBar, (184f, 0), (34f, metrics.MenuBarHeight), "File");
            MenuItem(menuBar, (224f, 0), (36f, metrics.MenuBarHeight), "Edit");
            MenuItem(menuBar, (268f, 0), (44f, metrics.MenuBarHeight), "View");
            MenuItem(menuBar, (318f, 0), (46f, metrics.MenuBarHeight), "Tools");
            MenuItem(menuBar, (370f, 0), (48f, metrics.MenuBarHeight), "Build");
            RightText(menuBar, (-8f, 0), 292f, "Layout: Studio    Scene: sandbox");
            style.BottomRule(menuBar, () => palette.Border);
        }

        Node(root, out var toolbar)
            .OffsetV((0, metrics.MenuBarHeight))
            .Mutate(style.Toolbar);
        {
            ButtonAt(toolbar, (6f, 4f), (26f, metrics.ButtonHeight), "M", true, 10, palette.Raised);
            ButtonAt(toolbar, (36f, 4f), (26f, metrics.ButtonHeight), "R", false, 10, palette.Raised);
            ButtonAt(toolbar, (66f, 4f), (26f, metrics.ButtonHeight), "S", false, 10, palette.Raised);
            VerticalRule(toolbar, 100f, 4f, 26f);
            ButtonAt(toolbar, (110f, 4f), (79f, metrics.ButtonHeight), "Select Tool", false, 12, palette.Raised);
            ButtonAt(toolbar, (194f, 4f), (84f, metrics.ButtonHeight), "Snap 0.25m", false, 12, palette.Raised);
            RightButton(toolbar, (-83f, 4f), (72f, metrics.ButtonHeight), "Play", false, 12, palette.Raised);
            RightButton(toolbar, (-6f, 4f), (72f, metrics.ButtonHeight), "Build", false, 12, palette.Raised);
        }

        Node(root, out var workspace).Mutate(style.Board);
        workspace.Mutate()
            .OffsetV((0, metrics.MenuBarHeight + metrics.ToolbarHeight))
            .SizeV((0, -(metrics.MenuBarHeight + metrics.ToolbarHeight + metrics.StatusBarHeight)));
        {
            Node(workspace, out var leftDock)
                .SizeRelativeV((0, 1))
                .SizeV((leftDockWidth, 0))
                .Mutate(PanelSurface);
            {
                PanelTitle(leftDock, "Scene", "Outliner");
            }

            Splitter(workspace, (leftDockWidth, 0), splitterWidth);

            Node(workspace, out var center)
                .OffsetV((centerLeft, 0))
                .SizeRelativeV((1, 1))
                .SizeV((centerWidthOffset, 0))
                .Mutate(style.Board);
            {
                Node(center, out var viewportPanel)
                    .SizeRelativeV((1, 1))
                    .SizeV((0, -bottomDockHeight))
                    .ColorV(palette.Panel);
                {
                    ViewportHeader(viewportPanel);

                    Node(viewportPanel, out var viewport)
                        .OffsetV((0, metrics.ViewportHeaderHeight))
                        .SizeRelativeV((1, 1))
                        .SizeV((0, -metrics.ViewportHeaderHeight))
                        .ColorV(palette.AppBackground);
                    {
                        Node(viewport)
                            .AlignmentV(Alignment.Bottom | Alignment.Right)
                            .OffsetV((-14f, -14f))
                            .SizeRelativeV((0, 0))
                            .SizeV((74f, 74f))
                            .ColorV(palette.WithAlpha(palette.Panel, 0.88f))
                            .Mutate(node => style.Border(node, () => palette.Border));
                    }
                }

                Node(center, out var bottomDock)
                    .AlignmentV(Alignment.Bottom | Alignment.Left)
                    .SizeRelativeV((1, 0))
                    .SizeV((0, bottomDockHeight))
                    .ColorV(palette.Panel)
                    .Mutate(node => style.TopRule(node, () => palette.Border))
                    .Mutate(BottomDock);

                style.BottomRule(center, () => palette.Border);
            }

            Splitter(workspace, (-rightDockWidth, 0), splitterWidth, Alignment.Top | Alignment.Right);

            Node(workspace, out var rightDock)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .SizeRelativeV((0, 1))
                .SizeV((rightDockWidth, 0))
                .Mutate(PanelSurface);
            {
                PanelTitle(rightDock, "Inspector", "Properties");
            }
        }

        Node(root, out var statusBar)
            .AlignmentV(Alignment.Bottom | Alignment.Left)
            .Mutate(style.StatusBar);
        {
            TextAt(statusBar, (4f, 0), (286f, metrics.StatusBarHeight), "C:/Projects/AlvorKit/stress-scenes/sandbox", palette.MutedText, 11);
            TextAt(statusBar, (288f, 0), (140f, metrics.StatusBarHeight), "Selected: none", palette.MutedText, 11);
            RightText(statusBar, (-8f, 0), 294f, "Memory 418 MB    Frame 6.9 ms    Ready");
        }

        void PanelSurface(EntMut panel)
        {
            panel.Mutate()
                .ColorV(palette.Panel)
                .Mutate(node => style.BottomRule(node, () => palette.Border));
        }

        void ViewportHeader(EntMut panel)
        {
            Node(panel, out var header)
                .SizeRelativeV((1, 0))
                .SizeV((0, metrics.ViewportHeaderHeight))
                .ColorV(palette.Raised)
                .Mutate(node => style.BottomRule(node, () => palette.Border));
            {
                ChipAt(header, (7f, 2.5f), (72.5f, metrics.ChipHeight), "Perspective", palette.Raised);
                ChipAt(header, (84.5f, 2.5f), (30f, metrics.ChipHeight), "Lit", palette.Raised);
                ChipAt(header, (119.5f, 2.5f), (53.5f, metrics.ChipHeight), "Gizmos", palette.Raised);
                ChipAt(header, (178f, 2.5f), (75f, metrics.ChipHeight), "Snap 0.25m", palette.Raised);
                RightChip(header, (-69f, 2.5f), (70f, metrics.ChipHeight), "Zoom 83%", palette.Raised);
                RightChip(header, (-7f, 2.5f), (57f, metrics.ChipHeight), "Grid 1m", palette.Raised);
            }
        }

        void BottomDock(EntMut panel)
        {
            Node(panel, out var tabs)
                .OffsetV((0, bottomDockTopInset))
                .SizeRelativeV((1, 0))
                .SizeV((0, metrics.TabStripHeight))
                .ColorV(palette.Raised)
                .Mutate(node => style.BottomRule(node, () => palette.Border));
            {
                TabAt(tabs, 0f, 88f, "Assets", true);
                TabAt(tabs, 88f, 88f, "Console", false);
                TabAt(tabs, 176f, 88f, "Profiler", false);
                TabAt(tabs, 264f, 88f, "Timeline", false);

                Node(tabs)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Bottom | Alignment.Left)
                    .OffsetV((88f, 0))
                    .SizeRelativeV((1, 0))
                    .SizeV((-88f, metrics.Hairline))
                    .ColorV(palette.Border);

                Node(tabs)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Bottom | Alignment.Left)
                    .OffsetV((0, style.PhysicalPixels(1) / 2f))
                    .SizeAlignmentSnapV(0f)
                    .SizeRelativeV((0, 0))
                    .SizeV((88f, style.PhysicalPixels(1)))
                    .ColorV(palette.Border);
            }

            Node(panel)
                .OffsetV((0, bottomDockTopInset + metrics.TabStripHeight))
                .SizeRelativeV((0, 1))
                .SizeV((190f, -bottomDockTopInset - metrics.TabStripHeight))
                .ColorV(palette.WithAlpha(palette.Panel, 0.86f))
                .Mutate(node => style.RightRule(node, () => palette.Border));

            Node(panel, out var assets)
                .OffsetV((190f, bottomDockTopInset + metrics.TabStripHeight))
                .SizeRelativeV((1, 1))
                .SizeV((-190f, -bottomDockTopInset - metrics.TabStripHeight))
                .ColorV(palette.Panel)
                .Mutate(style.Board);
            {
                Node(assets, out var assetToolbar)
                    .SizeRelativeV((1, 0))
                    .SizeV((0, metrics.AssetToolbarHeight))
                    .ColorV(palette.Panel)
                    .Mutate(node => style.BottomRule(node, () => palette.Border));
                {
                    RightButton(assetToolbar, (-69f, 2f), (72f, metrics.ButtonHeight), "New", false, 12, palette.Panel);
                    RightButton(assetToolbar, (-38f, 2f), (26f, metrics.ButtonHeight), "G", true, 10, palette.Panel);
                    RightButton(assetToolbar, (-7f, 2f), (26f, metrics.ButtonHeight), "L", false, 10, palette.Panel);
                }
            }

            Node(panel)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .SizeRelativeV((1, 0))
                .SizeV((0, bottomDockTopInset))
                .ColorV(palette.Border);

            Node(panel)
                .IsFloatingV(true)
                .AlignmentV(Alignment.Top | Alignment.Left)
                .OffsetV((0, activeTabAccentOffset))
                .SizeRelativeV((0, 0))
                .SizeV((88f, 2f))
                .ColorV(palette.Accent);
        }

        void PanelTitle(EntMut parent, string left, string right)
        {
            Node(parent, out var title).Mutate(style.PanelTitle);
            {
                StrongTextAt(title, (2f, 0), (150f, metrics.PanelTitleHeight), left, palette.Text, 12);
                RightText(title, (-2f, 0), 136f, right);
            }
        }

        void MenuItem(EntMut parent, Vec2 offset, Vec2 size, string text)
        {
            Node(parent)
                .OffsetV(offset)
                .Mutate(node => style.MenuItem(node, size))
                .TextV(text);
        }

        void TextAt(EntMut parent, Vec2 offset, Vec2 size, string text, Vec4 color, int fontSize)
        {
            Node(parent)
                .Mutate(style.Text)
                .OffsetV(offset)
                .SizeRelativeV((0, 0))
                .SizeV(size)
                .FontSizeV(fontSize)
                .TextColorV(color)
                .TextPaddingV((6f, 0, 6f, 0))
                .TextV(text);
        }

        void StrongTextAt(EntMut parent, Vec2 offset, Vec2 size, string text, Vec4 color, int fontSize)
        {
            Node(parent)
                .Mutate(style.EmphasisText)
                .OffsetV(offset)
                .SizeRelativeV((0, 0))
                .SizeV(size)
                .FontSizeV(fontSize)
                .TextColorV(color)
                .TextPaddingV((6f, 0, 6f, 0))
                .TextV(text);
        }

        void RightText(EntMut parent, Vec2 offset, float width, string text)
        {
            Node(parent)
                .Mutate(style.MutedText)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .OffsetV(offset)
                .SizeRelativeV((0, 1))
                .SizeV((width, 0))
                .TextAlignmentV(Alignment.Right | Alignment.Vertical)
                .TextPaddingV((0, 0, 8f, 0))
                .TextV(text);
        }

        void ButtonAt(EntMut parent, Vec2 offset, Vec2 size, string text, bool active, int fontSize, Vec4 outside)
        {
            Node(parent)
                .OffsetV(offset)
                .Mutate(node => style.Button(node, size, text, fontSize, () => active, () => outside));
        }

        void RightButton(EntMut parent, Vec2 offset, Vec2 size, string text, bool active, int fontSize, Vec4 outside)
        {
            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .OffsetV(offset)
                .Mutate(node => style.Button(node, size, text, fontSize, () => active, () => outside));
        }

        void ChipAt(EntMut parent, Vec2 offset, Vec2 size, string text, Vec4 outside)
        {
            Node(parent)
                .OffsetV(offset)
                .Mutate(node => style.Chip(node, size, text, () => outside));
        }

        void RightChip(EntMut parent, Vec2 offset, Vec2 size, string text, Vec4 outside)
        {
            Node(parent)
                .AlignmentV(Alignment.Top | Alignment.Right)
                .OffsetV(offset)
                .Mutate(node => style.Chip(node, size, text, () => outside));
        }

        void TabAt(EntMut parent, float x, float width, string text, bool active)
        {
            Node(parent, out var tab)
                .OffsetV((x, 0))
                .Mutate(node => style.Tab(node, (width, metrics.TabStripHeight), active))
                .TextV(text);

            _ = tab;
        }

        void Splitter(EntMut parent, Vec2 offset, float width, Alignment alignment = Alignment.Top | Alignment.Left)
        {
            Node(parent)
                .AlignmentV(alignment)
                .OffsetV(offset)
                .SizeRelativeV((0, 1))
                .SizeV((width, 0))
                .ColorV(palette.AppBackground)
                .Mutate(node => style.LeftRule(node, () => palette.Border))
                .Mutate(node => style.RightRule(node, () => palette.Border));
        }

        void VerticalRule(EntMut parent, float x, float y, float height) =>
            Node(parent)
                .OffsetV((x, y))
                .SizeRelativeV((0, 0))
                .SizeV((1f, height))
                .ColorV(palette.Border);
    }
}
