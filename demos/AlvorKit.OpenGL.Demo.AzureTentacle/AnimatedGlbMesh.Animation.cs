namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Contains the animation data shapes used by <see cref="AnimatedGlbMesh"/>.</summary>
internal sealed partial class AnimatedGlbMesh
{
    /// <summary>The supported animation target paths for this demo.</summary>
    private enum AnimationPath
    {
        /// <summary>A channel that animates a node's local translation.</summary>
        Translation,

        /// <summary>A channel that animates a node's local rotation.</summary>
        Rotation,

        /// <summary>A channel that animates a node's local scale.</summary>
        Scale,
    }

    /// <summary>Stores one mutable local node pose sampled from the current animation frame.</summary>
    private struct NodePose
    {
        /// <summary>The fixed local matrix used by nodes authored with the glTF matrix property.</summary>
        public readonly float[] Matrix = new float[MatrixFloatCount];

        /// <summary>The node local translation used for TRS-authored nodes.</summary>
        public Vec3 Translation;

        /// <summary>The node local rotation used for TRS-authored nodes.</summary>
        public Quaternion Rotation;

        /// <summary>The node local scale used for TRS-authored nodes.</summary>
        public Vec3 Scale;

        /// <summary>Whether <see cref="Matrix"/> should be used instead of TRS fields.</summary>
        public bool HasMatrix;

        /// <summary>Creates a node pose with identity TRS values.</summary>
        public NodePose()
        {
            Translation = Vec3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vec3.One;
        }

        /// <summary>Gets an identity local pose for nodes that omit transform properties.</summary>
        public static NodePose Identity => new();
    }

    /// <summary>Stores one sampled glTF animation clip.</summary>
    private sealed class AnimationClip(string name, float duration, AnimationChannel[] channels)
    {
        /// <summary>Gets the clip name copied from the GLB animation object.</summary>
        public string Name { get; } = name;

        /// <summary>Gets the clip duration in seconds.</summary>
        public float Duration { get; } = duration;

        /// <summary>Gets the sampler channels that target node transforms.</summary>
        public AnimationChannel[] Channels { get; } = channels;
    }

    /// <summary>Stores one animation channel and applies it to a sampled pose.</summary>
    private readonly struct AnimationChannel
    {
        /// <summary>The GLB node targeted by this channel.</summary>
        private readonly int nodeIndex;

        /// <summary>The transform property animated by this channel.</summary>
        private readonly AnimationPath path;

        /// <summary>The input keyframe times in seconds.</summary>
        private readonly float[] times;

        /// <summary>The vector output values for translation and scale channels.</summary>
        private readonly Vec3[]? vectorValues;

        /// <summary>The quaternion output values for rotation channels.</summary>
        private readonly Quaternion[]? rotationValues;

        /// <summary>Whether this channel uses STEP interpolation instead of LINEAR interpolation.</summary>
        private readonly bool step;

        /// <summary>Stores one animation channel with either vector or rotation output values.</summary>
        private AnimationChannel(
            int nodeIndex,
            AnimationPath path,
            float[] times,
            Vec3[]? vectorValues,
            Quaternion[]? rotationValues,
            bool step)
        {
            this.nodeIndex = nodeIndex;
            this.path = path;
            this.times = times;
            this.vectorValues = vectorValues;
            this.rotationValues = rotationValues;
            this.step = step;
        }

        /// <summary>Creates a translation or scale animation channel.</summary>
        public static AnimationChannel CreateVector(int nodeIndex, AnimationPath path, float[] times, Vec3[] values, bool step)
        {
            if (times.Length != values.Length)
                throw new FormatException("Animation vector channel input and output counts must match.");

            return new AnimationChannel(nodeIndex, path, times, values, null, step);
        }

        /// <summary>Creates a rotation animation channel.</summary>
        public static AnimationChannel CreateRotation(int nodeIndex, float[] times, Quaternion[] values, bool step)
        {
            if (times.Length != values.Length)
                throw new FormatException("Animation rotation channel input and output counts must match.");

            return new AnimationChannel(nodeIndex, AnimationPath.Rotation, times, null, values, step);
        }

        /// <summary>Samples this channel and writes its value into the supplied node pose array.</summary>
        public void Apply(float seconds, NodePose[] pose)
        {
            if (path == AnimationPath.Rotation)
            {
                pose[nodeIndex].Rotation = SampleRotation(seconds);
                return;
            }

            var value = SampleVector(seconds);
            if (path == AnimationPath.Translation)
                pose[nodeIndex].Translation = value;
            else
                pose[nodeIndex].Scale = value;
        }

        /// <summary>Samples a vector channel at the requested animation time.</summary>
        private Vec3 SampleVector(float seconds)
        {
            var values = vectorValues ?? throw new InvalidOperationException("Vector channel is missing vector values.");
            var frame = FindKeyframe(seconds);
            if (step || frame == times.Length - 1)
                return values[frame];

            var nextFrame = frame + 1;
            var amount = (seconds - times[frame]) / (times[nextFrame] - times[frame]);
            return Vec3.Lerp(values[frame], values[nextFrame], Math.Clamp(amount, 0f, 1f));
        }

        /// <summary>Samples a rotation channel at the requested animation time.</summary>
        private Quaternion SampleRotation(float seconds)
        {
            var values = rotationValues ?? throw new InvalidOperationException("Rotation channel is missing rotation values.");
            var frame = FindKeyframe(seconds);
            if (step || frame == times.Length - 1)
                return values[frame];

            var nextFrame = frame + 1;
            var amount = (seconds - times[frame]) / (times[nextFrame] - times[frame]);
            return Quaternion.Normalize(Quaternion.Slerp(values[frame], values[nextFrame], Math.Clamp(amount, 0f, 1f)));
        }

        /// <summary>Finds the keyframe at or immediately before the supplied animation time.</summary>
        private int FindKeyframe(float seconds)
        {
            if (times.Length == 0)
                throw new FormatException("Animation channels must contain at least one keyframe.");

            for (var frame = 0; frame < times.Length - 1; frame++)
            {
                if (seconds < times[frame + 1])
                    return frame;
            }

            return times.Length - 1;
        }
    }
}
