namespace AlvorKit.Engine;

/// <summary>GPU array buffer backed by an <see cref="Allocator"/> that can compact and resize itself.</summary>
[ExcludeFromCodeCoverage(Justification = "Moves bytes between live OpenGL buffers.")]
public class SharedVertexBuffer
{
    private readonly GlLayer gl;
    private readonly Allocator allocator;
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
    public Allocator Allocator => allocator;

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

    private void PackCallback()
    {
        var copyVbo = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.CopyWriteBuffer, copyVbo);
        gl.BufferData(GlBufferTarget.CopyWriteBuffer, (nint)size, 0, GlBufferUsage.DynamicCopy);
        gl.BindBuffer(GlBufferTarget.CopyReadBuffer, vbo);
        gl.CopyBufferSubData(GlCopyBufferSubDataTarget.CopyReadBuffer, GlCopyBufferSubDataTarget.CopyWriteBuffer, 0, 0, (nint)size);

        var liveAllocations = allocator.Allocations;
        var allocationSlots = allocator.AllocationSlots;
        var lastAllocationSlots = allocator.LastAllocationSlots;
        foreach (var allocation in liveAllocations)
        {
            var last = lastAllocationSlots[allocation];
            var current = allocationSlots[allocation];
            gl.CopyBufferSubData(
                GlCopyBufferSubDataTarget.CopyWriteBuffer,
                GlCopyBufferSubDataTarget.CopyReadBuffer,
                (nint)allocator.AlignedAddr(last.Index, last.Alignment),
                (nint)allocator.AlignedAddr(current.Index, current.Alignment),
                (nint)current.Size);
        }

        gl.UnbindBuffer(GlBufferTarget.CopyReadBuffer);
        gl.UnbindBuffer(GlBufferTarget.CopyWriteBuffer);
        gl.DeleteBuffer(copyVbo);
    }

    private void ResizeCallback(long newSize)
    {
        var copyVbo = gl.GenBuffer();
        gl.BindBuffer(GlBufferTarget.CopyWriteBuffer, copyVbo);
        gl.BufferData(GlBufferTarget.CopyWriteBuffer, (nint)size, 0, GlBufferUsage.DynamicCopy);
        gl.BindBuffer(GlBufferTarget.CopyReadBuffer, vbo);
        gl.CopyBufferSubData(GlCopyBufferSubDataTarget.CopyReadBuffer, GlCopyBufferSubDataTarget.CopyWriteBuffer, 0, 0, (nint)size);
        gl.BufferData(GlBufferTarget.CopyReadBuffer, (nint)newSize, 0, GlBufferUsage.DynamicDraw);
        gl.CopyBufferSubData(GlCopyBufferSubDataTarget.CopyWriteBuffer, GlCopyBufferSubDataTarget.CopyReadBuffer, 0, 0, (nint)size);
        gl.UnbindBuffer(GlBufferTarget.CopyReadBuffer);
        gl.UnbindBuffer(GlBufferTarget.CopyWriteBuffer);
        gl.DeleteBuffer(copyVbo);
        size = newSize;
    }
}
