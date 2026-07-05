namespace AlvorKit.Ranges.Demo.Visualizer;

/// <summary>Rasterizes scenario command timelines into one-row textures and maps pointer positions to commands.</summary>
[App]
public class AppTimelineTexture(
    RootGl gl,
    RootText text,
    RootUiMouse uiMouse,
    AppStyle s,
    AppSession session,
    AppTextureLimits limits)
{
    private Texture2D? texture;
    private Vec4u8[] pixels = [];
    private AllocatorScenario? scenario;
    private AppTimelineOverlayMode mode;
    private int width;

    /// <summary>Returns a texture containing the current scenario command sequence.</summary>
    public Texture Texture()
    {
        var current = session.Runner.Scenario;
        var currentMode = session.TimelineOverlayMode;
        var commandCount = current.Commands.Length;
        var desiredWidth = Math.Max(1, Math.Min(commandCount, limits.MaxTextureWidth));
        if (texture is not null && ReferenceEquals(scenario, current) && width == desiredWidth && mode == currentMode)
            return texture;

        EnsureTexture(desiredWidth);
        Rasterize(current, desiredWidth, currentMode);
        texture!.PixelsMipmap = pixels;
        scenario = current;
        mode = currentMode;
        return texture;
    }

    /// <summary>Gets the command index currently under the pointer inside a timeline lane.</summary>
    public int HoverIndex(EntMut lane)
    {
        var count = session.Runner.Scenario.Commands.Length;
        if (count <= 0 || lane.SizeR.X <= 0)
            return 0;

        var localX = Math.Clamp(uiMouse.Position.X - lane.PositionR.X, 0, lane.SizeR.X);
        return Math.Clamp((int)Math.Floor(localX / lane.SizeR.X * (double)count), 0, count - 1);
    }

    /// <summary>Returns tooltip text for the command under the pointer inside a timeline lane.</summary>
    public ReadOnlySpan<char> Tooltip(EntMut lane)
    {
        var commands = session.Runner.Scenario.Commands;
        if (commands.Length == 0)
            return [];

        var index = HoverIndex(lane);
        var command = commands[index];
        var eventIndex = index + 1;
        return command.Kind switch
        {
            AllocatorCommandKind.Alloc => text.Format(
                "scripted event #{0}: alloc slot {1}, alignment {2}, size {3}B; {4}",
                eventIndex,
                command.Slot,
                command.Alignment,
                command.Size,
                command.Label),
            AllocatorCommandKind.Realloc => text.Format(
                "scripted event #{0}: realloc slot {1}, alignment {2}, size {3}B; {4}",
                eventIndex,
                command.Slot,
                command.Alignment,
                command.Size,
                command.Label),
            AllocatorCommandKind.Free => text.Format(
                "scripted event #{0}: free slot {1}; {2}",
                eventIndex,
                command.Slot,
                command.Label),
            AllocatorCommandKind.Pack => text.Format(
                "scripted event #{0}: pack live ranges; {1}",
                eventIndex,
                command.Label),
            _ => text.Format("scripted event #{0}: {1}", eventIndex, command.Label),
        };
    }

    private void EnsureTexture(int desiredWidth)
    {
        if (texture is not null && width == desiredWidth)
            return;

        texture?.Dispose();
        texture = new Texture2D(gl, ((uint)desiredWidth, 1u))
        {
            MinFilter = GlTextureMinFilter.LinearMipmapLinear,
            MagFilter = GlTextureMagFilter.Nearest,
            WrapS = GlTextureWrapMode.ClampToEdge,
            WrapT = GlTextureWrapMode.ClampToEdge,
        };
        pixels = new Vec4u8[desiredWidth];
        width = desiredWidth;
        scenario = null;
    }

    private void Rasterize(AllocatorScenario current, int desiredWidth, AppTimelineOverlayMode currentMode)
    {
        if (currentMode == AppTimelineOverlayMode.Commands)
            RasterizeCommands(current.Commands, desiredWidth);
        else
            RasterizeState(current, desiredWidth, currentMode);
    }

    private void RasterizeCommands(ReadOnlySpan<AllocatorCommand> commands, int desiredWidth)
    {
        if (commands.Length == 0)
        {
            pixels[0] = Pack(s.PanelInsetColor);
            return;
        }

        for (var x = 0; x < desiredWidth; x++)
        {
            var start = (int)((long)x * commands.Length / desiredWidth);
            var end = (int)Math.Min(
                commands.Length,
                Math.Max(start + 1L, (long)(x + 1) * commands.Length / desiredWidth));
            var color = Vec4.Zero;
            for (var i = start; i < end; i++)
                color += s.CommandColor(commands[i].Kind);

            pixels[x] = Pack(color / (end - start));
        }
    }

    private void RasterizeState(AllocatorScenario current, int desiredWidth, AppTimelineOverlayMode currentMode)
    {
        var commands = current.Commands;
        if (commands.Length == 0)
        {
            pixels[0] = Pack(s.PanelInsetColor);
            return;
        }

        var packCount = 0;
        var resizeCount = 0;
        var handles = new int[current.HandleSlotCount];
        var allocator = new RangeAllocator(() => packCount++, _ => resizeCount++, current.InitialSize);
        var payloadBytes = 0L;
        var reservedBytes = 0L;
        var commandIndex = 0;

        for (var x = 0; x < desiredWidth; x++)
        {
            var end = (int)Math.Min(
                commands.Length,
                Math.Max(commandIndex + 1L, (long)(x + 1) * commands.Length / desiredWidth));
            var hadPack = false;
            var hadResize = false;
            var lastKind = AllocatorCommandKind.None;
            while (commandIndex < end)
            {
                var packBefore = packCount;
                var resizeBefore = resizeCount;
                var command = commands[commandIndex++];
                lastKind = command.Kind;
                Apply(command);
                hadPack |= packCount > packBefore;
                hadResize |= resizeCount > resizeBefore;
            }

            pixels[x] = Pack(StateColor(
                currentMode,
                allocator,
                payloadBytes,
                reservedBytes,
                hadPack,
                hadResize,
                lastKind,
                handles.Length));
        }

        void Apply(AllocatorCommand command)
        {
            var packBefore = packCount;
            if (command.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc or AllocatorCommandKind.Free)
                Subtract(handles[command.Slot]);

            switch (command.Kind)
            {
                case AllocatorCommandKind.Alloc:
                case AllocatorCommandKind.Realloc:
                    allocator.Alloc(ref handles[command.Slot], command.Alignment, command.Size);
                    break;
                case AllocatorCommandKind.Free:
                    allocator.Free(handles[command.Slot]);
                    handles[command.Slot] = 0;
                    break;
                case AllocatorCommandKind.Pack:
                    allocator.Pack();
                    break;
            }

            if (packCount > packBefore)
                RecomputeTotals();
            else if (command.Kind is AllocatorCommandKind.Alloc or AllocatorCommandKind.Realloc)
                Add(handles[command.Slot]);
        }

        void Add(int handle)
        {
            if (handle == 0)
                return;

            var allocation = allocator.AllocationSlots[handle];
            payloadBytes += allocation.Size;
            reservedBytes += ReservedSize(allocation);
        }

        void Subtract(int handle)
        {
            if (handle == 0)
                return;

            var allocation = allocator.AllocationSlots[handle];
            payloadBytes -= allocation.Size;
            reservedBytes -= ReservedSize(allocation);
        }

        void RecomputeTotals()
        {
            payloadBytes = 0;
            reservedBytes = 0;
            for (var i = 0; i < handles.Length; i++)
                Add(handles[i]);
        }
    }

    private Vec4 StateColor(
        AppTimelineOverlayMode currentMode,
        RangeAllocator allocator,
        long payloadBytes,
        long reservedBytes,
        bool hadPack,
        bool hadResize,
        AllocatorCommandKind lastKind,
        int handleCount)
    {
        return currentMode switch
        {
            AppTimelineOverlayMode.Used => UsedColor(Ratio(allocator.Used, allocator.Size)),
            AppTimelineOverlayMode.Efficiency => TimelineEfficiencyColor(payloadBytes, reservedBytes, allocator.Size),
            AppTimelineOverlayMode.FreeBlocks => FreeBlockColor(allocator.FreeBlockCount, handleCount),
            AppTimelineOverlayMode.Events => EventColor(hadPack, hadResize, lastKind),
            _ => s.CommandColor(lastKind),
        };
    }

    private Vec4 UsedColor(float ratio)
    {
        if (ratio <= 0)
            return s.OverlayFreeColor;

        return ratio < 0.5f
            ? Mix(s.DensityLowColor, s.DensityMidColor, ratio * 2f)
            : Mix(s.DensityMidColor, s.DensityHighColor, (ratio - 0.5f) * 2f);
    }

    private Vec4 TimelineEfficiencyColor(long payloadBytes, long reservedBytes, long backingSize)
    {
        if (reservedBytes <= 0)
            return s.OverlayFreeColor;

        var efficiency = Ratio(payloadBytes, reservedBytes);
        var density = Ratio(reservedBytes, backingSize);
        var color = efficiency < 0.5f
            ? Mix(s.EfficiencyWasteColor, s.EfficiencyMixedColor, efficiency * 2f)
            : Mix(s.EfficiencyMixedColor, s.EfficiencyGoodColor, (efficiency - 0.5f) * 2f);
        return Mix(s.OverlayFreeColor, color, density);
    }

    private Vec4 FreeBlockColor(int freeBlockCount, int handleCount)
    {
        var scale = MathF.Log(Math.Max(2, handleCount));
        var ratio = scale <= 0 ? 0 : Math.Clamp(MathF.Log(freeBlockCount + 1) / scale, 0f, 1f);
        return ratio < 0.5f
            ? Mix(s.FragmentLargeColor, s.FragmentMediumColor, ratio * 2f)
            : Mix(s.FragmentMediumColor, s.FragmentTinyColor, (ratio - 0.5f) * 2f);
    }

    private Vec4 EventColor(bool hadPack, bool hadResize, AllocatorCommandKind lastKind)
    {
        if (hadPack && hadResize)
            return s.HighlightColor;

        if (hadResize)
            return s.WarmAccentColor;

        if (hadPack)
            return s.CommandColor(AllocatorCommandKind.Pack);

        return s.Dim(s.CommandColor(lastKind), 0.35f);
    }

    private static long ReservedSize(RangeAllocation allocation) =>
        allocation.CapacitySize + MaxPadding(allocation.Alignment);

    private static long MaxPadding(int alignment) => alignment <= 1 ? 0 : alignment - 1L;

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

    private static Vec4u8 Pack(Vec4 color) =>
        (
            PackChannel(color.X),
            PackChannel(color.Y),
            PackChannel(color.Z),
            PackChannel(color.W));

    private static byte PackChannel(float value) =>
        (byte)Math.Clamp((int)MathF.Round(value * 255f), 0, 255);
}
