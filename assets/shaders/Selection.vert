#version 330 core

uniform vec3 offset;
uniform mat4 projection;
uniform mat4 view;

const vec3 box_vertices[8] = vec3[8](
	vec3(0.0, 0.0, 0.0),
	vec3(1.0, 0.0, 0.0),
	vec3(1.0, 1.0, 0.0),
	vec3(0.0, 1.0, 0.0),
	vec3(0.0, 0.0, 1.0),
	vec3(1.0, 0.0, 1.0),
	vec3(1.0, 1.0, 1.0),
	vec3(0.0, 1.0, 1.0)
);

const int cuboid_indices[36] = int[36](
	4, 5, 6, 6, 7, 4,
	1, 0, 3, 3, 2, 1,
	0, 4, 7, 7, 3, 0,
	5, 1, 2, 2, 6, 5,
	7, 6, 2, 2, 3, 7,
	0, 1, 5, 5, 4, 0
);

const vec4 edge_blueprints[12] = vec4[12](
	vec4(0.0, 0.0, 0.0, 0.0), vec4(0.0, 0.0, 1.0, 0.0), vec4(0.0, 0.0, 0.0, 2.0), vec4(1.0, 0.0, 0.0, 2.0),
	vec4(0.0, 1.0, 0.0, 0.0), vec4(0.0, 1.0, 1.0, 0.0), vec4(0.0, 1.0, 0.0, 2.0), vec4(1.0, 1.0, 0.0, 2.0),
	vec4(0.0, 0.0, 0.0, 1.0), vec4(1.0, 0.0, 0.0, 1.0), vec4(0.0, 0.0, 1.0, 1.0), vec4(1.0, 0.0, 1.0, 1.0)
);

void main() {
	int edgeID = gl_VertexID / 36;
	int vertID = gl_VertexID % 36;
	int localCubeIndex = cuboid_indices[vertID];

	vec4 blueprint = edge_blueprints[edgeID];
	vec3 edgeStart = blueprint.xyz;
	int stretchAxis = int(blueprint.w);

	vec3 rawUnitVert = box_vertices[localCubeIndex];

	vec3 localizedPos = edgeStart;
	if (stretchAxis == 0) localizedPos.x += rawUnitVert.x;
	else if (stretchAxis == 1) localizedPos.y += rawUnitVert.y;
	else if (stretchAxis == 2) localizedPos.z += rawUnitVert.z;

	gl_Position = projection * view * vec4(localizedPos + offset, 1.0);

	float baseThickness = 0.003;
	
	float thickness = baseThickness * gl_Position.w;

	vec3 thicknessOffset = vec3(0.0);
	
	if (stretchAxis == 0) {
		thicknessOffset.y = (rawUnitVert.y - 0.5) * thickness;
		thicknessOffset.z = (rawUnitVert.z - 0.5) * thickness;
	} else if (stretchAxis == 1) {
		thicknessOffset.x = (rawUnitVert.x - 0.5) * thickness;
		thicknessOffset.z = (rawUnitVert.z - 0.5) * thickness;
	} else {
		thicknessOffset.x = (rawUnitVert.x - 0.5) * thickness;
		thicknessOffset.y = (rawUnitVert.y - 0.5) * thickness;
	}

	vec4 clipOffset = projection * view * vec4(thicknessOffset, 0.0);
	gl_Position += clipOffset;
}
