#version 330 core

uniform vec3 offset;
uniform mat4 projection;
uniform mat4 view;

const vec3 base[24] = vec3[](
	vec3(0, 0, 0), vec3(1, 0, 0), vec3(0, 1, 0), vec3(1, 1, 0),
	vec3(0, 0, 0), vec3(0, 1, 0), vec3(1, 0, 0), vec3(1, 1, 0),

	vec3(0, 0, 1), vec3(1, 0, 1), vec3(0, 1, 1), vec3(1, 1, 1),
	vec3(0, 0, 1), vec3(0, 1, 1), vec3(1, 0, 1), vec3(1, 1, 1),

	vec3(0, 0, 0), vec3(0, 0, 1), vec3(0, 1, 0), vec3(0, 1, 1),
	vec3(1, 0, 0), vec3(1, 0, 1), vec3(1, 1, 0), vec3(1, 1, 1)
);

void main() {
	vec3 pos = base[gl_VertexID] + offset;
	gl_Position = projection * ((view * vec4(pos, 1.0)) * vec4(1.0, 1.0, 1.0, 1.001));
}