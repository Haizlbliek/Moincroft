#version 330 core

uniform sampler2D uTexture;

in vec2 texCoord;
in float ambientOcclusion;
in float light;
flat in int shade;
in vec4 color;

out vec4 out_color;

void main() {
	out_color = texture(uTexture, texCoord) * color;

	if (out_color.a < 0.01) discard;

	out_color.rgb *= ambientOcclusion / 6.0 + 0.5;
	out_color.rgb *= (light / 15.0) * 0.75 + 0.25;

	if (shade == 1) {
		out_color.rgb *= 0.8;
	}
	else if (shade == 2) {
		out_color.rgb *= 0.6;
	}
	else if (shade == 3) {
		out_color.rgb *= 0.5;
	}
}