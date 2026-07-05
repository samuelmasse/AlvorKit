namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>The maximum number of raw object ids deleted by one stack-buffered disposal batch.</summary>
    private const int DeleteBatchSize = 128;

    /// <summary>
    /// Layer: disposes child nodes (newest first), then deletes every object still tracked by this
    /// node, in reverse dependency order. A child node routes its deletes through the strict
    /// <c>glDelete*</c> validation, so its objects must already be unbound; the root drains its
    /// remaining objects straight to the backend as whole-context teardown.
    /// </summary>
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;

        while (children.Count > 0)
            children[^1].Dispose();

        if (parent is null)
        {
            DisposeRoot();
            return;
        }

        DisposeOwned();
        parent.children.Remove(this);
    }

    /// <summary>Deletes every object tracked by the root straight on the backend.</summary>
    private void DisposeRoot()
    {
        while (TryDrain(syncs, out var sync)) Inner.DeleteSync(sync);
        DeleteAll(transformFeedbacks, PluralResourceKind.TransformFeedback);
        DeleteAll(programPipelines, PluralResourceKind.ProgramPipeline);
        DeleteAll(queries, PluralResourceKind.Query);
        DeleteAll(vertexArrays, PluralResourceKind.VertexArray);
        DeleteAll(framebuffers, PluralResourceKind.Framebuffer);
        DeleteAll(renderbuffers, PluralResourceKind.Renderbuffer);
        DeleteAll(samplers, PluralResourceKind.Sampler);
        DeleteAll(textures, PluralResourceKind.Texture);
        DeleteAll(buffers, PluralResourceKind.Buffer);
        while (shaders.TryDrain(out var shader)) Inner.DeleteShader(shader);
        while (programs.TryDrain(out var program)) Inner.DeleteProgram(program);
    }

    /// <summary>
    /// Deletes every object tracked by this child node through the strict validated delete calls,
    /// so shared bind, state, and memory tracking stay coherent for the rest of the hierarchy.
    /// </summary>
    private void DisposeOwned()
    {
        while (TryDrain(syncs, out var sync)) base.DeleteSync(sync);
        DeleteAllValidated(transformFeedbacks, PluralResourceKind.TransformFeedback);
        DeleteAllValidated(programPipelines, PluralResourceKind.ProgramPipeline);
        DeleteAllValidated(queries, PluralResourceKind.Query);
        DeleteAllValidated(vertexArrays, PluralResourceKind.VertexArray);
        DeleteAllValidated(framebuffers, PluralResourceKind.Framebuffer);
        DeleteAllValidated(renderbuffers, PluralResourceKind.Renderbuffer);
        DeleteAllValidated(samplers, PluralResourceKind.Sampler);
        DeleteAllValidated(textures, PluralResourceKind.Texture);
        DeleteAllValidated(buffers, PluralResourceKind.Buffer);
        while (shaders.TryPeek(out var shader)) DeleteShader(shader);
        while (programs.TryPeek(out var program)) DeleteProgram(program);
    }

    /// <summary>
    /// Deletes and drains every handle tracked by a resource set through this layer's validated
    /// plural delete overrides.
    /// </summary>
    /// <typeparam name="THandle">The typed handle stored by the resource set.</typeparam>
    /// <param name="set">The resource set to drain.</param>
    /// <param name="kind">The delete function category for the resource family.</param>
    private void DeleteAllValidated<THandle>(GlResourceSet<THandle> set, PluralResourceKind kind) where THandle : unmanaged
    {
        Span<uint> ids = stackalloc uint[DeleteBatchSize];
        int count;
        while ((count = set.SnapshotIds(ids)) > 0)
        {
            fixed (uint* p = ids)
                DeleteBatchValidated(kind, count, (nint)p);
        }
    }

    /// <summary>
    /// Forwards one disposal batch to the matching validated plural delete override.
    /// </summary>
    /// <param name="kind">The delete function category.</param>
    /// <param name="count">The number of object ids to delete.</param>
    /// <param name="ids">The native pointer to the first raw object id.</param>
    private void DeleteBatchValidated(PluralResourceKind kind, int count, nint ids)
    {
        switch (kind)
        {
            case PluralResourceKind.TransformFeedback:
                DeleteTransformFeedbacks(count, ids);
                break;
            case PluralResourceKind.ProgramPipeline:
                DeleteProgramPipelines(count, ids);
                break;
            case PluralResourceKind.Query:
                DeleteQueries(count, ids);
                break;
            case PluralResourceKind.VertexArray:
                DeleteVertexArrays(count, ids);
                break;
            case PluralResourceKind.Framebuffer:
                DeleteFramebuffers(count, ids);
                break;
            case PluralResourceKind.Renderbuffer:
                DeleteRenderbuffers(count, ids);
                break;
            case PluralResourceKind.Sampler:
                DeleteSamplers(count, ids);
                break;
            case PluralResourceKind.Texture:
                DeleteTextures(count, ids);
                break;
            case PluralResourceKind.Buffer:
                DeleteBuffers(count, ids);
                break;
        }
    }

    /// <summary>
    /// Identifies which backend plural delete function receives a disposal batch.
    /// </summary>
    private enum PluralResourceKind
    {
        /// <summary>Transform feedback objects.</summary>
        TransformFeedback,

        /// <summary>Program pipeline objects.</summary>
        ProgramPipeline,

        /// <summary>Query objects.</summary>
        Query,

        /// <summary>Vertex array objects.</summary>
        VertexArray,

        /// <summary>Framebuffer objects.</summary>
        Framebuffer,

        /// <summary>Renderbuffer objects.</summary>
        Renderbuffer,

        /// <summary>Sampler objects.</summary>
        Sampler,

        /// <summary>Texture objects.</summary>
        Texture,

        /// <summary>Buffer objects.</summary>
        Buffer,
    }

    /// <summary>
    /// Deletes and drains every handle tracked by a plural resource set without managed allocations.
    /// </summary>
    /// <typeparam name="THandle">The typed handle stored by the resource set.</typeparam>
    /// <param name="set">The resource set to drain.</param>
    /// <param name="kind">The backend delete function category for the resource family.</param>
    private void DeleteAll<THandle>(GlResourceSet<THandle> set, PluralResourceKind kind) where THandle : unmanaged
    {
        Span<uint> ids = stackalloc uint[DeleteBatchSize];
        int count;
        while ((count = set.DrainIds(ids)) > 0)
        {
            fixed (uint* p = ids)
                DeleteBatch(kind, count, (nint)p);
        }
    }

    /// <summary>
    /// Forwards one disposal batch to the matching raw backend delete function.
    /// </summary>
    /// <param name="kind">The backend delete function category.</param>
    /// <param name="count">The number of object ids to delete.</param>
    /// <param name="ids">The native pointer to the first raw object id.</param>
    private void DeleteBatch(PluralResourceKind kind, int count, nint ids)
    {
        switch (kind)
        {
            case PluralResourceKind.TransformFeedback:
                Inner.DeleteTransformFeedbacks(count, ids);
                break;
            case PluralResourceKind.ProgramPipeline:
                Inner.DeleteProgramPipelines(count, ids);
                break;
            case PluralResourceKind.Query:
                Inner.DeleteQueries(count, ids);
                break;
            case PluralResourceKind.VertexArray:
                Inner.DeleteVertexArrays(count, ids);
                break;
            case PluralResourceKind.Framebuffer:
                Inner.DeleteFramebuffers(count, ids);
                break;
            case PluralResourceKind.Renderbuffer:
                Inner.DeleteRenderbuffers(count, ids);
                break;
            case PluralResourceKind.Sampler:
                Inner.DeleteSamplers(count, ids);
                break;
            case PluralResourceKind.Texture:
                Inner.DeleteTextures(count, ids);
                break;
            case PluralResourceKind.Buffer:
                Inner.DeleteBuffers(count, ids);
                break;
        }
    }

    /// <summary>
    /// Removes one item from a hash set without allocating a snapshot.
    /// </summary>
    /// <typeparam name="T">The item type stored by the hash set.</typeparam>
    /// <param name="set">The set to drain.</param>
    /// <param name="value">The value that was removed, or the default value when the set is empty.</param>
    /// <returns><see langword="true"/> when a value was removed; otherwise, <see langword="false"/>.</returns>
    private static bool TryDrain<T>(HashSet<T> set, out T value)
    {
        var found = false;
        value = default!;
        foreach (var candidate in set)
        {
            value = candidate;
            found = true;
            break;
        }
        if (!found)
            return false;
        set.Remove(value);
        return true;
    }
}
