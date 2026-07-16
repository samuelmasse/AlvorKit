namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Represents the skinned mesh, embedded texture, and animation clips loaded from the GLB.</summary>
internal sealed partial class AnimatedGlbMesh
{
    /// <summary>The glTF primitive mode for triangles.</summary>
    private const int TrianglePrimitiveMode = 4;

    /// <summary>The glTF component type for signed 32-bit floating-point accessors.</summary>
    private const int FloatComponentType = 5126;

    /// <summary>The glTF component type for unsigned-byte index and joint accessors.</summary>
    private const int UnsignedByteComponentType = 5121;

    /// <summary>The glTF component type for unsigned-short index and joint accessors.</summary>
    private const int UnsignedShortComponentType = 5123;

    /// <summary>The glTF component type for unsigned-int index accessors.</summary>
    private const int UnsignedIntComponentType = 5125;

    /// <summary>The animation clip this demo prefers when the asset provides it.</summary>
    private const string WalkingAnimationName = "Walking";

    /// <summary>The synthetic selector entry that renders the model's bind pose without animation channels.</summary>
    private const string BasePoseAnimationName = "Base model";

    /// <summary>The selector slot reserved for the unanimated base model pose.</summary>
    private const int BasePoseAnimationSlot = 0;

    /// <summary>The first selector slot that maps to a real GLB animation clip.</summary>
    private const int FirstClipAnimationSlot = 1;

    /// <summary>The parent node index assigned to scene roots.</summary>
    private const int NoParent = -1;

    /// <summary>The node index used when no skinned mesh node has been found yet.</summary>
    private const int MissingNode = -1;

    /// <summary>The fixed crossfade time used when switching between selectable animation slots.</summary>
    private const float AnimationTransitionDurationSeconds = 0.2f;

    /// <summary>The bind-pose local transform for every GLB node.</summary>
    private readonly NodePose[] bindPose;

    /// <summary>The current sampled local transform for every GLB node.</summary>
    private readonly NodePose[] sampledPose;

    /// <summary>The pose captured at animation selection time and used as the crossfade source.</summary>
    private readonly NodePose[] transitionFromPose;

    /// <summary>The parent index for every GLB node.</summary>
    private readonly int[] nodeParents;

    /// <summary>The skin joint node index for each joint palette entry.</summary>
    private readonly int[] jointNodes;

    /// <summary>The inverse bind matrices stored by the GLB skin, packed as column-major matrices.</summary>
    private readonly Mat4[] inverseBindMatrices;

    /// <summary>All animation clips loaded from the GLB.</summary>
    private readonly AnimationClip[] animations;

    /// <summary>The current local matrices for every node, rebuilt from <see cref="sampledPose"/>.</summary>
    private readonly Mat4[] nodeLocalMatrices;

    /// <summary>The current global matrices for every node, rebuilt through <see cref="nodeParents"/>.</summary>
    private readonly Mat4[] nodeGlobalMatrices;

    /// <summary>Tracks which node global matrices are already valid during the current pose rebuild.</summary>
    private readonly bool[] nodeGlobalReady;

    /// <summary>The current skinning palette uploaded to the vertex shader each frame.</summary>
    private readonly Mat4[] jointMatrices;

    /// <summary>The currently selected animation slot, including the synthetic base-pose slot.</summary>
    private int selectedAnimationSlotIndex;

    /// <summary>The wrapped playback position inside the selected animation clip.</summary>
    private float animationSeconds;

    /// <summary>The elapsed time inside the current animation crossfade.</summary>
    private float animationTransitionSeconds;

    /// <summary>Whether the current sampled pose should be blended from <see cref="transitionFromPose"/>.</summary>
    private bool animationTransitionActive;

