#version 330 core

uniform sampler2D uTexture;

in vec2 texCoord;
in float ambientOcclusion;

out vec4 out_color;

void main() {
	vec2 blockUv = vec2(4.0, 7.0);
	out_color = texture(uTexture, (texCoord * (16.0 / 18.0) + vec2(1.0 / 18.0) + blockUv) / vec2(32.0, 34.0));
	out_color.rgb *= ambientOcclusion / 6.0 + 0.5;
}