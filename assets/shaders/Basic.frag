#version 330 core

uniform sampler2D uTexture;

in vec2 texCoord;

out vec4 out_color;

void main() {
	out_color = texture(uTexture, (texCoord + vec2(1.0 / 18.0)) / vec2(32.0, 34.0) * (16.0 / 18.0));
}