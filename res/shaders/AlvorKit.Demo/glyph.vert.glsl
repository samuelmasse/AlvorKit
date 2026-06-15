#version 330 core
layout(location = 0) in vec2 position;
layout(location = 1) in vec2 uv;
out vec2 vertexUv;
uniform vec2 uScale;
void main() { vertexUv = uv; gl_Position = vec4(position * uScale, 0.0, 1.0); }
