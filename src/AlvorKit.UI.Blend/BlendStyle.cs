namespace AlvorKit;

/// <summary>Applies Blender-inspired recipes to AlvorKit UI nodes, using the embedded Inter faces from <see cref="RootInter"/>.</summary>
public class BlendStyle
{
    /// <summary>Regular font exposed to style collaborators and callers that measure text.</summary>
    private readonly Font font;

    /// <summary>Layout recipes backing the public style façade.</summary>
    private readonly BlendStyleLayout layout;

    /// <summary>Text recipes backing the public style façade.</summary>
    private readonly BlendStyleTypography typography;

    /// <summary>Interactive control recipes backing the public style façade.</summary>
    private readonly BlendStyleControls controls;

    /// <summary>Panel, separator, and overlay recipes backing the public style façade.</summary>
    private readonly BlendStyleSurfaces surfaces;

    /// <summary>Creates the style from embedded fonts, graphics ownership, UI scale, and keyboard input.</summary>
    public BlendStyle(RootInter inter, GlLayer gl, RootUiScale scale, RootKeyboard keyboard)
    {
        font = inter.Regular;
        layout = new(this);
        typography = new(this, font, inter.SemiBold);
        controls = new(this, font, gl, scale, keyboard);
        surfaces = new(this);
    }

    /// <summary>Gets the active color palette.</summary>
    public virtual BlendPalette Palette => BlendPalette.Default;

    /// <summary>Gets the active layout metrics.</summary>
    public virtual BlendMetrics Metrics { get; } = new();

    /// <summary>Gets the regular text font face, for collaborators that measure text.</summary>
    public Font TextFont => font;

    /// <summary>Applies the full-window vertical root layout with editor-shell click-away semantics.</summary>
    public void Root(EntMut ent) => layout.Root(ent);

    /// <summary>Applies an explicit-position board layout.</summary>
    public void Board(EntMut ent) => layout.Board(ent);

    /// <summary>Applies the top application menu bar surface.</summary>
    public void MenuBar(EntMut ent) => layout.MenuBar(ent);

    /// <summary>Applies the main tool strip surface.</summary>
    public void Toolbar(EntMut ent) => layout.Toolbar(ent);

    /// <summary>Applies the bottom status strip surface.</summary>
    public void StatusBar(EntMut ent) => layout.StatusBar(ent);

    /// <summary>Applies a plain panel fill.</summary>
    public void Panel(EntMut ent) => layout.Panel(ent);

    /// <summary>Applies a raised panel title strip.</summary>
    public void PanelTitle(EntMut ent) => layout.PanelTitle(ent);

    /// <summary>Applies a vertical panel body that fills the available space.</summary>
    public void PanelFillList(EntMut ent) => layout.PanelFillList(ent);

    /// <summary>Applies a vertical panel section sized to its children.</summary>
    public void PanelFitList(EntMut ent) => layout.PanelFitList(ent);

    /// <summary>Applies a raised horizontal header strip.</summary>
    public void HeaderStrip(EntMut ent) => layout.HeaderStrip(ent);

    /// <summary>Applies an inset vertical list panel with a bottom separator.</summary>
    public void InsetPanelList(EntMut ent) => layout.InsetPanelList(ent);

    /// <summary>Applies a padded vertical list body.</summary>
    public void ListBody(EntMut ent) => layout.ListBody(ent);

    /// <summary>Applies a vertical list sized from its children.</summary>
    public void VerticalList(EntMut ent) => layout.VerticalList(ent);

    /// <summary>Applies a horizontal list sized from its children.</summary>
    public void HorizontalList(EntMut ent) => layout.HorizontalList(ent);

    /// <summary>Applies a full-size weighted horizontal list.</summary>
    public void HorizontalFill(EntMut ent) => layout.HorizontalFill(ent);

    /// <summary>Applies a fixed-height horizontal row.</summary>
    public void HorizontalRow(EntMut ent) => layout.HorizontalRow(ent);

    /// <summary>Applies a selectable horizontal list row.</summary>
    public void SelectableListRow(EntMut ent) => layout.SelectableListRow(ent);

    /// <summary>Applies a fixed-height label/value metric row.</summary>
    public void MetricRow(EntMut ent) => layout.MetricRow(ent);

    /// <summary>Applies body text matching the editor-shell reference.</summary>
    public void Text(EntMut ent) => typography.Text(ent);

    /// <summary>Applies a semibold text treatment using the loaded font face.</summary>
    public void EmphasisText(EntMut ent) => typography.EmphasisText(ent);

    /// <summary>Applies smaller muted metadata text.</summary>
    public void MutedText(EntMut ent) => typography.MutedText(ent);

    /// <summary>Applies centered text for compact controls.</summary>
    public void CenterText(EntMut ent) => typography.CenterText(ent);

    /// <summary>Applies a text label sized from its text.</summary>
    public void Label(EntMut ent) => typography.Label(ent);

    /// <summary>Applies a muted text label sized from its text.</summary>
    public void MutedLabel(EntMut ent) => typography.MutedLabel(ent);

    /// <summary>Applies an emphasized text label sized from its text.</summary>
    public void EmphasisLabel(EntMut ent) => typography.EmphasisLabel(ent);

    /// <summary>Applies a label that fills its assigned row cell.</summary>
    public void CellLabel(EntMut ent) => typography.CellLabel(ent);

    /// <summary>Applies a muted label that fills its assigned row cell.</summary>
    public void MutedCellLabel(EntMut ent) => typography.MutedCellLabel(ent);

