namespace AlvorKit.Engine;

/// <summary>Shader program API for 3D programs with view and projection matrices.</summary>
public interface IRenderProgram3D : IRenderProgram
{
    /// <summary>Sets the view matrix uniform.</summary>
    Mat4 View { set; }

    /// <summary>Sets the projection matrix uniform.</summary>
    Mat4 Projection { set; }
}
