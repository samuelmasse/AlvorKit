namespace AlvorKit.Engine;

/// <summary>GPU array buffer backed by a <see cref="RangeAllocator"/> that can compact and resize itself.</summary>
[ExcludeFromCodeCoverage(Justification = "Moves bytes between live OpenGL buffers.")]
public class SharedVertexBuffer
{
    private readonly GlLayer gl;
    private readonly RangeAllocator allocator;
    private readonly GlBufferHandle vbo;
    private long size;

    /// <summary>Creates the shared vertex buffer in the supplied GL layer.</summary>
    public SharedVertexBuffer(GlLayer gl)
    {
        this.gl = gl;
        allocator = new(PackCallback, ResizeCallback);
        vbo = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, vbo);
        gl.BufferData(GlBufferTarget.ArrayBuffer, (nint)allocator.Size, 0, GlBufferUsage.DynamicDraw);
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        size = allocator.Size;
    }

    /// <summary>Gets the backing allocator.</summary>
    public RangeAllocator Allocator => allocator;

    /// <summary>Gets the tracked vertex buffer object handle.</summary>
    public GlBufferHandle Vbo => vbo;

    /// <summary>Gets the backing buffer size in bytes.</summary>
    public long Size => size;

    /// <summary>Allocates or resizes a logical region in the backing buffer.</summary>
    public void Alloc(ref int allocation, int alignment, long allocSize) => allocator.Alloc(ref allocation, alignment, allocSize);

    /// <summary>Returns the aligned byte address for a logical allocation.</summary>
    public long Addr(int allocation) => allocator.Addr(allocation);

    /// <summary>Frees a logical allocation.</summary>
    public void Free(int allocation) => allocator.Free(allocation);

    private unsafe void PackCallback()
    {
        gl.BindBuffer(GlBufferTarget.CopyReadBuffer, vbo);
        var snapshot = System.Runtime.InteropServices.NativeMemory.Alloc((nuint)size);
        try
        {
            gl.GetBufferSubData(GlBufferTarget.CopyReadBuffer, 0, (nint)size, (nint)snapshot);

            var liveAllocations = allocator.Allocations;
            var allocationSlots = allocator.AllocationSlots;
            var lastAllocationSlots = allocator.LastAllocationSlots;
            foreach (var allocation in liveAllocations)
            {
                var last = lastAllocationSlots[allocation];
                var current = allocationSlots[allocation];
                var source = (byte*)snapshot + allocator.AlignedAddr(last.Index, last.Alignment);
                gl.BufferSubData(
                    GlBufferTarget.CopyReadBuffer,
                    (nint)allocator.AlignedAddr(current.Index, current.Alignment),
                    (nint)current.Size,
                    (nint)source);
            }
        }
        finally
        {
            System.Runtime.InteropServices.NativeMemory.Free(snapshot);
            gl.UnbindBuffer(GlBufferTarget.CopyReadBuffer);
        }
    }

    private unsafe void ResizeCallback(long newSize)
    {
        gl.BindBuffer(GlBufferTarget.CopyReadBuffer, vbo);
        var snapshot = System.Runtime.InteropServices.NativeMemory.Alloc((nuint)size);
        try
        {
            gl.GetBufferSubData(GlBufferTarget.CopyReadBuffer, 0, (nint)size, (nint)snapshot);
            gl.BufferData(GlBufferTarget.CopyReadBuffer, (nint)newSize, 0, GlBufferUsage.DynamicDraw);
            gl.BufferSubData(GlBufferTarget.CopyReadBuffer, 0, (nint)size, (nint)snapshot);
        }
        finally
        {
            System.Runtime.InteropServices.NativeMemory.Free(snapshot);
            gl.UnbindBuffer(GlBufferTarget.CopyReadBuffer);
        }
        size = newSize;
    }
}