    /// <summary>Applies an emphasized label that fills its assigned row cell.</summary>
    public void EmphasisCellLabel(EntMut ent) => typography.EmphasisCellLabel(ent);

    /// <summary>Applies a menu item hit target with transparent idle fill.</summary>
    public void MenuItem(EntMut ent) => typography.MenuItem(ent);

    /// <summary>Applies a bottom-dock tab surface sized from its text.</summary>
    public void Tab(EntMut ent) => typography.Tab(ent);

    /// <summary>Applies an active bottom-dock tab surface sized from its text.</summary>
    public void ActiveTab(EntMut ent) => typography.ActiveTab(ent);

    /// <summary>Applies the emphasized first line of a tooltip.</summary>
    public void TooltipTitle(EntMut ent) => typography.TooltipTitle(ent);

    /// <summary>Applies a muted tooltip detail line.</summary>
    public void TooltipLine(EntMut ent) => typography.TooltipLine(ent);

    /// <summary>Builds a compact rounded button using the standard Blend button font size.</summary>
    public void Button(EntMut ent) => controls.Button(ent);

    /// <summary>Builds an active compact rounded button using the standard Blend button font size.</summary>
    public void ActiveButton(EntMut ent) => controls.ActiveButton(ent);

    /// <summary>Builds a compact rounded button sized for title rows and toolbar strips.</summary>
    public void ToolbarButton(EntMut ent) => controls.ToolbarButton(ent);

    /// <summary>Builds an active compact rounded button sized for title rows and toolbar strips.</summary>
    public void ActiveToolbarButton(EntMut ent) => controls.ActiveToolbarButton(ent);

    /// <summary>Builds a compact square button using the standard Blend square-button font size.</summary>
    public void SquareButton(EntMut ent) => controls.SquareButton(ent);

    /// <summary>Builds an active compact square button using the standard Blend square-button font size.</summary>
    public void ActiveSquareButton(EntMut ent) => controls.ActiveSquareButton(ent);

    /// <summary>Applies a smaller toolbar chip.</summary>
    public void Chip(EntMut ent) => controls.Chip(ent);

    /// <summary>Applies a non-interactive readout chip that remains hoverable for tooltips.</summary>
    public void ReadoutChip(EntMut ent) => controls.ReadoutChip(ent);

    /// <summary>Applies a static field-like surface.</summary>
    public void Field(EntMut ent) => controls.Field(ent);

    /// <summary>Runs the node's click or press callback when it is focused and Enter is pressed.</summary>
    public void ActivateOnEnter(EntMut ent) => controls.ActivateOnEnter(ent);

    /// <summary>Adds the accent bar that marks an active tab, sparing the tab's right separator.</summary>
    public void ActiveTabAccent(EntMut ent) => surfaces.ActiveTabAccent(ent);

    /// <summary>Applies a raised tab strip surface.</summary>
    public void TabStrip(EntMut ent) => surfaces.TabStrip(ent);

    /// <summary>Fills the tab strip after the last tab and carries its bottom rule.</summary>
    public void TabFiller(EntMut ent) => surfaces.TabFiller(ent);

    /// <summary>Applies a vertical dock panel surface with a bottom separator.</summary>
    public void Dock(EntMut ent) => surfaces.Dock(ent);

    /// <summary>Applies a thin vertical splitter between docks.</summary>
    public void Splitter(EntMut ent) => surfaces.Splitter(ent);

    /// <summary>Adds a one-pixel border around a node.</summary>
    public void Border(EntMut ent) => surfaces.Border(ent);

    /// <summary>Adds a one-pixel strong border around a node.</summary>
    public void StrongBorder(EntMut ent) => surfaces.StrongBorder(ent);

    /// <summary>Adds a top hairline rule.</summary>
    public void TopRule(EntMut ent) => surfaces.TopRule(ent);

    /// <summary>Adds a bottom hairline rule.</summary>
    public void BottomRule(EntMut ent) => surfaces.BottomRule(ent);

    /// <summary>Adds a left hairline rule.</summary>
    public void LeftRule(EntMut ent) => surfaces.LeftRule(ent);

    /// <summary>Adds a right hairline rule.</summary>
    public void RightRule(EntMut ent) => surfaces.RightRule(ent);

    /// <summary>Applies the full-screen tinted layer behind a modal dialog.</summary>
    public void ModalLayer(EntMut ent) => surfaces.ModalLayer(ent);

    /// <summary>Applies a centered modal dialog panel.</summary>
    public void ModalPanel(EntMut ent) => surfaces.ModalPanel(ent);

    /// <summary>Applies a modal dialog's padded content area.</summary>
    public void ModalContent(EntMut ent) => surfaces.ModalContent(ent);

    /// <summary>Applies a floating tooltip surface that sizes to its line children.</summary>
    public void Tooltip(EntMut ent) => surfaces.Tooltip(ent);

    /// <summary>Applies a small legend swatch; set the color at the call site.</summary>
    public void Swatch(EntMut ent) => surfaces.Swatch(ent);

    /// <summary>Adds a floating hairline rule node with a fixed color.</summary>
    public static void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, Vec4 color) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorV(color);

    /// <summary>Adds a floating hairline rule node with a reactive color.</summary>
    public static void Rule(EntMut ent, Alignment alignment, Vec2 relativeSize, Vec2 size, Func<Vec4> color) =>
        Node(ent)
            .IsFloatingV(true)
            .IsPostSizedV(true)
            .AlignmentV(alignment)
            .SizeRelativeV(relativeSize)
            .SizeV(size)
            .ColorF(color);
}
