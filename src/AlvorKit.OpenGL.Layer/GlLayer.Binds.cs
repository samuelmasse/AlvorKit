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
    private GlStateSlot<GlTextureUnit> activeTexture;
    private GlStateSlot<int> drawBuffersCount;
    private readonly GlBindingMap<GlBufferTarget> bufferBinds = new();
    private readonly GlBindingMap<(GlBufferTarget, uint)> indexedBufferBinds = new();
    private readonly GlBindingMap<uint> vertexBufferBinds = new();
    private readonly GlBindingMap<uint> samplerBinds = new();
    private readonly GlBindingMap<uint> imageTextureBinds = new();
    private readonly GlBindingMap<(uint, GlTextureTarget)> textureBinds = new();
    private readonly GlBindingMap<GlQueryTarget> queryBinds = new();
    private readonly GlBindingMap<(GlQueryTarget, uint)> queryIndexedBinds = new();
    private readonly Dictionary<GlTextureHandle, GlTextureTarget> textureTargets = [];

    private uint GetActiveTextureIndex(string function) =>
        activeTexture.Value is { } unit
            ? (uint)((int)unit - (int)GlTextureUnit.Texture0)
            : throw new GlMissingPrerequisiteException(function, "no active texture unit is set; call glActiveTexture first.");

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBuffer"/> for the same target.</remarks>
    public override void BindBuffer(GlBufferTarget target, GlBufferHandle buffer)
    {
        var id = (uint)buffer;
        bufferBinds.Bind(nameof(BindBuffer), target, id);
        base.BindBuffer(target, buffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBufferBase"/> for the same target and index.</remarks>
    public override void BindBufferBase(GlBufferTarget target, uint index, GlBufferHandle buffer)
    {
        bufferBinds.Bind(nameof(BindBufferBase), target, (uint)buffer);
        indexedBufferBinds.Bind(nameof(BindBufferBase), (target, index), (uint)buffer);
        base.BindBufferBase(target, index, buffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindBufferRange"/> for the same target and index.</remarks>
    public override void BindBufferRange(GlBufferTarget target, uint index, GlBufferHandle buffer, nint offset, nint size)
    {
        bufferBinds.Bind(nameof(BindBufferRange), target, (uint)buffer);
        indexedBufferBinds.Bind(nameof(BindBufferRange), (target, index), (uint)buffer);
        base.BindBufferRange(target, index, buffer, offset, size);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindVertexBuffer"/> for the same binding index.</remarks>
    public override void BindVertexBuffer(uint bindingindex, GlBufferHandle buffer, nint offset, int stride)
    {
        vertexBufferBinds.Bind(nameof(BindVertexBuffer), bindingindex, (uint)buffer);
        base.BindVertexBuffer(bindingindex, buffer, offset, stride);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindVertexArray"/>. Cannot bind a VAO while a buffer is bound to <see cref="GlBufferTarget.ArrayBuffer"/> or <see cref="GlBufferTarget.ElementArrayBuffer"/>.</remarks>
    public override void BindVertexArray(GlVertexArrayHandle array)
    {
        var id = (uint)array;
        if (id != 0)
        {
            if (bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {vbo} is still bound to ArrayBuffer.");
            if (bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out var ebo) && ebo != 0)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but buffer {ebo} is still bound to ElementArrayBuffer.");
            if (vertexBufferBinds.HasAny)
                throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to bind VAO {id}, but vertex buffer bindings are still set.");
        }
        vertexArray.Bind(nameof(BindVertexArray), id);
        base.BindVertexArray(array);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnuseProgram"/>.</remarks>
    public override void UseProgram(GlProgramHandle program)
    {
        this.program.Bind(nameof(UseProgram), (uint)program);
        base.UseProgram(program);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindProgramPipeline"/>.</remarks>
    public override void BindProgramPipeline(GlProgramPipelineHandle pipeline)
    {
        programPipeline.Bind(nameof(BindProgramPipeline), (uint)pipeline);
        base.BindProgramPipeline(pipeline);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindRenderbuffer"/>.</remarks>
    public override void BindRenderbuffer(GlRenderbufferTarget target, GlRenderbufferHandle renderbuffer)
    {
        this.renderbuffer.Bind(nameof(BindRenderbuffer), (uint)renderbuffer);
        base.BindRenderbuffer(target, renderbuffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindFramebuffer"/> for the same target.</remarks>
    public override void BindFramebuffer(GlFramebufferTarget target, GlFramebufferHandle framebuffer)
    {
        var id = (uint)framebuffer;
        ValidateFramebufferChange(nameof(BindFramebuffer), target, id, false);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.ReadFramebuffer)
            readFramebuffer.Bind(nameof(BindFramebuffer), id);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Bind(nameof(BindFramebuffer), id);
        base.BindFramebuffer(target, framebuffer);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTransformFeedback"/> for the same target.</remarks>
    public override void BindTransformFeedback(GlBindTransformFeedbackTarget target, GlTransformFeedbackHandle id)
    {
        transformFeedbackObject.Bind(nameof(BindTransformFeedback), (uint)id);
        base.BindTransformFeedback(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetActiveTexture"/>. Set-once: switching to another unit requires resetting first. <see cref="BindTexture"/>, <see cref="UnbindTexture"/>, and bound-texture allocations all require an active unit to be set.</remarks>
    public override void ActiveTexture(GlTextureUnit texture)
    {
        activeTexture.Set(nameof(ActiveTexture), texture);
        base.ActiveTexture(texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTexture"/> for the same target on the active unit. A texture cannot be used as more than one target.</remarks>
    public override void BindTexture(GlTextureTarget target, GlTextureHandle texture)
    {
        var id = (uint)texture;
        if (id != 0)
            TrackTextureTarget(nameof(BindTexture), texture, target);
        textureBinds.Bind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target), id);
        base.BindTexture(target, texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindTextureUnit"/> for the same unit.</remarks>
    public override void BindTextureUnit(uint unit, GlTextureHandle texture)
    {
        var target = GetTextureTarget(nameof(BindTextureUnit), texture);
        textureBinds.Bind(nameof(BindTextureUnit), (unit, target), (uint)texture);
        base.BindTextureUnit(unit, texture);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindSampler"/> for the same unit.</remarks>
    public override void BindSampler(uint unit, GlSamplerHandle sampler)
    {
        samplerBinds.Bind(nameof(BindSampler), unit, (uint)sampler);
        base.BindSampler(unit, sampler);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="UnbindImageTexture"/> for the same unit.</remarks>
    public override void BindImageTexture(uint unit, GlTextureHandle texture, int level, bool layered, int layer, GlBufferAccess access, GlInternalFormat format)
    {
        imageTextureBinds.Bind(nameof(BindImageTexture), unit, (uint)texture);
        base.BindImageTexture(unit, texture, level, layered, layer, access, format);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQuery</c> for the same target.</remarks>
    public override void BeginQuery(GlQueryTarget target, GlQueryHandle id)
    {
        queryBinds.Bind(nameof(BeginQuery), target, (uint)id);
        base.BeginQuery(target, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQuery</c> for the same target.</remarks>
    public override void EndQuery(GlQueryTarget target)
    {
        queryBinds.Unbind(nameof(EndQuery), target);
        base.EndQuery(target);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndQueryIndexed</c> for the same target and index.</remarks>
    public override void BeginQueryIndexed(GlQueryTarget target, uint index, GlQueryHandle id)
    {
        queryIndexedBinds.Bind(nameof(BeginQueryIndexed), (target, index), (uint)id);
        base.BeginQueryIndexed(target, index, id);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one earlier call to <c>glBeginQueryIndexed</c> for the same target and index.</remarks>
    public override void EndQueryIndexed(GlQueryTarget target, uint index)
    {
        queryIndexedBinds.Unbind(nameof(EndQueryIndexed), (target, index));
        base.EndQueryIndexed(target, index);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <c>glEndConditionalRender</c>.</remarks>
    public override void BeginConditionalRender(uint id, GlConditionalRenderMode mode)
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
    public override unsafe void BindBuffersBase(GlBufferTarget target, uint first, int count, nint buffers)
    {
        var ids = (uint*)buffers;
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersBase), target, buffers == 0 ? 0u : ids[count - 1]);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Bind(nameof(BindBuffersBase), (target, first + (uint)i), buffers == 0 ? 0u : ids[i]);
        base.BindBuffersBase(target, first, count, buffers);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Binds each buffer in <c>[first, first + count)</c>. Must be paired with exactly one later call to <see cref="UnbindBuffersRange"/> for the same target and range.</remarks>
    public override unsafe void BindBuffersRange(GlBufferTarget target, uint first, int count, nint buffers, nint offsets, nint sizes)
    {
        var ids = (uint*)buffers;
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersRange), target, buffers == 0 ? 0u : ids[count - 1]);
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
        {
            var texture = textures == 0 ? (GlTextureHandle)0u : (GlTextureHandle)ids[i];
            var target = GetTextureTarget(nameof(BindTextures), texture);
            textureBinds.Bind(nameof(BindTextures), (first + (uint)i, target), (uint)texture);
        }
        base.BindTextures(first, count, textures);
    }

    /// <inheritdoc/>
    /// <remarks>Layer: Must be paired with exactly one later call to <see cref="ResetDrawBuffers"/>. Set-once; cannot be combined with <see cref="DrawBuffers"/>.</remarks>
    public override void DrawBuffer(GlDrawBufferMode buf)
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

    /// <inheritdoc/>
    /// <remarks>Layer: validates the clear prerequisites set by this layer before forwarding to <c>glClear</c>.</remarks>
    public override void Clear(GlClearBufferMask mask)
    {
        ValidateClear(mask);
        base.Clear(mask);
    }

    private void TrackTextureTarget(string function, GlTextureHandle texture, GlTextureTarget target)
    {
        if ((uint)texture == 0)
            return;
        if (textureTargets.TryGetValue(texture, out var existing) && existing != target)
            throw new GlBindConflictException(function, $"texture {texture} is already used as {existing}, cannot use it as {target}.");
        textureTargets[texture] = target;
    }

    private GlTextureTarget GetTextureTarget(string function, GlTextureHandle texture)
    {
        if ((uint)texture == 0)
            throw new GlException(function, "cannot bind texture 0 through strict bind APIs; use the matching Unbind* method.");
        if (textureTargets.TryGetValue(texture, out var target))
            return target;
        throw new GlException(function, $"texture {texture} has no known target; bind it with glBindTexture or create it with glCreateTextures first.");
    }

    /// <summary>Layer: Unbinds <c>glBindBuffer</c> for <paramref name="target"/>. Must be paired with exactly one earlier call to <c>glBindBuffer</c> for the same target.</summary>
    public void UnbindBuffer(GlBufferTarget target)
    {
        if (target == GlBufferTarget.ElementArrayBuffer && vertexArray.Current != 0)
            throw new GlBindConflictException(nameof(BindBuffer), "attempted to unbind ElementArrayBuffer while a VAO is still bound.");
        bufferBinds.Unbind(nameof(BindBuffer), target);
        base.BindBuffer(target, (GlBufferHandle)0u);
    }
    /// <summary>Layer: Unbinds <c>glBindBufferBase</c> for <paramref name="target"/> at <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glBindBufferBase</c>.</summary>
    public void UnbindBufferBase(GlBufferTarget target, uint index) { bufferBinds.Bind(nameof(BindBufferBase), target, 0); indexedBufferBinds.Unbind(nameof(BindBufferBase), (target, index)); base.BindBufferBase(target, index, (GlBufferHandle)0u); }
    /// <summary>Layer: Unbinds <c>glBindBufferRange</c> for <paramref name="target"/> at <paramref name="index"/>. Must be paired with exactly one earlier call to <c>glBindBufferRange</c>.</summary>
    public void UnbindBufferRange(GlBufferTarget target, uint index) { bufferBinds.Bind(nameof(BindBufferRange), target, 0); indexedBufferBinds.Unbind(nameof(BindBufferRange), (target, index)); base.BindBufferRange(target, index, (GlBufferHandle)0u, 0, 0); }
    /// <summary>Layer: Unbinds <c>glBindVertexBuffer</c> for binding <paramref name="bindingindex"/>. Must be paired with exactly one earlier call to <c>glBindVertexBuffer</c>.</summary>
    public void UnbindVertexBuffer(uint bindingindex) { vertexBufferBinds.Unbind(nameof(BindVertexBuffer), bindingindex); base.BindVertexBuffer(bindingindex, (GlBufferHandle)0u, 0, 0); }
    /// <summary>Layer: Unbinds the vertex array. Must be paired with exactly one earlier call to <c>glBindVertexArray</c>.</summary>
    public void UnbindVertexArray()
    {
        if (bufferBinds.TryGet(GlBufferTarget.ArrayBuffer, out var vbo) && vbo != 0)
            throw new GlBindConflictException(nameof(BindVertexArray), $"attempted to unbind VAO, but buffer {vbo} is still bound to ArrayBuffer.");
        vertexArray.Unbind(nameof(BindVertexArray));
        if (bufferBinds.TryGet(GlBufferTarget.ElementArrayBuffer, out _))
            bufferBinds.Unbind(nameof(BindVertexArray), GlBufferTarget.ElementArrayBuffer);
        vertexBufferBinds.Clear();
        base.BindVertexArray((GlVertexArrayHandle)0u);
    }
    /// <summary>Layer: Stops using the current program. Must be paired with exactly one earlier call to <c>glUseProgram</c>.</summary>
    public void UnuseProgram() { program.Unbind(nameof(UseProgram)); base.UseProgram((GlProgramHandle)0u); }
    /// <summary>Layer: Unbinds the program pipeline. Must be paired with exactly one earlier call to <c>glBindProgramPipeline</c>.</summary>
    public void UnbindProgramPipeline() { programPipeline.Unbind(nameof(BindProgramPipeline)); base.BindProgramPipeline((GlProgramPipelineHandle)0u); }
    /// <summary>Layer: Unbinds <c>glBindRenderbuffer</c> for <paramref name="target"/>. Must be paired with exactly one earlier call to <c>glBindRenderbuffer</c>.</summary>
    public void UnbindRenderbuffer(GlRenderbufferTarget target) { renderbuffer.Unbind(nameof(BindRenderbuffer)); base.BindRenderbuffer(target, (GlRenderbufferHandle)0u); }
    /// <summary>Layer: Unbinds the sampler at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindSampler</c> for the same unit.</summary>
    public void UnbindSampler(uint unit) { samplerBinds.Unbind(nameof(BindSampler), unit); base.BindSampler(unit, (GlSamplerHandle)0u); }
    /// <summary>Layer: Unbinds <c>glBindTexture</c> for <paramref name="target"/> on the active unit. Must be paired with exactly one earlier call to <c>glBindTexture</c> for the same target.</summary>
    public void UnbindTexture(GlTextureTarget target) { textureBinds.Unbind(nameof(BindTexture), (GetActiveTextureIndex(nameof(BindTexture)), target)); base.BindTexture(target, (GlTextureHandle)0u); }
    /// <summary>Layer: Unbinds the texture at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindTextureUnit</c> for the same unit.</summary>
    public void UnbindTextureUnit(uint unit) { ResetTextureUnitBindings(nameof(BindTextureUnit), unit); base.BindTextureUnit(unit, (GlTextureHandle)0u); }
    /// <summary>Layer: Unbinds the image texture at unit <paramref name="unit"/>. Must be paired with exactly one earlier call to <c>glBindImageTexture</c> for the same unit.</summary>
    public void UnbindImageTexture(uint unit) { imageTextureBinds.Unbind(nameof(BindImageTexture), unit); base.BindImageTexture(unit, (GlTextureHandle)0u, 0, false, 0, default, default); }
    /// <summary>Layer: Returns <paramref name="target"/> to the default framebuffer. Must be paired with exactly one earlier call to <c>glBindFramebuffer</c> for the same target.</summary>
    public void UnbindFramebuffer(GlFramebufferTarget target)
    {
        ValidateFramebufferChange(nameof(BindFramebuffer), target, 0, true);
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.ReadFramebuffer)
            readFramebuffer.Unbind(nameof(BindFramebuffer));
        if (target is GlFramebufferTarget.Framebuffer or GlFramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Unbind(nameof(BindFramebuffer));
        base.BindFramebuffer(target, (GlFramebufferHandle)0u);
    }
    /// <summary>Layer: Returns <paramref name="target"/> to the default transform-feedback object. Must be paired with exactly one earlier call to <c>glBindTransformFeedback</c>.</summary>
    public void UnbindTransformFeedback(GlBindTransformFeedbackTarget target) { transformFeedbackObject.Unbind(nameof(BindTransformFeedback)); base.BindTransformFeedback(target, (GlTransformFeedbackHandle)0u); }

    /// <summary>Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersBase"/>. Must be paired with exactly one earlier call to <see cref="BindBuffersBase"/> for the same target and range.</summary>
    public unsafe void UnbindBuffersBase(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersBase), target, 0);
        for (var i = 0; i < count; i++)
            indexedBufferBinds.Unbind(nameof(BindBuffersBase), (target, first + (uint)i));
        uint* buffers = stackalloc uint[count];
        base.BindBuffersBase(target, first, count, (nint)buffers);
    }
    /// <summary>Layer: Unbinds the range of indexed buffers bound by <see cref="BindBuffersRange"/>. Must be paired with exactly one earlier call to <see cref="BindBuffersRange"/> for the same target and range.</summary>
    public unsafe void UnbindBuffersRange(GlBufferTarget target, uint first, int count)
    {
        if (count > 0)
            bufferBinds.Bind(nameof(BindBuffersRange), target, 0);
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
            ResetTextureUnitBindings(nameof(BindTextures), first + (uint)i);
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
    public void ResetDrawBuffers() { drawBuffersCount.Reset(nameof(DrawBuffer)); base.DrawBuffer(GlDrawBufferMode.ColorAttachment0); }

    private void ResetTextureUnitBindings(string function, uint unit) =>
        textureBinds.UnbindWhere(function, key => key.Item1 == unit);

    private void ValidateFramebufferChange(string function, GlFramebufferTarget target, uint framebuffer, bool unbind)
    {
        if (target != GlFramebufferTarget.ReadFramebuffer && drawBuffersCount.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but draw buffers are still set.");
        if (target != GlFramebufferTarget.ReadFramebuffer && viewport.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but viewport is still set.");
        if (target != GlFramebufferTarget.DrawFramebuffer && readBuffer.IsSet)
            throw new GlBindConflictException(function, $"attempted to {FramebufferActionName(framebuffer, unbind)}, but read buffer is still set.");
    }

    private void ValidateClear(GlClearBufferMask mask)
    {
        if ((mask & GlClearBufferMask.ColorBufferBit) != 0)
            ValidateClearColorBuffer();
        if ((mask & GlClearBufferMask.DepthBufferBit) != 0 && !clearDepth.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear depth buffer because clear depth is not set.");
        if ((mask & GlClearBufferMask.StencilBufferBit) != 0 && !clearStencil.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear stencil buffer because clear stencil is not set.");
        if (enableMap.IsSet(GlEnableCap.ScissorTest) && !scissor.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear while scissor test is enabled because no scissor box is set.");
        if (indexedEnableMap.IsSet((GlEnableCap.ScissorTest, 0)) && !scissorMap.HasAny)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear while indexed scissor test is enabled because no indexed scissor box is set.");
    }

    private void ValidateClearColorBuffer()
    {
        if (!clearColor.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear color buffer because clear color is not set.");
        if (drawFramebuffer.Current != 0 && !drawBuffersCount.IsSet)
            throw new GlMissingPrerequisiteException(nameof(Clear), "cannot clear color buffer with a draw framebuffer bound because draw buffers are not set.");
    }

    private static string FramebufferActionName(uint framebuffer, bool unbind) => unbind ? "unbind framebuffer" : $"bind framebuffer {framebuffer}";
}
