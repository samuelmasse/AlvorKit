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
/// </list>
/// The <c>Reset*</c>/<c>Unbind*</c> methods (in the <c>GlLayer.State</c> and <c>GlLayer.Binds</c>
/// partials) are the vocabulary for that discipline; "unbind" is just <c>glBind*(target, 0)</c> and
/// "reset" is the setter called with its <c>Default*</c> value. The <c>Default*</c> values are
/// virtual so a backend whose defaults differ from the OpenGL spec can override them.
/// </summary>
public partial class GlLayer(Gl inner) : GlWrapper(inner), IDisposable
{
}
