#version 330 core
in vec2 vTexCoord;
in vec3 vFaceColor;

uniform sampler2D uTexture;

out vec4 FragColor;

void main()
{
    vec4 texel = texture(uTexture, vTexCoord);
    vec2 edgeDistance = min(vTexCoord, 1.0 - vTexCoord);
    float edge = smoothstep(0.0, 0.055, min(edgeDistance.x, edgeDistance.y));
    vec3 face = texel.rgb * vFaceColor;
    FragColor = vec4(mix(vec3(0.025, 0.03, 0.04), face, edge), texel.a);
}
