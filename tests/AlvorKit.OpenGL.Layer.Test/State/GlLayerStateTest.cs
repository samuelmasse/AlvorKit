namespace AlvorKit.OpenGL.Layer.Test;

/// <summary>
/// Tests strict state set/reset rules enforced by <see cref="GlLayer"/>.
/// </summary>
[TestClass]
public partial class GlLayerStateTest
{
    private GlLayer gl = null!;

    [TestInitialize]
    public void Setup() => gl = new GlLayer(new GlNoop());
}
