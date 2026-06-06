#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in uint aAmbientOcclusion;
layout (location = 3) in vec4 aColor;

uniform mat4 projection;
uniform mat4 view;
uniform vec3 uChunkOffset;

out vec2 texCoord;
out float ambientOcclusion;
out float light;
out vec4 color;

void main() {
	ambientOcclusion = float(aAmbientOcclusion & 0xFu);
	light = float((aAmbientOcclusion >> 4) & 0xFu);
	texCoord = aTexCoord;
	color = aColor.bgra;
	gl_Position = projection * view * vec4(aPosition + uChunkOffset, 1.0);
}