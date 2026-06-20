#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in vec4 aJoints;
layout (location = 3) in vec4 aWeights;

uniform mat4 uModelViewProjection;
uniform mat4 uJointMatrices[64];

out vec2 vTexCoord;

void main()
{
    int joint0 = int(aJoints.x + 0.5);
    int joint1 = int(aJoints.y + 0.5);
    int joint2 = int(aJoints.z + 0.5);
    int joint3 = int(aJoints.w + 0.5);

    mat4 skin =
        (uJointMatrices[joint0] * aWeights.x) +
        (uJointMatrices[joint1] * aWeights.y) +
        (uJointMatrices[joint2] * aWeights.z) +
        (uJointMatrices[joint3] * aWeights.w);

    gl_Position = uModelViewProjection * skin * vec4(aPosition, 1.0);
    vTexCoord = aTexCoord;
}
