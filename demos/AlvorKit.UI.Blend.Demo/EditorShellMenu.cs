namespace AlvorKit.UI.Blend.Demo;

/// <summary>Builds the stripped Blender-like editor shell demo using layout primitives.</summary>
[App]
public class EditorShellMenu(EditorShellStyle s, EditorShellLayout layout)
{
    public void Create(EntMut root)
    {
        var palette = s.Palette;
        var metrics = s.Metrics;

        Node(root, out var shell)
            .Mutate(s.Root);
        {
            MenuBar(shell);
            Toolbar(shell);
            Workspace(shell);
            StatusBar(shell);
        }

        void MenuBar(EntMut parent)
        {
            Node(parent, out var menuBar).Mutate(s.MenuBar);
            {
                menuBar.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(0)
                    .PaddingV(metrics.MenuBarPadding);

                Brand(menuBar);
                MenuItem(menuBar, "File");
                MenuItem(menuBar, "Edit");
                MenuItem(menuBar, "View");
                MenuItem(menuBar, "Tools");
                MenuItem(menuBar, "Build");
                Spacer(menuBar);
                TextLabel(menuBar, "Layout: Studio    Scene: sandbox", metrics.MenuBarHeight, false, true);
            }
        }

        void Brand(EntMut parent)
        {
            Node(parent, out var brand)
                .Mutate(s.RightRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((layout.BrandAreaWidth, 0))
                .ColorV(palette.Panel);
            {
                brand.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(metrics.LooseSpacing)
                    .PaddingV(metrics.BrandPadding);

                Node(brand, out var mark)
                    .AlignmentV(Alignment.Vertical)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 0))
                    .SizeV((layout.BrandMarkSize, layout.BrandMarkSize))
                    .ColorV(palette.ActiveSurface);
                {
                    Node(mark)
                        .IsFloatingV(true)
                        .IsPostSizedV(true)
                        .AlignmentV(Alignment.Top | Alignment.Left)
                        .SizeRelativeV((1, 0))
                        .SizeV((0, metrics.Hairline))
                        .ColorV(palette.Accent);

                    Node(mark)
                        .IsFloatingV(true)
                        .IsPostSizedV(true)
                        .AlignmentV(Alignment.Bottom | Alignment.Left)
                        .SizeRelativeV((1, 0))
                        .SizeV((0, metrics.Hairline))
                        .ColorV(palette.Accent);

                    Node(mark)
                        .IsFloatingV(true)
                        .IsPostSizedV(true)
                        .AlignmentV(Alignment.Top | Alignment.Left)
                        .SizeRelativeV((0, 1))
                        .SizeV((metrics.Hairline, 0))
                        .ColorV(palette.Accent);

                    Node(mark)
                        .IsFloatingV(true)
                        .IsPostSizedV(true)
                        .AlignmentV(Alignment.Top | Alignment.Right)
                        .SizeRelativeV((0, 1))
                        .SizeV((metrics.Hairline, 0))
                        .ColorV(palette.Accent);
                }

                TextLabel(brand, "AlvorKit Studio", metrics.MenuBarHeight, true, false);
            }
        }

        void Toolbar(EntMut parent)
        {
            Node(parent, out var toolbar).Mutate(s.Toolbar);
            {
                toolbar.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(metrics.ToolbarSpacing)
                    .PaddingV(metrics.ToolbarPadding);

                TransformTools(toolbar);
                Button(toolbar, "Select Tool", false);
                Button(toolbar, "Snap 0.25m", false);
                Spacer(toolbar);
                Button(toolbar, "Play", false);
                Button(toolbar, "Build", false);
            }
        }

