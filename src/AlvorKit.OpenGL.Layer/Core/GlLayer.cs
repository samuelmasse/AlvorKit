namespace AlvorKit.OpenGL.Layer;

/// <summary>
/// A strict, self-checking wrapper over a <see cref="Gl"/> backend. It models OpenGL state as
/// single-assignment so that leaks and double-sets surface at the call site instead of as silent
/// state corruption:
/// <list type="bullet">
/// <item>Every state setter (<c>glViewport</c>, <c>glBlendFunc</c>, ...) must be returned to its
/// default with the matching <c>Reset*</c> before it is set again, and mutually exclusive families
/// (for example <c>glBlendFunc</c> versus <c>glBlendFuncSeparate</c>) cannot be combined.</item>
/// <item>Every bind (<c>glBindBuffer</c>, <c>glBindTexture</c>, ...) must be released with the
/// matching <c>Unbind*</c> for the same target before another object is bound there; scoped binds
/// such as <c>glBeginQuery</c> are closed with their real <c>glEnd*</c>.</item>
/// <item>Explicit <c>glDelete*</c> calls require the deleted object to be unbound or inactive first.</item>
/// </list>
/// The <c>Reset*</c>/<c>Unbind*</c> methods (in the <c>GlLayer.State</c> and <c>GlLayer.Binds</c>
/// partials) are the vocabulary for that discipline; "unbind" is just <c>glBind*(target, 0)</c> and
/// "reset" is the setter called with its <c>Default*</c> value. The <c>Default*</c> values are
/// virtual so a backend whose defaults differ from the OpenGL spec can override them.
/// </summary>
public partial class GlLayer : GlWrapper, IDisposable
{
    /// <summary>Context-wide tracking state shared with every node of one layer hierarchy.</summary>
    private readonly GlLayerState state;

    /// <summary>The parent node that this layer was created from, or null for the root.</summary>
    private readonly GlLayer? parent;

    /// <summary>Child nodes created from this layer, oldest first.</summary>
    private readonly List<GlLayer> children = [];

    /// <summary>Whether this node has already been disposed.</summary>
    private bool disposed;

    /// <summary>Creates a root layer that owns fresh context-tracking state.</summary>
    /// <param name="inner">The backend GL implementation that receives validated calls.</param>
    public GlLayer(Gl inner) : base(inner) => state = new GlLayerState();

    /// <summary>
    /// Creates a child layer node. The child validates against the same context-tracking state as
    /// the parent and calls the same backend, but owns the GL objects created through it: disposing
    /// the child deletes its children's objects and then its own, leaving the rest of the hierarchy
    /// untouched.
    /// </summary>
    /// <param name="parent">The parent node whose context state and backend this child shares.</param>
    protected GlLayer(GlLayer parent) : base(parent.Inner)
    {
        state = parent.state;
        this.parent = parent;
        parent.children.Add(this);
    }

    /// <summary>Layer: the parent node this layer was created from, or null for the root.</summary>
    public GlLayer? Parent => parent;

    /// <summary>Layer: the live child nodes created from this layer, oldest first.</summary>
    public IReadOnlyList<GlLayer> Children => children;
}
