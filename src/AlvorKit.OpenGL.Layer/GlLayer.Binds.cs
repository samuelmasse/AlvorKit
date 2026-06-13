namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    private GlBinding vertexArray;
    private GlBinding program;
    private GlBinding programPipeline;
    private GlBinding renderbuffer;
    private GlBinding readFramebuffer;
    private GlBinding drawFramebuffer;
    private GlBinding transformFeedbackObject;
    private GlBinding conditionalRender;
    private GlStateSlot<TextureUnit> activeTexture;
    private GlStateSlot<int> drawBuffersCount;
    private readonly GlBindingMap<BufferTarget> bufferBinds = new();
    private readonly GlBindingMap<(BufferTarget, uint)> indexedBufferBinds = new();
    private readonly GlBindingMap<uint> vertexBufferBinds = new();
    private readonly GlBindingMap<uint> samplerBinds = new();
    private readonly GlBindingMap<uint> imageTextureBinds = new();
    private readonly GlBindingMap<(uint, TextureTarget)> textureBinds = new();
    private readonly GlBindingMap<uint> textureUnitBinds = new();
    private readonly GlBindingMap<QueryTarget> queryBinds = new();
    private readonly GlBindingMap<(QueryTarget, uint)> queryIndexedBinds = new();
    private readonly Dictionary<uint, TextureTarget> textureTargets = [];

    private uint GetActiveTextureIndex(string function) =>
        activeTexture.Value is { } unit
            ? (uint)((int)unit - (int)TextureUnit.Texture0)
            : throw new GlMissingPrerequisiteException(function, "no active texture unit is set; call glActiveTexture first.");

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBuffer"/> for the same target.</remarks>
    public override void BindBuffer(BufferTarget target, uint buffer)
    {
        bufferBinds.Bind(nameof(BindBuffer), target, buffer);
        base.BindBuffer(target, buffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBufferBase"/> for the same target and index.</remarks>
    public override void BindBufferBase(BufferTarget target, uint index, uint buffer)
    {
        indexedBufferBinds.Bind(nameof(BindBufferBase), (target, index), buffer);
        base.BindBufferBase(target, index, buffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBufferRange"/> for the same target and index.</remarks>
    public override void BindBufferRange(BufferTarget target, uint index, uint buffer, nint offset, nint size)
    {
        indexedBufferBinds.Bind(nameof(BindBufferRange), (target, index), buffer);
        base.BindBufferRange(target, index, buffer, offset, size);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindVertexBuffer"/> for the same binding index.</remarks>
    public override void BindVertexBuffer(uint bindingindex, uint buffer, nint offset, int stride)
    {
        vertexBufferBinds.Bind(nameof(BindVertexBuffer), bindingindex, buffer);
        base.BindVertexBuffer(bindingindex, buffer, offset, stride);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindVertexArray"/>. Cannot bind a VAO while a buffer is bound to <see cref="BufferTarget.ArrayBuffer"/> or <see cref="BufferTarget.ElementArrayBuffer"/>.</remarks>
    public override void BindVertexArray(uint array)
    {
        if (array != 0)
        {
            if (bufferBinds.TryGet(BufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {array}, but buffer {vbo} is still bound to ArrayBuffer.");
            if (bufferBinds.TryGet(BufferTarget.ElementArrayBuffer, out var ebo) && ebo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {array}, but buffer {ebo} is still bound to ElementArrayBuffer.");
        }
        vertexArray.Bind(nameof(BindVertexArray), array);
        base.BindVertexArray(array);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnuseProgram"/>.</remarks>
    public override void UseProgram(uint program)
    {
        this.program.Bind(nameof(UseProgram), program);
        base.UseProgram(program);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindProgramPipeline"/>.</remarks>
    public override void BindProgramPipeline(uint pipeline)
    {
        programPipeline.Bind(nameof(BindProgramPipeline), pipeline);
        base.BindProgramPipeline(pipeline);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindRenderbuffer"/>.</remarks>
    public override void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer)
    {
        this.renderbuffer.Bind(nameof(BindRenderbuffer), renderbuffer);
        base.BindRenderbuffer(target, renderbuffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindFramebuffer"/> for the same target.</remarks>
    public override void BindFramebuffer(FramebufferTarget target, uint framebuffer)
    {
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.ReadFramebuffer)
            readFramebuffer.Bind(nameof(BindFramebuffer), framebuffer);
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Bind(nameof(BindFramebuffer), framebuffer);
        base.BindFramebuffer(target, framebuffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTransformFeedback"/> for the same target.</remarks>
    public override void BindTransformFeedback(BindTransformFeedbackTarget target, uint id)
    {
        transformFeedbackObject.Bind(nameof(BindTransformFeedback), id);
        base.BindTransformFeedback(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetActiveTexture"/>. Set-once: switching to another unit requires resetting first. <see cref="BindTexture"/>, <see cref="UnbindTexture"/>, and bound-texture allocations all require an active unit to be set.</remarks>
    public override void ActiveTexture(TextureUnit texture)
    {
        activeTexture.Set(nameof(ActiveTexture), texture);
        base.ActiveTexture(texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTexture"/> for the same target on the active unit. A texture cannot be used as more than one target.</remarks>
    public override void BindTexture(TextureTarget target, uint texture)
    {
        if (texture != 0)
            TrackTextureTarget(nameof(BindTexture), texture, target);
        textureBinds.Bind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target), texture);
        base.BindTexture(target, texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTextureUnit"/> for the same unit.</remarks>
    public override void BindTextureUnit(uint unit, uint texture)
    {
        textureUnitBinds.Bind(nameof(BindTextureUnit), unit, texture);
        base.BindTextureUnit(unit, texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindSampler"/> for the same unit.</remarks>
    public override void BindSampler(uint unit, uint sampler)
    {
        samplerBinds.Bind(nameof(BindSampler), unit, sampler);
        base.BindSampler(unit, sampler);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindImageTexture"/> for the same unit.</remarks>
    public override void BindImageTexture(uint unit, uint texture, int level, bool layered, int layer, BufferAccess access, InternalFormat format)
    {
        imageTextureBinds.Bind(nameof(BindImageTexture), unit, texture);
        base.BindImageTexture(unit, texture, level, layered, layer, access, format);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQuery</c> for the same target.</remarks>
    public override void BeginQuery(QueryTarget target, uint id)
    {
        queryBinds.Bind(nameof(BeginQuery), target, id);
        base.BeginQuery(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQuery</c> for the same target.</remarks>
    public override void EndQuery(QueryTarget target)
    {
        queryBinds.Unbind(nameof(EndQuery), target);
        base.EndQuery(target);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQueryIndexed</c> for the same target and index.</remarks>
    public override void BeginQueryIndexed(QueryTarget target, uint index, uint id)
    {
        queryIndexedBinds.Bind(nameof(BeginQueryIndexed), (target, index), id);
        base.BeginQueryIndexed(target, index, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQueryIndexed</c> for the same target and index.</remarks>
    public override void EndQueryIndexed(QueryTarget target, uint index)
    {
        queryIndexedBinds.Unbind(nameof(EndQueryIndexed), (target, index));
        base.EndQueryIndexed(target, index);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndConditionalRender</c>.</remarks>
    public override void BeginConditionalRender(uint id, ConditionalRenderMode mode)
    {
        conditionalRender.Bind(nameof(BeginConditionalRender), id);
        base.BeginConditionalRender(id, mode);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginConditionalRender</c>.</remarks>
    public override void EndConditionalRender()
    {
        conditionalRender.Unbind(nameof(EndConditionalRender));
        base.EndConditionalRender();
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindBuffersBase"/> for the same target and range.</remarks>
    public override unsafe void BindBuffersBase(BufferTarget target, uint first, int count, nint buffers)
    {
        var ids = (uint*)buffers;
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Bind(nameof(BindBuffersBase), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersBase(target, first, count, buffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindBuffersRange"/> for the same target and range.</remarks>
    public override unsafe void BindBuffersRange(BufferTarget target, uint first, int count, nint buffers, nint offsets, nint sizes)
    {
        var ids = (uint*)buffers;
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Bind(nameof(BindBuffersRange), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersRange(target, first, count, buffers, offsets, sizes);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each buffer to vertex binding points <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindVertexBuffers"/> for the same range.</remarks>
    public override unsafe void BindVertexBuffers(uint first, int count, nint buffers, nint offsets, nint strides)
    {
        var ids = (uint*)buffers;
        for (var i = 0; i < count; i++)
            vertexBufferBinds.Bind(nameof(BindVertexBuffers), first + (uint)i, buffers == 0 ? 0u : ids[i]);
        base.BindVertexBuffers(first, count, buffers, offsets, strides);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each sampler to texture units <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindSamplers"/> for the same range.</remarks>
    public override unsafe void BindSamplers(uint first, int count, nint samplers)
    {
        var ids = (uint*)samplers;
        for (var i = 0; i < count; i++)
            samplerBinds.Bind(nameof(BindSamplers), first + (uint)i, samplers == 0 ? 0u : ids[i]);
        base.BindSamplers(first, count, samplers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each texture to image units <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindImageTextures"/> for the same range.</remarks>
    public override unsafe void BindImageTextures(uint first, int count, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < count; i++)
            imageTextureBinds.Bind(nameof(BindImageTextures), first + (uint)i, textures == 0 ? 0u : ids[i]);
        base.BindImageTextures(first, count, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each texture to units <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindTextures"/> for the same range.</remarks>
    public override unsafe void BindTextures(uint first, int count, nint textures)
    {
        var ids = (uint*)textures;
        for (var i = 0; i < count; i++)
            textureUnitBinds.Bind(nameof(BindTextures), first + (uint)i, textures == 0 ? 0u : ids[i]);
        base.BindTextures(first, count, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>. Set-once; cannot be combined with <see cref="DrawBuffers"/>.</remarks>
    public override void DrawBuffer(DrawBufferMode buf)
    {
        drawBuffersCount.Set(nameof(DrawBuffer), 1);
        base.DrawBuffer(buf);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>. Set-once; cannot be combined with <see cref="DrawBuffer"/>.</remarks>
    public override void DrawBuffers(int n, nint bufs)
    {
        drawBuffersCount.Set(nameof(DrawBuffers), n);
        base.DrawBuffers(n, bufs);
    }

    private void TrackTextureTarget(string function, uint texture, TextureTarget target)
    {
        if (textureTargets.TryGetValue(texture, out var existing) && existing != target)
            throw new GlBindConflictException(function, $"texture {texture} is already used as {existing}, cannot use it as {target}.");
        textureTargets[texture] = target;
    }

    /// <summary>Layer: Unbinds <c>glBindBuffer</c> for <paramref name="target"/>. Must be paired with exactly one earlier call to <c>glBindBuffer</c> for the same target.</summary>
    public void UnbindBuffer(BufferTarget target) { bufferBinds.Unbind(nameof(BindBuffer), target); base.BindBuffer(target, 0); }
    /// <summary>Layer: Unbinds <c>glBindBufferBase</c> for <paramref name="target"/> at <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glBindBufferBase</c>.</summary>
    public void UnbindBufferBase(BufferTarget target, uint index) { indexedBufferBinds.Unbind(nameof(BindBufferBase), (target, index)); base.BindBufferBase(target, index, 0); }
    /// <summary>Layer: Unbinds <c>glBindBufferRange</c> for <paramref name="target"/> at <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glBindBufferRange</c>.</summary>
    public void UnbindBufferRange(BufferTarget target, uint index) { indexedBufferBinds.Unbind(nameof(BindBufferRange), (target, index)); base.BindBufferRange(target, index, 0, 0, 0); }
    /// <summary>Layer: Unbinds <c>glBindVertexBuffer</c> for binding <paramref name="bindingindex"/>. Must be paired with exactly one earlier call to <c>glBindVertexBuffer</c>.</summary>
    public void UnbindVertexBuffer(uint bindingindex) { vertexBufferBinds.Unbind(nameof(BindVertexBuffer), bindingindex); base.BindVertexBuffer(bindingindex, 0, 0, 0); }
    /// <summary>Layer: Unbinds thevertex array. Must be paired with exactly one earlier call to <c>glBindVertexArray</c>.</summary>
    public void UnbindVertexArray() { vertexArray.Unbind(nameof(BindVertexArray)); base.BindVertexArray(0); }
    /// <summary>Layer: Stops using the current program. Must be paired with exactly one earlier call to <c>glUseProgram</c>.</summary>
    public void UnuseProgram() { program.Unbind(nameof(UseProgram)); base.UseProgram(0); }
    /// <summary>Layer: Unbinds theprogram pipeline. Must be paired with exactly one earlier call to <c>glBindProgramPipeline</c>.</summary>
    public void UnbindProgramPipeline() { programPipeline.Unbind(nameof(BindProgramPipeline)); base.BindProgramPipeline(0); }
    /// <summary>Layer: Unbinds <c>glBindRenderbuffer</c> for <paramref name="target"/>. Must be paired with exactly one earlier call to <c>glBindRenderbuffer</c>.</summary>
    public void UnbindRenderbuffer(RenderbufferTarget target) { renderbuffer.Unbind(nameof(BindRenderbuffer)); base.BindRenderbuffer(target, 0); }
    /// <summary>Layer: Unbinds thesampler at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindSampler</c> for the same unit.</summary>
    public void UnbindSampler(uint unit) { samplerBinds.Unbind(nameof(BindSampler), unit); base.BindSampler(unit, 0); }
    /// <summary>Layer: Unbinds <c>glBindTexture</c> for <paramref name="target"/> on the active unit. Must be paired with exactly one earlier call to <c>glBindTexture</c> for the same target.</summary>
    public void UnbindTexture(TextureTarget target) { textureBinds.Unbind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target)); base.BindTexture(target, 0); }
    /// <summary>Layer: Unbinds thetexture at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindTextureUnit</c> for the same unit.</summary>
    public void UnbindTextureUnit(uint unit) { textureUnitBinds.Unbind(nameof(BindTextureUnit), unit); base.BindTextureUnit(unit, 0); }
    /// <summary>Layer: Unbinds theimage texture at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindImageTexture</c> for the same unit.</summary>
    public void UnbindImageTexture(uint unit) { imageTextureBinds.Unbind(nameof(BindImageTexture), unit); base.BindImageTexture(unit, 0, 0, false, 0, default, default); }
    /// <summary>Layer: Returns <paramref name="target"/> to the default framebuffer. Must be paired with exactly one earlier call to <c>glBindFramebuffer</c> for the same target.</summary>
    public void UnbindFramebuffer(FramebufferTarget target)
    {
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.ReadFramebuffer)
            readFramebuffer.Unbind(nameof(BindFramebuffer));
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Unbind(nameof(BindFramebuffer));
        base.BindFramebuffer(target, 0);
    }
    /// <summary>Layer: Returns <paramref name="target"/> to the default transform-feedback object. Must be paired with exactly one earlier call to <c>glBindTransformFeedback</c>.</summary>
    public void UnbindTransformFeedback(BindTransformFeedbackTarget target) { transformFeedbackObject.Unbind(nameof(BindTransformFeedback)); base.BindTransformFeedback(target, 0); }

    /// <summary>Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersBase"/>. Must be paired with exactly one earlier call to <see cref="BindBuffersBase"/> for the same target and range.</summary>
    public unsafe void UnbindBuffersBase(BufferTarget target, uint first, int count)
    {
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Unbind(nameof(BindBuffersBase), (target, first + (uint)i));
        uint* buffers = stackalloc uint[count];
        base.BindBuffersBase(target, first, count, (nint)buffers);
    }
    /// <summary>Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersRange"/>. Must be paired with exactly one earlier call to <see cref="BindBuffersRange"/> for the same target and range.</summary>
    public unsafe void UnbindBuffersRange(BufferTarget target, uint first, int count)
    {
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Unbind(nameof(BindBuffersRange), (target, first + (uint)i));
        uint* buffers = stackalloc uint[count];
        nint* offsets = stackalloc nint[count];
        nint* sizes = stackalloc nint[count];
        base.BindBuffersRange(target, first, count, (nint)buffers, (nint)offsets, (nint)sizes);
    }
    /// <summary>Layer: Unbinds the range of vertex buffers bound by <see cref="BindVertexBuffers"/>. Must be paired with exactly one earlier call to <see cref="BindVertexBuffers"/> for the same range.</summary>
    public unsafe void UnbindVertexBuffers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            vertexBufferBinds.Unbind(nameof(BindVertexBuffers), first + (uint)i);
        uint* buffers = stackalloc uint[count];
        nint* offsets = stackalloc nint[count];
        int* strides = stackalloc int[count];
        base.BindVertexBuffers(first, count, (nint)buffers, (nint)offsets, (nint)strides);
    }
    /// <summary>Layer: Unbinds the range of samplers bound by <see cref="BindSamplers"/>. Must be paired with exactly one earlier call to <see cref="BindSamplers"/> for the same range.</summary>
    public unsafe void UnbindSamplers(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            samplerBinds.Unbind(nameof(BindSamplers), first + (uint)i);
        uint* samplers = stackalloc uint[count];
        base.BindSamplers(first, count, (nint)samplers);
    }
    /// <summary>Layer: Unbinds the range of textures bound by <see cref="BindTextures"/>. Must be paired with exactly one earlier call to <see cref="BindTextures"/> for the same range.</summary>
    public unsafe void UnbindTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            textureUnitBinds.Unbind(nameof(BindTextures), first + (uint)i);
        uint* textures = stackalloc uint[count];
        base.BindTextures(first, count, (nint)textures);
    }
    /// <summary>Layer: Unbinds the range of image textures bound by <see cref="BindImageTextures"/>. Must be paired with exactly one earlier call to <see cref="BindImageTextures"/> for the same range.</summary>
    public unsafe void UnbindImageTextures(uint first, int count)
    {
        for (var i = 0; i < count; i++)
            imageTextureBinds.Unbind(nameof(BindImageTextures), first + (uint)i);
        uint* textures = stackalloc uint[count];
        base.BindImageTextures(first, count, (nint)textures);
    }
    /// <summary>Layer: Restores the default draw buffer (<c>glDrawBuffer(ColorAttachment0)</c>). Must be paired with exactly one earlier call to <see cref="DrawBuffer"/> or <see cref="DrawBuffers"/>.</summary>
    public void ResetDrawBuffers() { drawBuffersCount.Reset(nameof(DrawBuffer)); base.DrawBuffer(DrawBufferMode.ColorAttachment0); }
}