        void TransformTools(EntMut parent)
        {
            Node(parent, out var group)
                .Mutate(s.RightRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeV((0, metrics.SquareButtonSize))
                .SizeInnerSumRelativeV((1, 0))
                .PaddingV(metrics.TransformGroupPadding)
                .MarginV((0, 0, metrics.TransformGroupMarginRight, 0));
            {
                group.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSpacingV(metrics.CompactSpacing);

                SquareButton(group, "M", true);
                SquareButton(group, "R", false);
                SquareButton(group, "S", false);
            }
        }

        void Workspace(EntMut parent)
        {
            Node(parent, out var workspace)
                .ColorV(palette.AppBackground);
            {
                workspace.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(0);

                DockPanel(workspace, layout.LeftDockWidth, "Scene", "Outliner");
                Splitter(workspace, layout.SplitterWidth);
                CenterDock(workspace);
                Splitter(workspace, layout.SplitterWidth);
                DockPanel(workspace, layout.RightDockWidth, "Inspector", "Properties");
            }
        }

        void DockPanel(EntMut parent, float width, string leftTitle, string rightTitle)
        {
            Node(parent, out var panel)
                .Mutate(PanelSurface)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((width, 0));
            {
                panel.Mutate()
                    .InnerLayoutV(InnerLayout.VerticalList)
                    .InnerSizingV(InnerSizing.VerticalWeight);

                PanelTitle(panel, leftTitle, rightTitle);
                PanelBody(panel);
            }
        }

        void CenterDock(EntMut parent)
        {
            Node(parent, out var center)
                .Mutate(s.Board)
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSizingV(InnerSizing.VerticalWeight)
                .ColorV(palette.AppBackground);
            {
                ViewportPanel(center);
                BottomDock(center);

                s.BottomRule(center);
            }
        }

        void ViewportPanel(EntMut parent)
        {
            Node(parent, out var panel)
                .ColorV(palette.Panel);
            {
                panel.Mutate()
                    .InnerLayoutV(InnerLayout.VerticalList)
                    .InnerSizingV(InnerSizing.VerticalWeight);

                ViewportHeader(panel);

                Node(panel, out var viewport)
                    .ColorV(palette.AppBackground);
                {
                    Node(viewport)
                        .Mutate(s.Border)
                        .IsFloatingV(true)
                        .AlignmentV(Alignment.Bottom | Alignment.Right)
                        .OffsetV((-layout.AxisWidgetInset, -layout.AxisWidgetInset))
                        .SizeRelativeV((0, 0))
                        .SizeV((layout.AxisWidgetSize, layout.AxisWidgetSize))
                        .ColorV(palette.WithAlpha(palette.Panel, 0.88f));
                }
            }
        }

        void ViewportHeader(EntMut parent)
        {
            Node(parent, out var header)
                .Mutate(s.BottomRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, metrics.ViewportHeaderHeight))
                .ColorV(palette.Raised);
            {
                header.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(metrics.ToolbarSpacing)
                    .PaddingV(metrics.ViewportHeaderPadding);

                Chip(header, "Perspective");
                Chip(header, "Lit");
                Chip(header, "Gizmos");
                Chip(header, "Snap 0.25m");
                Spacer(header);
                Chip(header, "Zoom 83%");
                Chip(header, "Grid 1m");
            }
        }

