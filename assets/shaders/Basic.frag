#version 330 core

uniform sampler2D uTexture;

in vec2 texCoord;
in float ambientOcclusion;
in float light;
in vec4 color;

out vec4 out_color;

void main() {
	out_color = texture(uTexture, texCoord) * color;

	if (out_color.a < 0.01) discard;

	out_color.rgb *= ambientOcclusion / 6.0 + 0.5;
	out_color.rgb *= (light / 15.0) * 0.75 + 0.25;
}