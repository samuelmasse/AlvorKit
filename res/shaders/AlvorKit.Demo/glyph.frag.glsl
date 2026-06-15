#version 330 core
in vec2 vertexUv;
out vec4 color;
uniform sampler2D glyph;
void main() { color = vec4(vec3(texture(glyph, vertexUv).r), 1.0); }
