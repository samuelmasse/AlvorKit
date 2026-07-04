namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Contains vertex accessor loading for <see cref="AnimatedGlbMesh"/>.</summary>
internal sealed partial class AnimatedGlbMesh
{
    /// <summary>Reads position, texture coordinate, joint, and weight accessors into one interleaved float array.</summary>
    private static float[] ReadVertices(GlbDocument document, JsonElement primitive, out Vec3 boundsMin, out Vec3 boundsMax)
    {
        var attributes = GlbDocument.RequiredProperty(primitive, "attributes");
        var positionAccessor = GlbDocument.RequiredProperty(attributes, "POSITION").GetInt32();
        var texCoordAccessor = GlbDocument.RequiredProperty(attributes, "TEXCOORD_0").GetInt32();
        var jointsAccessor = GlbDocument.RequiredProperty(attributes, "JOINTS_0").GetInt32();
        var weightsAccessor = GlbDocument.RequiredProperty(attributes, "WEIGHTS_0").GetInt32();
        var vertexCount = document.Accessor(positionAccessor).GetProperty("count").GetInt32();
        var vertices = new float[vertexCount * 13];

        boundsMin = (float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        boundsMax = (float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (var vertexIndex = 0; vertexIndex < vertexCount; vertexIndex++)
        {
            var position = ReadVec3(document, positionAccessor, vertexIndex);
            var texCoord = ReadVec2(document, texCoordAccessor, vertexIndex);
            var joints = ReadJointVec4(document, jointsAccessor, vertexIndex);
            var weights = ReadVec4(document, weightsAccessor, vertexIndex);
            var offset = vertexIndex * 13;

            vertices[offset] = position.X;
            vertices[offset + 1] = position.Y;
            vertices[offset + 2] = position.Z;
            vertices[offset + 3] = texCoord.X;
            vertices[offset + 4] = texCoord.Y;
            vertices[offset + 5] = joints.X;
            vertices[offset + 6] = joints.Y;
            vertices[offset + 7] = joints.Z;
            vertices[offset + 8] = joints.W;
            vertices[offset + 9] = weights.X;
            vertices[offset + 10] = weights.Y;
            vertices[offset + 11] = weights.Z;
            vertices[offset + 12] = weights.W;

            boundsMin = Vec3.Min(boundsMin, position);
            boundsMax = Vec3.Max(boundsMax, position);
        }

        return vertices;
    }
}
