namespace AlvorKit.OpenGL.Layer;

public partial class GlLayer
{
    /// <summary>Tracks the currently bound vertex array object.</summary>
    private GlBinding vertexArray;

    /// <summary>Tracks the currently used program object.</summary>
    private GlBinding program;

    /// <summary>Tracks the currently bound program pipeline object.</summary>
    private GlBinding programPipeline;

    /// <summary>Tracks the currently bound renderbuffer object.</summary>
    private GlBinding renderbuffer;

    /// <summary>Tracks the currently bound read framebuffer object.</summary>
    private GlBinding readFramebuffer;

    /// <summary>Tracks the currently bound draw framebuffer object.</summary>
    private GlBinding drawFramebuffer;

    /// <summary>Tracks the currently bound transform feedback object.</summary>
    private GlBinding transformFeedbackObject;

    /// <summary>Tracks the currently active conditional render scope.</summary>
    private GlBinding conditionalRender;

    /// <summary>Tracks the active texture unit set through <c>glActiveTexture</c>.</summary>
    private GlStateSlot<GlTextureUnit> activeTexture;

    /// <summary>Tracks whether draw buffers are configured for the current framebuffer.</summary>
    private GlStateSlot<int> drawBuffersCount;

    /// <summary>Tracks generic buffer bindings by target.</summary>
    private readonly GlBindingMap<GlBufferTarget> bufferBinds = new();

    /// <summary>Tracks indexed buffer bindings by target and index.</summary>
    private readonly GlBindingMap<(GlBufferTarget, uint)> indexedBufferBinds = new();

    /// <summary>Tracks vertex buffer bindings by binding index.</summary>
    private readonly GlBindingMap<uint> vertexBufferBinds = new();

    /// <summary>Tracks sampler bindings by texture unit.</summary>
    private readonly GlBindingMap<uint> samplerBinds = new();

    /// <summary>Tracks image texture bindings by image unit.</summary>
    private readonly GlBindingMap<uint> imageTextureBinds = new();

    /// <summary>Tracks texture bindings by texture unit and target.</summary>
    private readonly GlBindingMap<(uint, GlTextureTarget)> textureBinds = new();

    /// <summary>Tracks query scopes by target.</summary>
    private readonly GlBindingMap<GlQueryTarget> queryBinds = new();

    /// <summary>Tracks indexed query scopes by target and index.</summary>
    private readonly GlBindingMap<(GlQueryTarget, uint)> queryIndexedBinds = new();

    /// <summary>Tracks the target family first associated with each texture handle.</summary>
    private readonly Dictionary<GlTextureHandle, GlTextureTarget> textureTargets = [];

    /// <summary>
    /// Gets the zero-based active texture unit index required by strict texture bind operations.
    /// </summary>
    /// <param name="function">The GL function that requires an active texture unit.</param>
    /// <returns>The active texture unit index.</returns>
    private uint GetActiveTextureIndex(string function) =>
        activeTexture.Value is { } unit
            ? (uint)((int)unit - (int)GlTextureUnit.Texture0)
            : throw new GlMissingPrerequisiteException(function, "no active texture unit is set; call glActiveTexture first.");
}