    /// <summary>Stores the loaded skinned mesh payload and initializes its joint palette to the selected clip's first frame.</summary>
    private AnimatedGlbMesh(
        float[] vertices,
        uint[] indices,
        byte[] textureBytes,
        Vec3 boundsMin,
        Vec3 boundsMax,
        NodePose[] bindPose,
        int[] nodeParents,
        int[] jointNodes,
        Mat4[] inverseBindMatrices,
        AnimationClip[] animations,
        int selectedAnimationIndex)
    {
        Vertices = vertices;
        Indices = indices;
        TextureBytes = textureBytes;
        BoundsMin = boundsMin;
        BoundsMax = boundsMax;
        this.bindPose = bindPose;
        this.nodeParents = nodeParents;
        this.jointNodes = jointNodes;
        this.inverseBindMatrices = inverseBindMatrices;
        this.animations = animations;
        selectedAnimationSlotIndex = selectedAnimationIndex + FirstClipAnimationSlot;
        sampledPose = new NodePose[bindPose.Length];
        transitionFromPose = new NodePose[bindPose.Length];
        nodeLocalMatrices = new Mat4[bindPose.Length];
        nodeGlobalMatrices = new Mat4[bindPose.Length];
        nodeGlobalReady = new bool[bindPose.Length];
        jointMatrices = new Mat4[jointNodes.Length];
        UpdateAnimation(0f);
    }

    /// <summary>Gets the interleaved position, UV, joint-index, and weight vertex data.</summary>
    public float[] Vertices { get; }

    /// <summary>Gets the unsigned-int triangle indices uploaded by the demo.</summary>
    public uint[] Indices { get; }

    /// <summary>Gets the embedded PNG bytes referenced by the primitive's base color material.</summary>
    public byte[] TextureBytes { get; }

    /// <summary>Gets the minimum model-space position in the loaded mesh.</summary>
    public Vec3 BoundsMin { get; }

    /// <summary>Gets the maximum model-space position in the loaded mesh.</summary>
    public Vec3 BoundsMax { get; }

    /// <summary>Gets the selected animation clip name for display or diagnostics.</summary>
    public string AnimationName => GetAnimationName(selectedAnimationSlotIndex);

    /// <summary>Gets the number of vertices in the loaded mesh.</summary>
    public int VertexCount => Vertices.Length / 13;

    /// <summary>Gets the number of triangles in the loaded mesh.</summary>
    public int TriangleCount => Indices.Length / 3;

    /// <summary>Gets the number of joints in the loaded skin.</summary>
    public int JointCount => jointNodes.Length;

    /// <summary>Gets the number of animation clips loaded from the GLB.</summary>
    public int AnimationCount => animations.Length + FirstClipAnimationSlot;

    /// <summary>Gets the currently selected animation slot index.</summary>
    public int SelectedAnimationIndex => selectedAnimationSlotIndex;

    /// <summary>Gets the active animation clip used by the pose sampler.</summary>
    private AnimationClip SelectedAnimation => animations[selectedAnimationSlotIndex - FirstClipAnimationSlot];

    /// <summary>Gets whether the model should render its unanimated bind pose.</summary>
    private bool BasePoseSelected => selectedAnimationSlotIndex == BasePoseAnimationSlot;

    /// <summary>Gets the current skinning palette as packed column-major matrices.</summary>
    public ReadOnlySpan<Mat4> JointMatrices => jointMatrices;

