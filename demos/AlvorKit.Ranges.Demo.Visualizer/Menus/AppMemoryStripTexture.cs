namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Rasterizes dense allocator memory strips into one-row textures and resolves hovered bytes.</summary>
[App]
public class AppMemoryStripTexture(
    RootGl gl,
    RootUiMouse uiMouse,
    AppStyle s,
    AppSession session,
    AppTextureLimits limits,
    AppMemoryStripTooltips tooltips)
{
    private readonly StripTextureCache overview = new();
    private readonly StripTextureCache detail = new();

    /// <summary>Returns a texture containing the current memory strip view.</summary>
    public Texture Texture(AppMemoryStripView view)
    {
        var cache = Cache(view);
        var width = TextureWidth(view);
        var signature = new StripTextureSignature(
            session.VisualRevision,
            view.ViewStart,
            view.ViewEnd,
            view.MuteTail,
            session.ShowPadding,
            session.MemoryOverlayMode,
            width);

        if (cache.Signature == signature && cache.Texture is not null)
            return cache.Texture;

        EnsureCache(cache, width);
        Rasterize(cache.Pixels, width, view);
        cache.Texture!.PixelsMipmap = cache.Pixels;
        cache.Signature = signature;
        return cache.Texture;
    }

    /// <summary>Returns tooltip text for the byte currently hovered inside a strip node.</summary>
    public ReadOnlySpan<char> Tooltip(AppMemoryStripView view, EntMut strip)
    {
        if (strip.SizeR.X <= 0 || uiMouse.Position.X < strip.PositionR.X || uiMouse.Position.X > strip.PositionR.X + strip.SizeR.X)
            return [];

        var byteIndex = HoverByte(view, strip);
        if (TryFindRange(view.Snapshot.Ranges, byteIndex, out var range))
        {
            if (session.IsLatestPayloadRequest(range.Slot) && byteIndex >= range.PayloadIndex && byteIndex < range.PayloadIndex + range.Size)
                return tooltips.LatestRequest(view, range);

            if (byteIndex < range.PayloadIndex)
                return tooltips.Padding(view, range, range.LeadingPadding, leading: true);

            if (byteIndex < range.PayloadIndex + range.Size)
                return tooltips.Payload(view, range);

            if (byteIndex < range.PayloadIndex + range.CapacitySize)
                return tooltips.Retained(view, range);

            return tooltips.Padding(view, range, range.TrailingPadding, leading: false);
        }

        if (TryFindFreeSpan(view.Snapshot.FreeSpans, byteIndex, out var span))
        {
            var mutedTail = view.MuteTail && span.Index + span.Size == view.Snapshot.Size && view.Snapshot.Ranges.Length > 0;
            return tooltips.Free(view, span.Index, span.Size, mutedTail);
        }

        return [];
    }

    private StripTextureCache Cache(AppMemoryStripView view) =>
        view.DetailedLabels ? detail : overview;

    private int TextureWidth(AppMemoryStripView view)
    {
        var length = Math.Max(1, view.ViewEnd - view.ViewStart);
        return (int)Math.Min(length, limits.MaxTextureWidth);
    }

    private void EnsureCache(StripTextureCache cache, int width)
    {
        if (cache.Texture is not null && cache.Width == width)
            return;

        cache.Texture?.Dispose();
        cache.Texture = new Texture2D(gl, ((uint)width, 1u))
        {
            MinFilter = GlTextureMinFilter.LinearMipmapLinear,
            MagFilter = GlTextureMagFilter.Nearest,
            WrapS = GlTextureWrapMode.ClampToEdge,
            WrapT = GlTextureWrapMode.ClampToEdge,
        };
        cache.Pixels = new Vec4u8[width];
        cache.Width = width;
        cache.Signature = default;
    }

    private void Rasterize(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        switch (session.MemoryOverlayMode)
        {
            case AppMemoryOverlayMode.Allocations:
                RasterizeAllocations(pixels, width, view);
                break;
            case AppMemoryOverlayMode.Occupancy:
            case AppMemoryOverlayMode.Density:
            case AppMemoryOverlayMode.Efficiency:
            case AppMemoryOverlayMode.Slack:
                RasterizeAggregatePixels(pixels, width, view, session.MemoryOverlayMode);
                break;
            case AppMemoryOverlayMode.Fragmentation:
                RasterizeFragmentation(pixels, width, view);
                break;
            case AppMemoryOverlayMode.Churn:
                RasterizeChurn(pixels, width, view);
                break;
            case AppMemoryOverlayMode.Outliers:
                RasterizeOutliers(pixels, width, view);
                break;
            case AppMemoryOverlayMode.Relocation:
                RasterizeRelocation(pixels, width, view);
                break;
            default:
                RasterizeAllocations(pixels, width, view);
                break;
        }
    }

    private void RasterizeAllocations(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        Fill(pixels, Pack(s.PanelInsetColor));

        var snapshot = view.Snapshot;
        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            var mutedTail = view.MuteTail && span.Index + span.Size == snapshot.Size && snapshot.Ranges.Length > 0;
            DrawSegment(pixels, width, view, span.Index, span.Size, mutedTail ? s.TailFreeBlockColor : s.FreeBlockColor);
        }

        for (var i = 0; i < snapshot.Ranges.Length; i++)
        {
            var range = snapshot.Ranges[i];
            var color = s.AllocationColor(range.Slot);
            DrawSegment(pixels, width, view, range.Index, range.ReservedSize, s.Dim(color, 0.42f));

            if (session.ShowPadding && range.LeadingPadding > 0)
                DrawSegment(pixels, width, view, range.Index, range.LeadingPadding, s.PaddingColor);

            if (session.ShowPadding && range.TrailingPadding > 0)
                DrawSegment(
                    pixels,
                    width,
                    view,
                    range.PayloadIndex + range.CapacitySize,
                    range.TrailingPadding,
                    s.PaddingColor);

            if (range.RetainedExtraSize > 0)
                DrawSegment(pixels, width, view, range.PayloadIndex + range.Size, range.RetainedExtraSize, s.RetainedColor);

            DrawSegment(pixels, width, view, range.PayloadIndex, range.Size, color);

            if (session.IsLatestPayloadRequest(range.Slot))
                DrawSegment(pixels, width, view, range.PayloadIndex, range.Size, s.LatestRequestFillColor);
        }
    }

    private void RasterizeAggregatePixels(Vec4u8[] pixels, int width, AppMemoryStripView view, AppMemoryOverlayMode mode)
    {
        var ranges = view.Snapshot.Ranges;
        var rangeIndex = 0;
        var viewLength = Math.Max(1.0, view.ViewEnd - view.ViewStart);
        for (var x = 0; x < width; x++)
        {
            var pixelStart = view.ViewStart + x * viewLength / width;
            var pixelEnd = view.ViewStart + (x + 1) * viewLength / width;
            while (rangeIndex < ranges.Length && RangeEnd(ranges[rangeIndex]) <= pixelStart)
                rangeIndex++;

            var totals = new PixelMemoryTotals(Math.Max(1.0, pixelEnd - pixelStart));
            for (var i = rangeIndex; i < ranges.Length && ranges[i].Index < pixelEnd; i++)
                AddRangeTotals(ref totals, ranges[i], pixelStart, pixelEnd);

            pixels[x] = Pack(AggregateColor(totals, mode));
        }
    }

    private void RasterizeFragmentation(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        Fill(pixels, Pack(s.OverlayOccupiedColor));

        var snapshot = view.Snapshot;
        var largestFree = LargestFreeSpan(snapshot.FreeSpans);
        for (var i = 0; i < snapshot.FreeSpans.Length; i++)
        {
            var span = snapshot.FreeSpans[i];
            var mutedTail = view.MuteTail && span.Index + span.Size == snapshot.Size && snapshot.Ranges.Length > 0;
            DrawSegment(pixels, width, view, span.Index, span.Size, mutedTail ? s.TailFreeBlockColor : FragmentationColor(span.Size, largestFree));
        }
    }

    private void RasterizeChurn(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        Fill(pixels, Pack(s.OverlayFreeColor));

        var ranges = view.Snapshot.Ranges;
        for (var i = 0; i < ranges.Length; i++)
        {
            var range = ranges[i];
            var color = s.ChurnIdleColor;
            if (session.TryRecentSlotAge(range.Slot, out var age))
                color = RecentColor(age);

            DrawSegment(pixels, width, view, range.Index, range.ReservedSize, color);
        }
    }

    private void RasterizeOutliers(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        Fill(pixels, Pack(s.OverlayFreeColor));

        var ranges = view.Snapshot.Ranges;
        for (var i = 0; i < ranges.Length; i++)
        {
            var range = ranges[i];
            var color = session.IsOutlierSlot(range.Slot)
                ? Mix(s.OutlierColor, s.DensityHighColor, session.OutlierIntensity(range.Slot))
                : s.OverlayOccupiedColor;
            DrawSegment(pixels, width, view, range.Index, range.ReservedSize, color);
        }
    }

    private void RasterizeRelocation(Vec4u8[] pixels, int width, AppMemoryStripView view)
    {
        Fill(pixels, Pack(s.OverlayFreeColor));

        var ranges = view.Snapshot.Ranges;
        for (var i = 0; i < ranges.Length; i++)
        {
            var range = ranges[i];
            var color = session.RangeMotionForSlot(range.Slot) switch
            {
                AppRangeMotionKind.New => s.RelocationNewColor,
                AppRangeMotionKind.Reused => s.RelocationReusedColor,
                AppRangeMotionKind.Moved => s.RelocationMovedColor,
                _ => s.OverlayOccupiedColor,
            };
            DrawSegment(pixels, width, view, range.Index, range.ReservedSize, color);
        }
    }

    private void DrawSegment(Vec4u8[] pixels, int width, AppMemoryStripView view, long index, long size, Vec4 color)
    {
        if (size <= 0)
            return;

        var clippedStart = Math.Max(index, view.ViewStart);
        var clippedEnd = Math.Min(index + size, view.ViewEnd);
        if (clippedEnd <= clippedStart)
            return;

        var viewLength = Math.Max(1.0, view.ViewEnd - view.ViewStart);
        var start = (int)Math.Floor((clippedStart - view.ViewStart) / viewLength * width);
        var end = (int)Math.Ceiling((clippedEnd - view.ViewStart) / viewLength * width);
        start = Math.Clamp(start, 0, width - 1);
        end = Math.Clamp(end, start + 1, width);

        var packed = Pack(color);
        for (var i = start; i < end; i++)
            pixels[i] = Over(packed, pixels[i]);
    }

    private Vec4 AggregateColor(PixelMemoryTotals totals, AppMemoryOverlayMode mode)
    {
        var density = Ratio(totals.Reserved, totals.Length);
        return mode switch
        {
            AppMemoryOverlayMode.Occupancy => OccupancyColor(totals, density),
            AppMemoryOverlayMode.Density => DensityColor(density),
            AppMemoryOverlayMode.Efficiency => EfficiencyColor(totals, density),
            AppMemoryOverlayMode.Slack => SlackColor(totals),
            _ => s.PanelInsetColor,
        };
    }

    private Vec4 OccupancyColor(PixelMemoryTotals totals, float density)
    {
        if (totals.Reserved <= 0)
            return s.OverlayFreeColor;

        var payloadRatio = Ratio(totals.Payload, totals.Reserved);
        var occupied = Mix(s.OccupancyReservedColor, s.OccupancyPayloadColor, payloadRatio);
        return Mix(s.OverlayFreeColor, occupied, density);
    }

    private Vec4 DensityColor(float density)
    {
        if (density <= 0)
            return s.OverlayFreeColor;

        return density < 0.5f
            ? Mix(s.DensityLowColor, s.DensityMidColor, density * 2f)
            : Mix(s.DensityMidColor, s.DensityHighColor, (density - 0.5f) * 2f);
    }

    private Vec4 EfficiencyColor(PixelMemoryTotals totals, float density)
    {
        if (totals.Reserved <= 0)
            return s.OverlayFreeColor;

        var efficiency = Ratio(totals.Payload, totals.Reserved);
        var color = efficiency < 0.5f
            ? Mix(s.EfficiencyWasteColor, s.EfficiencyMixedColor, efficiency * 2f)
            : Mix(s.EfficiencyMixedColor, s.EfficiencyGoodColor, (efficiency - 0.5f) * 2f);
        return Mix(s.OverlayFreeColor, color, density);
    }

    private Vec4 SlackColor(PixelMemoryTotals totals)
    {
        var free = Math.Max(0, totals.Length - totals.Reserved);
        return WeightedColor(
            s.OverlayFreeColor,
            free,
            s.AllocationColor(0),
            totals.Payload,
            s.RetainedColor,
            totals.Retained,
            s.PaddingColor,
            totals.Padding,
            totals.Length);
    }

    private Vec4 FragmentationColor(long size, long largestFree)
    {
        if (largestFree <= 0)
            return s.OverlayFreeColor;

        var ratio = Math.Clamp(size / (float)largestFree, 0f, 1f);
        return ratio < 0.12f
            ? Mix(s.FragmentTinyColor, s.FragmentMediumColor, ratio / 0.12f)
            : Mix(s.FragmentMediumColor, s.FragmentLargeColor, (ratio - 0.12f) / 0.88f);
    }

    private Vec4 RecentColor(int age)
    {
        var fade = Math.Clamp(age / 127f, 0f, 1f);
        return Mix(s.ChurnRecentColor, s.ChurnIdleColor, fade);
    }

    private long HoverByte(AppMemoryStripView view, EntMut strip)
    {
        var localX = Math.Clamp(uiMouse.Position.X - strip.PositionR.X, 0, strip.SizeR.X);
        var viewLength = Math.Max(1, view.ViewEnd - view.ViewStart);
        var offset = (long)Math.Floor(localX / Math.Max(1f, strip.SizeR.X) * viewLength);
        return Math.Clamp(view.ViewStart + offset, view.ViewStart, Math.Max(view.ViewStart, view.ViewEnd - 1));
    }

    private static bool TryFindRange(ReadOnlySpan<AllocatorRangeVisual> ranges, long byteIndex, out AllocatorRangeVisual range)
    {
        var low = 0;
        var high = ranges.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var candidate = ranges[mid];
            if (byteIndex < candidate.Index)
                high = mid - 1;
            else if (byteIndex >= candidate.Index + candidate.ReservedSize)
                low = mid + 1;
            else
            {
                range = candidate;
                return true;
            }
        }

        range = default;
        return false;
    }

    private static bool TryFindFreeSpan(ReadOnlySpan<AllocatorSpanVisual> spans, long byteIndex, out AllocatorSpanVisual span)
    {
        var low = 0;
        var high = spans.Length - 1;
        while (low <= high)
        {
            var mid = low + ((high - low) >> 1);
            var candidate = spans[mid];
            if (byteIndex < candidate.Index)
                high = mid - 1;
            else if (byteIndex >= candidate.Index + candidate.Size)
                low = mid + 1;
            else
            {
                span = candidate;
                return true;
            }
        }

        span = default;
        return false;
    }

    private static void AddRangeTotals(ref PixelMemoryTotals totals, AllocatorRangeVisual range, double pixelStart, double pixelEnd)
    {
        totals.Reserved += Overlap(range.Index, RangeEnd(range), pixelStart, pixelEnd);
        totals.Payload += Overlap(range.PayloadIndex, range.PayloadIndex + range.Size, pixelStart, pixelEnd);
        totals.Retained += Overlap(
            range.PayloadIndex + range.Size,
            range.PayloadIndex + range.CapacitySize,
            pixelStart,
            pixelEnd);
        totals.Padding += Overlap(range.Index, range.PayloadIndex, pixelStart, pixelEnd);
        totals.Padding += Overlap(
            range.PayloadIndex + range.CapacitySize,
            RangeEnd(range),
            pixelStart,
            pixelEnd);
    }

    private static double Overlap(double start, double end, double clipStart, double clipEnd) =>
        Math.Max(0, Math.Min(end, clipEnd) - Math.Max(start, clipStart));

    private static float Ratio(double numerator, double denominator) =>
        denominator <= 0 ? 0f : (float)Math.Clamp(numerator / denominator, 0, 1);

    private static Vec4 Mix(Vec4 left, Vec4 right, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        var inverse = 1f - amount;
        return (
            left.X * inverse + right.X * amount,
            left.Y * inverse + right.Y * amount,
            left.Z * inverse + right.Z * amount,
            left.W * inverse + right.W * amount);
    }

    private static Vec4 WeightedColor(
        Vec4 color0,
        double weight0,
        Vec4 color1,
        double weight1,
        Vec4 color2,
        double weight2,
        Vec4 color3,
        double weight3,
        double total)
    {
        if (total <= 0)
            return color0;

        var scale = 1f / (float)total;
        return (
            (float)(color0.X * weight0 + color1.X * weight1 + color2.X * weight2 + color3.X * weight3) * scale,
            (float)(color0.Y * weight0 + color1.Y * weight1 + color2.Y * weight2 + color3.Y * weight3) * scale,
            (float)(color0.Z * weight0 + color1.Z * weight1 + color2.Z * weight2 + color3.Z * weight3) * scale,
            1f);
    }

    private static long LargestFreeSpan(ReadOnlySpan<AllocatorSpanVisual> spans)
    {
        var largest = 0L;
        for (var i = 0; i < spans.Length; i++)
            largest = Math.Max(largest, spans[i].Size);

        return largest;
    }

    private static long RangeEnd(AllocatorRangeVisual range) => range.Index + range.ReservedSize;

    private static void Fill(Vec4u8[] pixels, Vec4u8 color)
    {
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = color;
    }

    private static Vec4u8 Pack(Vec4 color) =>
        (
            PackChannel(color.X),
            PackChannel(color.Y),
            PackChannel(color.Z),
            PackChannel(color.W));

    private static byte PackChannel(float value) =>
        (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);

    private static Vec4u8 Over(Vec4u8 source, Vec4u8 destination)
    {
        var alpha = source.W / 255f;
        var inverse = 1f - alpha;
        return (
            Blend(source.X, destination.X, alpha, inverse),
            Blend(source.Y, destination.Y, alpha, inverse),
            Blend(source.Z, destination.Z, alpha, inverse),
            255);
    }

    private static byte Blend(byte source, byte destination, float alpha, float inverse) =>
        (byte)Math.Clamp((int)MathF.Round(source * alpha + destination * inverse), 0, 255);

    private sealed class StripTextureCache
    {
        internal Texture2D? Texture;
        internal Vec4u8[] Pixels = [];
        internal int Width;
        internal StripTextureSignature Signature;
    }

    private readonly record struct StripTextureSignature(
        int Revision,
        long ViewStart,
        long ViewEnd,
        bool MuteTail,
        bool ShowPadding,
        AppMemoryOverlayMode Mode,
        int Width);

    private struct PixelMemoryTotals(double length)
    {
        internal readonly double Length = length;
        internal double Reserved;
        internal double Payload;
        internal double Retained;
        internal double Padding;
    }
}
