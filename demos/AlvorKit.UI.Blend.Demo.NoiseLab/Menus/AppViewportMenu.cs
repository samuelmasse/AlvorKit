namespace AlvorKit.UI.Blend.Demo.NoiseLab;

/// <summary>Builds the noise viewport: stats header strip and the pannable, zoomable preview with a value probe.</summary>
[App]
public class AppViewportMenu(
    RootText text,
    RootUiMouse uiMouse,
    RootKeyboard keyboard,
    AppStyle s,
    AppSession session)
{
    public void Create(EntMut root)
    {
        const float minStep = 0.15f;
        const float maxStep = 8f;
        const float zWheelStep = 2f;

        Node(root, out var viewport)
            .Mutate(s.PanelFillList);
        {
            Node(viewport, out var header)
                .Mutate(s.HeaderStrip)
                .SizeV((0, s.Metrics.ViewportHeaderHeight))
                .PaddingV(s.Metrics.ViewportHeaderPadding)
                .InnerSpacingV(s.Metrics.ToolbarSpacing);
            {
                Readout(header, "slice depth\nShift-wheel over the preview steps z", () => text.Format("z {0:0.0}", session.Z));

                Node(header);

                Readout(header, "smallest generated sample", () => text.Format("min {0:0.000}", session.Field.SampleMin));
                Readout(header, "largest generated sample", () => text.Format("max {0:0.000}", session.Field.SampleMax));
                Readout(header, "time of the last generation", () => text.Format("gen {0:0.00} ms", session.Field.GenerateMs));
            }

            Node(viewport, out var body)
                .ColorV(s.Palette.AppBackground)
                .SizeRelativeV((1, 1))
                .InnerLayoutV(InnerLayout.VerticalList)
                .InnerSizingV(InnerSizing.VerticalWeight)
                .PaddingV((14, 12, 14, 12))
                .InnerSpacingV(s.Metrics.CompactSpacing);
            {
                Node(body)
                    .Mutate(s.MutedLabel)
                    .SizeWeightTypeV(SizeWeightType.Self)
                    .TextF(() => text.Format(
                        "2D slice at z = {0:0.0} — drag pans · wheel zooms · Shift-wheel steps z · hover probes the sample",
                        session.Z));

                var panStart = Vec2.Zero;
                var panOffset = Vec2.Zero;
                // The sample grid tracks this node's visible size (one sample per UI unit), so window
                // resizes regenerate more or fewer samples instead of scaling the image; the texture
                // therefore always draws texel-accurate. Sprite UVs are y-inverted against uploaded rows,
                // so an unflipped draw would mirror the buffer vertically (and invert vertical panning).
                Node(body, out var view)
                    .Mutate(s.Board)
                    .SizeRelativeV((1, 1))
                    .TextureF(() => session.Field.Texture)
                    .TextureFlipV(SpriteBatchFlip.Vertical)
                    .IsSelectableV(true)
                    .IsSilentFocusableV(true)
                    .IsScrollableV(true)
                    .CursorF(() => CursorShape.Crosshair)
                    .Mutate(s.Border)
                    .OnPressF(() =>
                    {
                        panStart = uiMouse.Position;
                        panOffset = session.Offset;
                    })
                    .OnUpdateF(() =>
                    {
                        session.ResizeView((int)view.SizeR.X, (int)view.SizeR.Y);

                        if (!view.IsPressedR)
                            return;

                        var delta = uiMouse.Position - panStart;
                        var offset = panOffset - delta;
                        if (offset != session.Offset)
                        {
                            session.Offset = offset;
                            session.MarkDirty();
                        }
                    })
                    .OnScrollF(wheel =>
                    {
                        if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
                        {
                            session.Z += wheel.Y * zWheelStep;
                            session.MarkDirty();
                            return;
                        }

                        var step = Math.Clamp(session.Step * MathF.Pow(0.9f, wheel.Y), minStep, maxStep);
                        if (step == session.Step)
                            return;

                        Vec2 center = (session.Field.Width * 0.5f, session.Field.Height * 0.5f);
                        var scale = session.Step / step;
                        session.Offset = ((session.Offset + center) * scale) - center;
                        session.Step = step;
                        session.MarkDirty();
                    })
                    .TooltipF(() =>
                    {
                        // Renders can happen before the first logical update sizes the grid (for example
                        // while the window is not yet focused), so probe nothing until samples exist.
                        if (session.Field.Width == 0)
                            return default;

                        var local = uiMouse.Position - view.PositionR;
                        var x = (int)local.X;
                        var y = (int)local.Y;
                        return text.Format(
                            "value {0:0.000}\npixel {1}, {2} · world {3:0.0}, {4:0.0}",
                            session.Field.Sample(x, y),
                            x,
                            y,
                            (session.Offset.X + x) * session.Step,
                            (session.Offset.Y + y) * session.Step);
                    });
            }
        }

        void Readout(EntMut header, string tooltip, Func<ReadOnlySpan<char>> value)
        {
            Node(header)
                .Mutate(s.ReadoutChip)
                .AlignmentV(Alignment.Vertical)
                .SizeWeightTypeV(SizeWeightType.Self)
                .TextF(value)
                .TooltipV(tooltip);
        }
    }
}
