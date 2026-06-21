namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Contains pose sampling, animation selection, and transition blending for <see cref="AnimatedGlbMesh"/>.</summary>
internal sealed partial class AnimatedGlbMesh
{
    /// <summary>Advances the selected animation, applies any active crossfade, and rebuilds the shader joint palette.</summary>
    public void UpdateAnimation(float elapsedSeconds)
    {
        var frameSeconds = MathF.Max(0f, elapsedSeconds);
        AdvanceSelectedAnimation(frameSeconds);
        SamplePose(animationSeconds);
        ApplyAnimationTransition(frameSeconds);
        RebuildNodeLocalMatrices();
        RebuildNodeGlobalMatrices();
        RebuildJointMatrices();
    }

    /// <summary>Selects the next animation slot and starts a short crossfade from the current sampled pose.</summary>
    public void SelectNextAnimation() => SelectAnimation((selectedAnimationSlotIndex + 1) % AnimationCount);

    /// <summary>Selects the previous animation slot and starts a short crossfade from the current sampled pose.</summary>
    public void SelectPreviousAnimation() => SelectAnimation((selectedAnimationSlotIndex + AnimationCount - 1) % AnimationCount);

    /// <summary>Gets the display name for one selectable animation slot.</summary>
    public string GetAnimationName(int animationIndex) =>
        animationIndex == BasePoseAnimationSlot ? BasePoseAnimationName : animations[animationIndex - FirstClipAnimationSlot].Name;

    /// <summary>Gets the duration, in seconds, for one selectable animation slot.</summary>
    public float GetAnimationDuration(int animationIndex) =>
        animationIndex == BasePoseAnimationSlot ? 0f : animations[animationIndex - FirstClipAnimationSlot].Duration;

    /// <summary>Advances playback time for the selected real clip, or holds the synthetic base-pose slot at zero.</summary>
    private void AdvanceSelectedAnimation(float elapsedSeconds)
    {
        if (BasePoseSelected)
        {
            animationSeconds = 0f;
            return;
        }

        var animation = SelectedAnimation;
        if (animation.Duration <= 0f)
            return;

        animationSeconds += elapsedSeconds;
        animationSeconds %= animation.Duration;
    }

    /// <summary>Selects one animation slot by index and captures the current sampled pose as the transition source.</summary>
    private void SelectAnimation(int animationIndex)
    {
        if (animationIndex == selectedAnimationSlotIndex)
            return;

        CaptureTransitionSourcePose();
        selectedAnimationSlotIndex = animationIndex;
        animationSeconds = 0f;
        animationTransitionSeconds = 0f;
        animationTransitionActive = true;
        UpdateAnimation(0f);
    }

    /// <summary>Copies the current sampled pose into the reusable transition-source buffer without allocating.</summary>
    private void CaptureTransitionSourcePose()
    {
        for (var nodeIndex = 0; nodeIndex < sampledPose.Length; nodeIndex++)
            transitionFromPose[nodeIndex] = sampledPose[nodeIndex];
    }

    /// <summary>Copies the bind pose and applies each animation channel at the requested clip time.</summary>
    private void SamplePose(float seconds)
    {
        for (var nodeIndex = 0; nodeIndex < bindPose.Length; nodeIndex++)
            sampledPose[nodeIndex] = bindPose[nodeIndex];

        if (BasePoseSelected)
            return;

        var channels = SelectedAnimation.Channels;
        for (var channelIndex = 0; channelIndex < channels.Length; channelIndex++)
            channels[channelIndex].Apply(seconds, sampledPose);
    }

    /// <summary>Blends the sampled pose from the captured transition source while a crossfade is active.</summary>
    private void ApplyAnimationTransition(float elapsedSeconds)
    {
        if (!animationTransitionActive)
            return;

        animationTransitionSeconds += elapsedSeconds;
        var blend = animationTransitionSeconds / AnimationTransitionDurationSeconds;
        if (blend >= 1f)
        {
            animationTransitionActive = false;
            return;
        }

        for (var nodeIndex = 0; nodeIndex < sampledPose.Length; nodeIndex++)
            BlendPose(transitionFromPose[nodeIndex], sampledPose[nodeIndex], blend, ref sampledPose[nodeIndex]);
    }

    /// <summary>Blends one node pose from the transition source toward the selected animation pose.</summary>
    private static void BlendPose(in NodePose from, in NodePose to, float amount, ref NodePose destination)
    {
        destination = to;
        if (from.HasMatrix || to.HasMatrix)
            return;

        var clampedAmount = Math.Clamp(amount, 0f, 1f);
        destination.Translation = Vec3.Lerp(from.Translation, to.Translation, clampedAmount);
        destination.Rotation = Quaternion.Normalize(Quaternion.Slerp(from.Rotation, to.Rotation, clampedAmount));
        destination.Scale = Vec3.Lerp(from.Scale, to.Scale, clampedAmount);
    }

    /// <summary>Rebuilds node local matrices from the sampled pose.</summary>
    private void RebuildNodeLocalMatrices()
    {
        for (var nodeIndex = 0; nodeIndex < sampledPose.Length; nodeIndex++)
            WriteLocalMatrix(sampledPose[nodeIndex], nodeLocalMatrices.AsSpan(nodeIndex * MatrixFloatCount, MatrixFloatCount));
    }

    /// <summary>Rebuilds node global matrices in hierarchy order through a small recursive cache.</summary>
    private void RebuildNodeGlobalMatrices()
    {
        Array.Fill(nodeGlobalReady, false);
        for (var nodeIndex = 0; nodeIndex < nodeParents.Length; nodeIndex++)
            EnsureNodeGlobalMatrix(nodeIndex);
    }

    /// <summary>Ensures one node global matrix has been composed from its parent chain.</summary>
    private void EnsureNodeGlobalMatrix(int nodeIndex)
    {
        if (nodeGlobalReady[nodeIndex])
            return;

        var local = nodeLocalMatrices.AsSpan(nodeIndex * MatrixFloatCount, MatrixFloatCount);
        var global = nodeGlobalMatrices.AsSpan(nodeIndex * MatrixFloatCount, MatrixFloatCount);
        var parentIndex = nodeParents[nodeIndex];

        if (parentIndex == NoParent)
        {
            local.CopyTo(global);
        }
        else
        {
            EnsureNodeGlobalMatrix(parentIndex);
            var parent = nodeGlobalMatrices.AsSpan(parentIndex * MatrixFloatCount, MatrixFloatCount);
            Multiply(parent, local, global);
        }

        nodeGlobalReady[nodeIndex] = true;
    }

    /// <summary>Combines each animated joint global transform with its inverse bind matrix for the shader palette.</summary>
    private void RebuildJointMatrices()
    {
        for (var jointIndex = 0; jointIndex < jointNodes.Length; jointIndex++)
        {
            var global = nodeGlobalMatrices.AsSpan(jointNodes[jointIndex] * MatrixFloatCount, MatrixFloatCount);
            var inverseBind = inverseBindMatrices.AsSpan(jointIndex * MatrixFloatCount, MatrixFloatCount);
            var joint = jointMatrices.AsSpan(jointIndex * MatrixFloatCount, MatrixFloatCount);
            Multiply(global, inverseBind, joint);
        }
    }
}