    /// <summary>Loads the first skinned triangle primitive from a GLB 2.0 file and selects the walking clip when present.</summary>
    public static AnimatedGlbMesh Load(string path)
    {
        using var document = GlbDocument.Open(path);
        var root = document.Root;
        var meshIndex = 0;
        var mesh = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "meshes"), meshIndex, "meshes");
        var primitive = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(mesh, "primitives"), 0, "meshes[0].primitives");
        var mode = GlbDocument.GetInt32OrDefault(primitive, "mode", TrianglePrimitiveMode);

        if (mode != TrianglePrimitiveMode)
            throw new NotSupportedException($"The demo only supports GLB triangle primitives, but the model uses mode {mode}.");

        var meshNodeIndex = FindMeshNode(root, meshIndex);
        var meshNode = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "nodes"), meshNodeIndex, "nodes");
        var skinIndex = GlbDocument.RequiredProperty(meshNode, "skin").GetInt32();
        var skin = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "skins"), skinIndex, "skins");

        var vertices = ReadVertices(document, primitive, out var boundsMin, out var boundsMax);
        var indices = ReadIndices(document, GlbDocument.RequiredProperty(primitive, "indices").GetInt32());
        var textureBytes = ReadBaseColorTextureBytes(document, primitive);
        var bindPose = ReadBindPose(root);
        var nodeParents = ReadNodeParents(root);
        var jointNodes = ReadJointNodes(skin);
        var inverseBindMatrices = ReadInverseBindMatrices(document, skin, jointNodes.Length);
        var (animations, selectedAnimationIndex) = ReadAnimations(document, WalkingAnimationName);

        return new AnimatedGlbMesh(
            vertices,
            indices,
            textureBytes,
            boundsMin,
            boundsMax,
            bindPose,
            nodeParents,
            jointNodes,
            inverseBindMatrices,
            animations,
            selectedAnimationIndex);
    }

    /// <summary>Reads scalar index data from a glTF accessor and widens it to unsigned integers for one OpenGL index format.</summary>
    private static uint[] ReadIndices(GlbDocument document, int accessorIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        var componentType = GlbDocument.RequiredProperty(accessor, "componentType").GetInt32();

        if (GlbDocument.RequiredProperty(accessor, "type").GetString() != "SCALAR")
            throw new NotSupportedException($"Accessor {accessorIndex} must be a SCALAR index accessor.");

        var componentSize = componentType switch
        {
            UnsignedByteComponentType => 1,
            UnsignedShortComponentType => 2,
            UnsignedIntComponentType => 4,
            _ => throw new NotSupportedException($"Accessor {accessorIndex} uses unsupported index component type {componentType}."),
        };
        var count = GlbDocument.RequiredProperty(accessor, "count").GetInt32();
        var indices = new uint[count];

        for (var index = 0; index < count; index++)
        {
            var offset = document.AccessorElementOffset(accessor, index, componentSize);
            indices[index] = componentType switch
            {
                UnsignedByteComponentType => document.Binary[offset],
                UnsignedShortComponentType => BinaryPrimitives.ReadUInt16LittleEndian(document.Binary.Slice(offset, 2)),
                _ => BinaryPrimitives.ReadUInt32LittleEndian(document.Binary.Slice(offset, 4)),
            };
        }

        return indices;
    }

    /// <summary>Finds the primitive material's base color texture image and copies its encoded bytes.</summary>
    private static byte[] ReadBaseColorTextureBytes(GlbDocument document, JsonElement primitive)
    {
        var root = document.Root;
        var materialIndex = GlbDocument.GetInt32OrDefault(primitive, "material", 0);
        var material = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "materials"), materialIndex, "materials");
        var pbr = GlbDocument.RequiredProperty(material, "pbrMetallicRoughness");
        var baseColor = GlbDocument.RequiredProperty(pbr, "baseColorTexture");
        var textureIndex = GlbDocument.RequiredProperty(baseColor, "index").GetInt32();
        var texture = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "textures"), textureIndex, "textures");
        var imageIndex = GlbDocument.RequiredProperty(texture, "source").GetInt32();
        var image = GlbDocument.RequiredArrayElement(GlbDocument.RequiredProperty(root, "images"), imageIndex, "images");

        if (GlbDocument.GetStringOrDefault(image, "mimeType", string.Empty) != "image/png")
            throw new NotSupportedException("The demo expects the GLB base color texture to be an embedded PNG image.");

        var imageViewIndex = GlbDocument.RequiredProperty(image, "bufferView").GetInt32();
        return document.ReadBufferView(imageViewIndex).ToArray();
    }

    /// <summary>Reads all GLB node parent links into a direct lookup table.</summary>
    private static int[] ReadNodeParents(JsonElement root)
    {
        var nodes = GlbDocument.RequiredProperty(root, "nodes");
        var parents = new int[nodes.GetArrayLength()];
        Array.Fill(parents, NoParent);

        for (var nodeIndex = 0; nodeIndex < nodes.GetArrayLength(); nodeIndex++)
        {
            var node = nodes[nodeIndex];
            if (!node.TryGetProperty("children", out var children))
                continue;

            for (var childIndex = 0; childIndex < children.GetArrayLength(); childIndex++)
                parents[children[childIndex].GetInt32()] = nodeIndex;
        }

        return parents;
    }

    /// <summary>Reads each node's default local transform from TRS or matrix properties.</summary>
    private static NodePose[] ReadBindPose(JsonElement root)
    {
        var nodes = GlbDocument.RequiredProperty(root, "nodes");
        var bindPose = new NodePose[nodes.GetArrayLength()];

        for (var nodeIndex = 0; nodeIndex < nodes.GetArrayLength(); nodeIndex++)
            bindPose[nodeIndex] = ReadNodePose(nodes[nodeIndex]);

        return bindPose;
    }

    /// <summary>Reads one node's default local transform, preserving matrix nodes as fixed local matrices.</summary>
    private static NodePose ReadNodePose(JsonElement node)
    {
        if (node.TryGetProperty("matrix", out var matrix))
        {
            var pose = NodePose.Identity;
            pose.HasMatrix = true;
            pose.Matrix = ReadMat4Property(matrix);

            return pose;
        }

        return new NodePose
        {
            Translation = ReadVector3Property(node, "translation", Vec3.Zero),
            Rotation = ReadQuaternionProperty(node, "rotation", Quat.Identity),
            Scale = ReadVector3Property(node, "scale", Vec3.One),
        };
    }

    /// <summary>Reads the skin joint-node list in palette order.</summary>
    private static int[] ReadJointNodes(JsonElement skin)
    {
        var joints = GlbDocument.RequiredProperty(skin, "joints");
        var jointNodes = new int[joints.GetArrayLength()];

        for (var index = 0; index < jointNodes.Length; index++)
            jointNodes[index] = joints[index].GetInt32();

        return jointNodes;
    }

    /// <summary>Reads the skin inverse bind matrix accessor as packed column-major matrices.</summary>
    private static Mat4[] ReadInverseBindMatrices(GlbDocument document, JsonElement skin, int jointCount)
    {
        var accessorIndex = GlbDocument.RequiredProperty(skin, "inverseBindMatrices").GetInt32();
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "MAT4", FloatComponentType, accessorIndex);

        if (GlbDocument.RequiredProperty(accessor, "count").GetInt32() != jointCount)
            throw new NotSupportedException("The skin inverse bind matrix count must match the joint count.");

        return ReadMat4Array(document, accessorIndex);
    }

    /// <summary>Reads every animation clip and returns the index of the preferred starting clip.</summary>
    private static (AnimationClip[] Animations, int SelectedIndex) ReadAnimations(GlbDocument document, string preferredName)
    {
        var animations = GlbDocument.RequiredProperty(document.Root, "animations");
        if (animations.GetArrayLength() == 0)
            throw new NotSupportedException("The GLB does not contain animation clips.");

        var clips = new AnimationClip[animations.GetArrayLength()];
        var selectedIndex = 0;

        for (var animationIndex = 0; animationIndex < clips.Length; animationIndex++)
        {
            var animation = animations[animationIndex];
            clips[animationIndex] = ReadAnimationClip(document, animation, animationIndex);
            if (GlbDocument.GetStringOrDefault(animation, "name", string.Empty) == preferredName)
                selectedIndex = animationIndex;
        }

        return (clips, selectedIndex);
    }

    /// <summary>Reads one animation clip's channels and keyframes.</summary>
    private static AnimationClip ReadAnimationClip(GlbDocument document, JsonElement animation, int animationIndex)
    {
        var samplers = GlbDocument.RequiredProperty(animation, "samplers");
        var channels = GlbDocument.RequiredProperty(animation, "channels");
        var animationChannels = new AnimationChannel[channels.GetArrayLength()];
        var duration = 0f;

        for (var channelIndex = 0; channelIndex < animationChannels.Length; channelIndex++)
        {
            var channel = channels[channelIndex];
            var samplerIndex = GlbDocument.RequiredProperty(channel, "sampler").GetInt32();
            var sampler = GlbDocument.RequiredArrayElement(samplers, samplerIndex, "animations[].samplers");
            var target = GlbDocument.RequiredProperty(channel, "target");
            var nodeIndex = GlbDocument.RequiredProperty(target, "node").GetInt32();
            var path = ReadAnimationPath(GlbDocument.RequiredProperty(target, "path").GetString() ?? string.Empty);
            var times = ReadScalarFloatArray(document, GlbDocument.RequiredProperty(sampler, "input").GetInt32());
            var step = ReadInterpolation(sampler);
            var outputAccessor = GlbDocument.RequiredProperty(sampler, "output").GetInt32();

            duration = MathF.Max(duration, times[^1]);
            animationChannels[channelIndex] = path == AnimationPath.Rotation
                ? AnimationChannel.CreateRotation(nodeIndex, times, ReadQuaternionArray(document, outputAccessor), step)
                : AnimationChannel.CreateVector(nodeIndex, path, times, ReadVec3Array(document, outputAccessor), step);
        }

        var name = GlbDocument.GetStringOrDefault(animation, "name", $"Animation {animationIndex}");
        return new AnimationClip(name, duration, animationChannels);
    }

    /// <summary>Maps a glTF channel target path to the demo's animation path enum.</summary>
    private static AnimationPath ReadAnimationPath(string path) =>
        path switch
        {
            "translation" => AnimationPath.Translation,
            "rotation" => AnimationPath.Rotation,
            "scale" => AnimationPath.Scale,
            _ => throw new NotSupportedException($"Animation target path '{path}' is not supported by this demo."),
        };

    /// <summary>Reads a sampler interpolation mode and returns whether the channel should step between keyframes.</summary>
    private static bool ReadInterpolation(JsonElement sampler)
    {
        var interpolation = GlbDocument.GetStringOrDefault(sampler, "interpolation", "LINEAR");
        return interpolation switch
        {
            "STEP" => true,
            "LINEAR" => false,
            _ => throw new NotSupportedException($"Animation interpolation '{interpolation}' is not supported by this demo."),
        };
    }

    /// <summary>Finds the GLB node that instantiates the requested mesh and carries the skin reference.</summary>
    private static int FindMeshNode(JsonElement root, int meshIndex)
    {
        var nodes = GlbDocument.RequiredProperty(root, "nodes");
        var meshNodeIndex = MissingNode;

        for (var nodeIndex = 0; nodeIndex < nodes.GetArrayLength(); nodeIndex++)
        {
            var node = nodes[nodeIndex];
            if (!node.TryGetProperty("mesh", out var mesh) || mesh.GetInt32() != meshIndex)
                continue;

            meshNodeIndex = nodeIndex;
            break;
        }

        if (meshNodeIndex == MissingNode)
            throw new FormatException($"The GLB JSON does not contain a node for mesh {meshIndex}.");

        return meshNodeIndex;
    }

    /// <summary>Reads a three-float vector from a glTF accessor entry.</summary>
    private static Vec3 ReadVec3(GlbDocument document, int accessorIndex, int elementIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "VEC3", FloatComponentType, accessorIndex);
        var offset = document.AccessorElementOffset(accessor, elementIndex, 12);
        return (document.ReadSingle(offset), document.ReadSingle(offset + 4), document.ReadSingle(offset + 8));
    }

    /// <summary>Reads a two-float vector from a glTF accessor entry.</summary>
    private static Vec2 ReadVec2(GlbDocument document, int accessorIndex, int elementIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "VEC2", FloatComponentType, accessorIndex);
        var offset = document.AccessorElementOffset(accessor, elementIndex, 8);
        return (document.ReadSingle(offset), document.ReadSingle(offset + 4));
    }

    /// <summary>Reads a four-float vector from a glTF accessor entry.</summary>
    private static Vec4 ReadVec4(GlbDocument document, int accessorIndex, int elementIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "VEC4", FloatComponentType, accessorIndex);
        var offset = document.AccessorElementOffset(accessor, elementIndex, 16);
        return (
            document.ReadSingle(offset),
            document.ReadSingle(offset + 4),
            document.ReadSingle(offset + 8),
            document.ReadSingle(offset + 12));
    }

    /// <summary>Reads a four-component unsigned joint-index accessor.</summary>
    private static Vec4u ReadJointVec4(GlbDocument document, int accessorIndex, int elementIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        var componentType = GlbDocument.RequiredProperty(accessor, "componentType").GetInt32();
        var componentSize = componentType switch
        {
            UnsignedByteComponentType => 1,
            UnsignedShortComponentType => 2,
            _ => throw new NotSupportedException($"Accessor {accessorIndex} uses unsupported joint component type {componentType}."),
        };

        if (GlbDocument.RequiredProperty(accessor, "type").GetString() != "VEC4")
            throw new NotSupportedException($"Accessor {accessorIndex} must be a VEC4 joint accessor.");

        var offset = document.AccessorElementOffset(accessor, elementIndex, componentSize * 4);
        return componentType == UnsignedByteComponentType
            ? (document.Binary[offset], document.Binary[offset + 1], document.Binary[offset + 2], document.Binary[offset + 3])
            : (
                BinaryPrimitives.ReadUInt16LittleEndian(document.Binary.Slice(offset, 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(document.Binary.Slice(offset + 2, 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(document.Binary.Slice(offset + 4, 2)),
                BinaryPrimitives.ReadUInt16LittleEndian(document.Binary.Slice(offset + 6, 2)));
    }

    /// <summary>Reads a scalar float accessor into a managed keyframe time array.</summary>
    private static float[] ReadScalarFloatArray(GlbDocument document, int accessorIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "SCALAR", FloatComponentType, accessorIndex);
        var count = GlbDocument.RequiredProperty(accessor, "count").GetInt32();
        var values = new float[count];

        for (var index = 0; index < count; index++)
            values[index] = document.ReadSingle(document.AccessorElementOffset(accessor, index, sizeof(float)));

        return values;
    }

    /// <summary>Reads a VEC3 float accessor into a vector keyframe array.</summary>
    private static Vec3[] ReadVec3Array(GlbDocument document, int accessorIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "VEC3", FloatComponentType, accessorIndex);
        var count = GlbDocument.RequiredProperty(accessor, "count").GetInt32();
        var values = new Vec3[count];

        for (var index = 0; index < count; index++)
            values[index] = ReadVec3(document, accessorIndex, index);

        return values;
    }

    /// <summary>Reads a VEC4 float accessor into a quaternion keyframe array.</summary>
    private static Quat[] ReadQuaternionArray(GlbDocument document, int accessorIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "VEC4", FloatComponentType, accessorIndex);
        var count = GlbDocument.RequiredProperty(accessor, "count").GetInt32();
        var values = new Quat[count];

        for (var index = 0; index < count; index++)
        {
            var value = ReadVec4(document, accessorIndex, index);
            values[index] = new Quat(value.X, value.Y, value.Z, value.W).Normalized;
        }

        return values;
    }

    /// <summary>Reads a MAT4 float accessor into packed column-major matrices.</summary>
    private static Mat4[] ReadMat4Array(GlbDocument document, int accessorIndex)
    {
        var accessor = document.Accessor(accessorIndex);
        ValidateAccessor(accessor, "MAT4", FloatComponentType, accessorIndex);
        var count = GlbDocument.RequiredProperty(accessor, "count").GetInt32();
        var matrices = new Mat4[count];

        for (var matrixIndex = 0; matrixIndex < count; matrixIndex++)
            matrices[matrixIndex] = ReadMat4(document, accessor, matrixIndex);

        return matrices;
    }

    /// <summary>Validates that an accessor has the vector shape and component type the animated model path requires.</summary>
    private static void ValidateAccessor(JsonElement accessor, string expectedType, int expectedComponentType, int accessorIndex)
    {
        var componentType = GlbDocument.RequiredProperty(accessor, "componentType").GetInt32();
        var type = GlbDocument.RequiredProperty(accessor, "type").GetString();

        if (type != expectedType || componentType != expectedComponentType)
        {
            throw new NotSupportedException(
                $"Accessor {accessorIndex} must be {expectedType} with component type {expectedComponentType}.");
        }
    }

    /// <summary>Reads a Vec3 node property or returns the supplied default value.</summary>
    private static Vec3 ReadVector3Property(JsonElement node, string name, Vec3 defaultValue)
    {
        if (!node.TryGetProperty(name, out var values))
            return defaultValue;

        return (values[0].GetSingle(), values[1].GetSingle(), values[2].GetSingle());
    }

    /// <summary>Reads a Quat node property or returns the supplied default value.</summary>
    private static Quat ReadQuaternionProperty(JsonElement node, string name, Quat defaultValue)
    {
        if (!node.TryGetProperty(name, out var values))
            return defaultValue;

        return new Quat(
            values[0].GetSingle(),
            values[1].GetSingle(),
            values[2].GetSingle(),
            values[3].GetSingle()).Normalized;
    }

    /// <summary>Builds a node's local matrix from either its fixed matrix value or TRS components.</summary>
    private static Mat4 LocalMatrix(NodePose pose)
    {
        if (pose.HasMatrix)
            return pose.Matrix;

        return Mat4.CreateTranslation(pose.Translation) *
            Mat4.CreateRotation(pose.Rotation.Normalized) *
            Mat4.CreateScale(pose.Scale);
    }

    /// <summary>Reads one MAT4 accessor element into the demo's column-major matrix type.</summary>
    private static Mat4 ReadMat4(GlbDocument document, JsonElement accessor, int elementIndex)
    {
        var offset = document.AccessorElementOffset(accessor, elementIndex, Mat4.ComponentCount * sizeof(float));
        return new Mat4(
            ReadVec4(document, offset),
            ReadVec4(document, offset + (4 * sizeof(float))),
            ReadVec4(document, offset + (8 * sizeof(float))),
            ReadVec4(document, offset + (12 * sizeof(float))));
    }

    /// <summary>Reads one glTF matrix property into the demo's column-major matrix type.</summary>
    private static Mat4 ReadMat4Property(JsonElement matrix) =>
        new(
            ReadJsonVec4(matrix, 0),
            ReadJsonVec4(matrix, 4),
            ReadJsonVec4(matrix, 8),
            ReadJsonVec4(matrix, 12));

    /// <summary>Reads one column vector from a packed binary matrix.</summary>
    private static Vec4 ReadVec4(GlbDocument document, int offset) =>
        new(
            document.ReadSingle(offset),
            document.ReadSingle(offset + sizeof(float)),
            document.ReadSingle(offset + (2 * sizeof(float))),
            document.ReadSingle(offset + (3 * sizeof(float))));

    /// <summary>Reads one column vector from a JSON matrix array.</summary>
    private static Vec4 ReadJsonVec4(JsonElement values, int offset) =>
        new(
            values[offset].GetSingle(),
            values[offset + 1].GetSingle(),
            values[offset + 2].GetSingle(),
            values[offset + 3].GetSingle());
}
