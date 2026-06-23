namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class GeometryCameraTest
{
    /// <summary>The root cube exposes the six unit-cube faces in stable order.</summary>
    [TestMethod]
    public void RootCube_Faces_ReturnUnitCubeFaces()
    {
        var cube = new RootCube();

        Assert.AreEqual(6, cube.Faces.Length);
        Assert.AreEqual(new Vec3(0, 0, 1), cube.Front.Normal);
        Assert.AreEqual(new Vec3(0, 0, -1), cube.Back.Normal);
        Assert.AreEqual(new Vec3(0, 1, 0), cube.Top.Normal);
        Assert.AreEqual(new Vec3(0, -1, 0), cube.Bottom.Normal);
        Assert.AreEqual(new Vec3(-1, 0, 0), cube.Left.Normal);
        Assert.AreEqual(cube.Front, cube.Faces[0]);
        Assert.AreEqual(cube.Right, cube.Faces[5]);
        Assert.AreEqual(new Vec3(0, 1, 1), cube.Front.Quad.TopLeft);
        Assert.AreEqual(new Box3(Vec3.Zero, Vec3.One), cube.Front.Quad.Bounds.Including(cube.Back.Quad.Bounds));
    }

    /// <summary>Root scale derives an adjustable rational scale from monitor scale.</summary>
    [TestMethod]
    public void RootScale_Indexer_ScalesIntegerValues()
    {
        var scale = new RootScale(new(new(new FakeWindowHost { MonitorScale = 1.5f })));

        Assert.AreEqual(1.5f, scale.Scale);
        Assert.AreEqual(15, scale[10]);
        scale.Numerator = 3;
        scale.Denominator = 2;
        Assert.AreEqual(9, scale[6]);
    }

    /// <summary>Camera rotation wraps angles and computes orthogonal direction vectors.</summary>
    [TestMethod]
    public void Camera3D_RotateAndComputeVectors_UpdateDirections()
    {
        var camera = new Camera3D();

        camera.Rotate(new Vec2(0.25f, 0.5f));
        camera.ComputeVectors();

        Assert.AreEqual(0.25f, camera.Rotation.X, 0.0001f);
        Assert.AreNotEqual(default, camera.LookAt);
        Assert.AreNotEqual(default, camera.Up);
        Assert.AreNotEqual(default, camera.Right);
        Assert.AreNotEqual(default, camera.Front);
    }

    /// <summary>Camera rotation wraps above and below one full turn.</summary>
    [TestMethod]
    public void Camera3D_Rotate_WrapsAngles()
    {
        var camera = new Camera3D();

        camera.Rotate(new Vec3(float.Tau + 0.25f, -0.25f, 0));

        Assert.AreEqual(0.25f, camera.Rotation.X, 0.0001f);
        Assert.AreEqual(float.Tau - 0.25f, camera.Rotation.Y, 0.0001f);
    }

    /// <summary>Camera pitch clamping prevents rotations through the vertical poles.</summary>
    [TestMethod]
    public void Camera3D_PreventBackFlipsAndFrontFlips_ClampsPitch()
    {
        var camera = new Camera3D
        {
            Rotation = (0, float.Pi, 0),
        };

        camera.PreventBackFlipsAndFrontFlips();

        Assert.AreEqual(1.5f * float.Pi, camera.Rotation.Y, 0.0001f);

        camera.Rotation = (0, float.Pi / 2, 0);
        camera.PreventBackFlipsAndFrontFlips();

        Assert.AreEqual(89.9f * float.Pi / 180f, camera.Rotation.Y, 0.0001f);
    }

    /// <summary>Perspective computes view and projection matrices from a camera and canvas.</summary>
    [TestMethod]
    public void Perspective3D_ComputeMatrix_UpdatesMatrices()
    {
        var camera = new Camera3D
        {
            Offset = (1, 2, 3),
        };

        camera.ComputeVectors();

        var perspective = new Perspective3D
        {
            Fov = 80,
            Near = 0.2f,
            Far = 500,
        };

        perspective.ComputeMatrix((1920, 1080), camera);

        Assert.AreEqual(80, perspective.Fov);
        Assert.AreEqual(0.2f, perspective.Near);
        Assert.AreEqual(500, perspective.Far);
        Assert.AreNotEqual(default, perspective.View);
        Assert.AreNotEqual(default, perspective.Projection);
    }

    /// <summary>Built-in 3D shaders multiply matrices before the vertex for column-major OpenGL uploads.</summary>
    [TestMethod]
    public void RootPositionColorPrograms3D_VertexShaders_UseProjectionViewVertexOrder()
    {
        const string expected = "gl_Position = matProjection * matView * vec4(inPosition, 1.0);";

        StringAssert.Contains(RootPositionColorProgram3D.Vert, expected);
        StringAssert.Contains(RootPositionColorTextureProgram3D.Vert, expected);
    }
}
