namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    private Binding vertexArray;
    private Binding program;
    private Binding programPipeline;
    private Binding renderbuffer;
    private Binding readFramebuffer = new(zeroIsValid: true);
    private Binding drawFramebuffer = new(zeroIsValid: true);
    private Binding transformFeedbackObject = new(zeroIsValid: true);
    private Binding conditionalRender;
    private TextureUnit activeTextureUnit = TextureUnit.Texture0;
    private readonly BindingMap<BufferTarget> bufferBinds = new();
    private readonly BindingMap<(BufferTarget, uint)> indexedBufferBinds = new();
    private readonly BindingMap<uint> vertexBufferBinds = new();
    private readonly BindingMap<uint> samplerBinds = new();
    private readonly BindingMap<uint> imageTextureBinds = new();
    private readonly BindingMap<(uint, TextureTarget)> textureBinds = new();
    private readonly BindingMap<uint> textureUnitBinds = new();
    private readonly BindingMap<QueryTarget> queryBinds = new();
    private readonly BindingMap<(QueryTarget, uint)> queryIndexedBinds = new();
    private readonly Dictionary<uint, TextureTarget> textureTargets = [];

    public override void BindBuffer(BufferTarget target, uint buffer)
    {
        bufferBinds.Bind(nameof(BindBuffer), target, buffer);
        base.BindBuffer(target, buffer);
    }

    public override void BindBufferBase(BufferTarget target, uint index, uint buffer)
    {
        indexedBufferBinds.Bind(nameof(BindBufferBase), (target, index), buffer);
        base.BindBufferBase(target, index, buffer);
    }

    public override void BindBufferRange(BufferTarget target, uint index, uint buffer, nint offset, nint size)
    {
        indexedBufferBinds.Bind(nameof(BindBufferRange), (target, index), buffer);
        base.BindBufferRange(target, index, buffer, offset, size);
    }

    public override void BindVertexBuffer(uint bindingindex, uint buffer, nint offset, int stride)
    {
        vertexBufferBinds.Bind(nameof(BindVertexBuffer), bindingindex, buffer);
        base.BindVertexBuffer(bindingindex, buffer, offset, stride);
    }

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

    public override void UseProgram(uint program)
    {
        this.program.Bind(nameof(UseProgram), program);
        base.UseProgram(program);
    }

    public override void BindProgramPipeline(uint pipeline)
    {
        programPipeline.Bind(nameof(BindProgramPipeline), pipeline);
        base.BindProgramPipeline(pipeline);
    }

    public override void BindRenderbuffer(RenderbufferTarget target, uint renderbuffer)
    {
        this.renderbuffer.Bind(nameof(BindRenderbuffer), renderbuffer);
        base.BindRenderbuffer(target, renderbuffer);
    }

    public override void BindFramebuffer(FramebufferTarget target, uint framebuffer)
    {
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.ReadFramebuffer)
            readFramebuffer.Bind(nameof(BindFramebuffer), framebuffer);
        if (target is FramebufferTarget.Framebuffer or FramebufferTarget.DrawFramebuffer)
            drawFramebuffer.Bind(nameof(BindFramebuffer), framebuffer);
        base.BindFramebuffer(target, framebuffer);
    }

    public override void BindTransformFeedback(BindTransformFeedbackTarget target, uint id)
    {
        transformFeedbackObject.Bind(nameof(BindTransformFeedback), id);
        base.BindTransformFeedback(target, id);
    }

    public override void ActiveTexture(TextureUnit texture)
    {
        activeTextureUnit = texture;
        base.ActiveTexture(texture);
    }

    public override void BindTexture(TextureTarget target, uint texture)
    {
        if (texture != 0)
            TrackTextureTarget(nameof(BindTexture), texture, target);
        textureBinds.Bind(nameof(BindTexture), (ActiveTextureIndex, target), texture);
        base.BindTexture(target, texture);
    }

    public override void BindTextureUnit(uint unit, uint texture)
    {
        textureUnitBinds.Bind(nameof(BindTextureUnit), unit, texture);
        base.BindTextureUnit(unit, texture);
    }

    public override void BindSampler(uint unit, uint sampler)
    {
        samplerBinds.Bind(nameof(BindSampler), unit, sampler);
        base.BindSampler(unit, sampler);
    }

    public override void BindImageTexture(uint unit, uint texture, int level, bool layered, int layer, BufferAccess access, InternalFormat format)
    {
        imageTextureBinds.Bind(nameof(BindImageTexture), unit, texture);
        base.BindImageTexture(unit, texture, level, layered, layer, access, format);
    }

    public override void BeginQuery(QueryTarget target, uint id)
    {
        queryBinds.Begin(nameof(BeginQuery), target, id);
        base.BeginQuery(target, id);
    }

    public override void EndQuery(QueryTarget target)
    {
        queryBinds.End(nameof(EndQuery), target);
        base.EndQuery(target);
    }

    public override void BeginQueryIndexed(QueryTarget target, uint index, uint id)
    {
        queryIndexedBinds.Begin(nameof(BeginQueryIndexed), (target, index), id);
        base.BeginQueryIndexed(target, index, id);
    }

    public override void EndQueryIndexed(QueryTarget target, uint index)
    {
        queryIndexedBinds.End(nameof(EndQueryIndexed), (target, index));
        base.EndQueryIndexed(target, index);
    }

    public override void BeginConditionalRender(uint id, ConditionalRenderMode mode)
    {
        conditionalRender.Begin(nameof(BeginConditionalRender), id);
        base.BeginConditionalRender(id, mode);
    }

    public override void EndConditionalRender()
    {
        conditionalRender.End(nameof(EndConditionalRender));
        base.EndConditionalRender();
    }

    private uint ActiveTextureIndex => (uint)((int)activeTextureUnit - (int)TextureUnit.Texture0);

    private void TrackTextureTarget(string function, uint texture, TextureTarget target)
    {
        if (textureTargets.TryGetValue(texture, out var existing) && existing != target)
            throw new GlBindConflictException(function, $"texture {texture} is already used as {existing}, cannot use it as {target}.");
        textureTargets[texture] = target;
    }
}
