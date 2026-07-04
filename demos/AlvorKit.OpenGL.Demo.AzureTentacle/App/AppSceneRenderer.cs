namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Draws the animated GLB scene into the root OpenGL backbuffer.</summary>
[App]
public class AppSceneRenderer(
    RootGl gl,
    RootCanvas canvas,
    RootBackbuffer backbuffer,
    AppStyle style,
    AppSession session)
{
    public void Render()
    {
        var size = canvas.Size;
        if (size.X <= 0f || size.Y <= 0f)
            return;

        backbuffer.Clear(style.SceneClearColor);
        gl.Viewport(0, 0, (int)size.X, (int)size.Y);
        gl.Enable(GlEnableCap.DepthTest);
        gl.DepthFunc(GlDepthFunction.Less);
        gl.Enable(GlEnableCap.CullFace);
        gl.CullFace(GlTriangleFace.Back);

        session.RenderModel((int)size.X, (int)size.Y);

        gl.ResetCullFace();
        gl.Disable(GlEnableCap.CullFace);
        gl.ResetDepthFunc();
        gl.Disable(GlEnableCap.DepthTest);
        gl.ResetViewport();
    }
}
