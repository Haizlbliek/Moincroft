public static class ARGB {
	public static int ColorFromFloat(float a, float r, float g, float b) {
		return Color(As8BitChannel(a), As8BitChannel(r), As8BitChannel(g), As8BitChannel(b));
	}

	public static int Color(int a, int r, int g, int b) {
		return (a & 0xFF) << 24 | (r & 0xFF) << 16 | (g & 0xFF) << 8 | (b & 0xFF);
	}

	public static int As8BitChannel(float value) {
		return Mathf.FloorToInt(value * 255f);
	}
}