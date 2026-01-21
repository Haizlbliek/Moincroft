#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 offset;

uniform mat4 projection;
uniform mat4 view;

out vec4 col;

void main() {
	col = vec4(int(aPosition.x) % 2, int(aPosition.y) % 2, int(aPosition.z) % 2, 1.0);
	gl_Position = projection * view * vec4(aPosition + offset, 1.0);
}