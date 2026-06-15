namespace AlvorKit.OpenGL.Layer;

public unsafe partial class GlLayer
{
    /// <summary>Layer: deletes every object still tracked by this layer, in reverse dependency order.</summary>
    public void Dispose()
    {
        foreach (var sync in Drain(syncs)) base.DeleteSync(sync);
        DeleteAll(transformFeedbacks, base.DeleteTransformFeedbacks);
        DeleteAll(programPipelines, base.DeleteProgramPipelines);
        DeleteAll(queries, base.DeleteQueries);
        DeleteAll(vertexArrays, base.DeleteVertexArrays);
        DeleteAll(framebuffers, base.DeleteFramebuffers);
        DeleteAll(renderbuffers, base.DeleteRenderbuffers);
        DeleteAll(samplers, base.DeleteSamplers);
        DeleteAll(textures, base.DeleteTextures);
        DeleteAll(buffers, base.DeleteBuffers);
        foreach (var shader in shaders.Drain()) base.DeleteShader(shader);
        foreach (var program in programs.Drain()) base.DeleteProgram(program);
    }

    /// <summary>
    /// Deletes a native array of GL object ids.
    /// </summary>
    /// <param name="n">The number of object ids to delete.</param>
    /// <param name="ids">The native pointer to the first object id.</param>
    private delegate void PluralDelete(int n, nint ids);

    /// <summary>
    /// Deletes and drains every handle tracked by a plural resource set.
    /// </summary>
    /// <typeparam name="THandle">The typed handle stored by the resource set.</typeparam>
    /// <param name="set">The resource set to drain.</param>
    /// <param name="delete">The backend delete function for the resource family.</param>
    private static void DeleteAll<THandle>(GlResourceSet<THandle> set, PluralDelete delete) where THandle : struct
    {
        if (set.Count == 0)
            return;
        var ids = set.DrainIds();
        fixed (uint* p = ids)
            delete(ids.Length, (nint)p);
    }

    /// <summary>
    /// Removes every item from a hash set and returns a snapshot of the removed values.
    /// </summary>
    /// <typeparam name="T">The item type stored by the hash set.</typeparam>
    /// <param name="set">The set to drain.</param>
    /// <returns>The values that were present before the set was drained.</returns>
    private static T[] Drain<T>(HashSet<T> set)
    {
        var ids = new T[set.Count];
        set.CopyTo(ids);
        set.Clear();
        return ids;
    }
}