        void BottomDock(EntMut parent)
        {
            Node(parent, out var panel)
                .Mutate(s.TopRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, layout.BottomDockHeight))
                .ColorV(palette.Panel);
            {
                panel.Mutate()
                    .InnerLayoutV(InnerLayout.VerticalList)
                    .InnerSizingV(InnerSizing.VerticalWeight);

                TabStrip(panel);
                BottomBody(panel);
            }
        }

        void TabStrip(EntMut parent)
        {
            Node(parent, out var tabs)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, metrics.TabStripHeight))
                .MarginV((0, layout.BottomDockTopInset, 0, 0))
                .ColorV(palette.Raised);
            {
                tabs.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSpacingV(0);

                Tab(tabs, "Assets", true);
                Tab(tabs, "Console", false);
                Tab(tabs, "Profiler", false);
                Tab(tabs, "Timeline", false);

                Node(tabs)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Bottom | Alignment.Left)
                    .SizeRelativeV((1, 0))
                    .SizeV((0, metrics.Hairline))
                    .ColorV(palette.Border);
            }
        }

        void BottomBody(EntMut parent)
        {
            Node(parent, out var body)
                .ColorV(palette.Panel);
            {
                body.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight);

                Node(body)
                    .Mutate(s.RightRule)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .SizeRelativeV((0, 1))
                    .SizeV((layout.AssetFolderWidth, 0))
                    .ColorV(palette.WithAlpha(palette.Panel, 0.86f));

                Node(body, out var assets)
                    .ColorV(palette.Panel);
                {
                    assets.Mutate()
                        .InnerLayoutV(InnerLayout.VerticalList)
                        .InnerSizingV(InnerSizing.VerticalWeight);

                    AssetToolbar(assets);
                    Node(assets).ColorV(palette.Panel);
                }
            }
        }

        void AssetToolbar(EntMut parent)
        {
            Node(parent, out var toolbar)
                .Mutate(s.BottomRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((1, 0))
                .SizeV((0, metrics.AssetToolbarHeight))
                .ColorV(palette.Panel);
            {
                Node(toolbar, out var row)
                    .SizeRelativeV((1, 1))
                    .PaddingV(metrics.AssetToolbarPadding);
                {
                    row.Mutate()
                        .InnerLayoutV(InnerLayout.HorizontalList)
                        .InnerSizingV(InnerSizing.HorizontalWeight)
                        .InnerSpacingV(metrics.ToolbarSpacing);

                    Spacer(row);
                    Button(row, "New", false);
                    SquareButton(row, "G", true);
                    SquareButton(row, "L", false);
                }
            }
        }

        void StatusBar(EntMut parent)
        {
            Node(parent, out var status).Mutate(s.StatusBar);
            {
                status.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(metrics.StatusSpacing)
                    .PaddingV(metrics.StatusBarPadding);

                TextLabel(status, "C:/Projects/AlvorKit/stress-scenes/sandbox", metrics.StatusBarHeight, false, false);
                TextLabel(status, "Selected: none", metrics.StatusBarHeight, false, false);
                Spacer(status);
                TextLabel(status, "Memory 418 MB", metrics.StatusBarHeight, false, false);
                TextLabel(status, "Frame 6.9 ms", metrics.StatusBarHeight, false, false);
                TextLabel(status, "Ready", metrics.StatusBarHeight, false, false);
            }
        }

        void PanelSurface(EntMut panel)
        {
            panel.Mutate()
                .Mutate(s.BottomRule)
                .ColorV(palette.Panel);
        }

        void PanelTitle(EntMut parent, string left, string right)
        {
            Node(parent, out var title)
                .Mutate(s.PanelTitle);
            {
                title.Mutate()
                    .InnerLayoutV(InnerLayout.HorizontalList)
                    .InnerSizingV(InnerSizing.HorizontalWeight)
                    .InnerSpacingV(metrics.LooseSpacing)
                    .PaddingV(metrics.PanelTitlePadding);

                TextLabel(title, left, metrics.PanelTitleHeight, true, false);
                Spacer(title);
                TextLabel(title, right, metrics.PanelTitleHeight, false, false);
            }
        }

        void PanelBody(EntMut parent) =>
            Node(parent)
                .ColorV(palette.Panel);

        void MenuItem(EntMut parent, string text)
        {
            Node(parent)
                .Mutate(s.MenuItem)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeTextRelativeV((1, 0))
                .SizeV((0, metrics.MenuBarHeight))
                .TextV(text);
        }

        void TextLabel(EntMut parent, string text, float height, bool strong, bool rightAligned)
        {
            Node(parent, out var label)
                .Mutate(strong ? s.EmphasisText : s.MutedText)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 0))
                .SizeTextRelativeV((1, 0))
                .SizeV((0, height))
                .TextPaddingV((0, 0, metrics.RightGlyphPadding, 0))
                .TextV(text);

            if (rightAligned)
                label.Mutate()
                    .TextAlignmentV(Alignment.Right | Alignment.Vertical);
        }

        void Button(EntMut parent, string text, bool active)
        {
            Node(parent)
                .Mutate(active ? s.ActiveButton : s.Button)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(text);
        }

        void SquareButton(EntMut parent, string text, bool active)
        {
            Node(parent)
                .Mutate(active ? s.ActiveSquareButton : s.SquareButton)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(text);
        }

        void Chip(EntMut parent, string text)
        {
            Node(parent)
                .Mutate(s.Chip)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(text);
        }

        void Tab(EntMut parent, string text, bool active)
        {
            Node(parent, out var tab)
                .Mutate(active ? s.ActiveTab : s.Tab)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextV(text);
            {
                if (!active)
                    return;

                Node(tab)
                    .IsFloatingV(true)
                    .AlignmentV(Alignment.Top | Alignment.Left)
                    .OffsetV((0, layout.ActiveTabAccentOffset))
                    .SizeRelativeV((1, 0))
                    .SizeV((0, metrics.ActiveTabAccentHeight))
                    .ColorV(palette.Accent);
            }
        }

        void Splitter(EntMut parent, float width)
        {
            Node(parent)
                .Mutate(s.LeftRule)
                .Mutate(s.RightRule)
                .SizeWeightTypeV(SizeWeightType.Self)
                .SizeRelativeV((0, 1))
                .SizeV((width, 0))
                .ColorV(palette.AppBackground);
        }

        static void Spacer(EntMut parent) =>
            Node(parent)
                .ColorV(default);
    }
}
