#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in float aAmbientOcclusion;
layout (location = 3) in vec3 offset;

uniform mat4 projection;
uniform mat4 view;

out vec2 texCoord;
out float ambientOcclusion;

void main() {
	ambientOcclusion = aAmbientOcclusion;
	texCoord = aTexCoord;
	gl_Position = projection * view * vec4(aPosition + offset, 1.0);
}