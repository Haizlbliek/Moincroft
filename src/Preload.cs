namespace Moincroft;

public static class Preload {
	public static Shader Basic;
	public static Shader Selection;
	public static Texture atlas;

	public static (byte, byte, byte, byte)[] AO_LUT;
	public static sbyte[][] FaceNeighbours_LOT;

	public static void Initialize() {
		Basic = Shader.Load("assets/shaders/Basic.vert", "assets/shaders/Basic.frag");
		Selection = Shader.Load("assets/shaders/Selection.vert", "assets/shaders/Selection.frag");
		atlas = Texture.Load("assets/texture_atlas.png");

		Program.gl.UseProgram(Basic);
		int loc = Program.gl.GetUniformLocation(Basic, "uTexture");
		Program.gl.Uniform1(loc, 0);
		Program.gl.UseProgram(0);

		AO_LUT = PrecomputeAOTable();
		FaceNeighbours_LOT = [
			[ 1, -1,  0,    1,  1,  0,    1,  0, -1,    1,  0,  1,    1, -1, -1,    1,  1, -1,    1, -1,  1,    1,  1,  1],
			[-1, -1,  0,   -1,  1,  0,   -1,  0, -1,   -1,  0,  1,   -1, -1, -1,   -1,  1, -1,   -1, -1,  1,   -1,  1,  1],
			[ 1,  1,  0,   -1,  1,  0,    0,  1,  1,    0,  1, -1,    1,  1,  1,   -1,  1,  1,    1,  1, -1,   -1,  1, -1],
			[ 1, -1,  0,   -1, -1,  0,    0, -1,  1,    0, -1, -1,    1, -1,  1,   -1, -1,  1,    1, -1, -1,   -1, -1, -1],
			[ 0, -1,  1,    0,  1,  1,   -1,  0,  1,    1,  0,  1,   -1, -1,  1,   -1,  1,  1,    1, -1,  1,    1,  1,  1],
			[ 0, -1, -1,    0,  1, -1,   -1,  0, -1,    1,  0, -1,   -1, -1, -1,   -1,  1, -1,    1, -1, -1,    1,  1, -1]
		];
	}

	public static (byte, byte, byte, byte)[] PrecomputeAOTable() {
		var table = new (byte, byte, byte, byte)[256];

		for (int mask = 0; mask < 256; mask++) {
			bool c0 = (mask & (1 << 0)) != 0;
			bool s0 = (mask & (1 << 1)) != 0;
			bool c1 = (mask & (1 << 2)) != 0;
			bool s1 = (mask & (1 << 3)) != 0;
			bool c2 = (mask & (1 << 4)) != 0;
			bool s2 = (mask & (1 << 5)) != 0;
			bool c3 = (mask & (1 << 6)) != 0;
			bool s3 = (mask & (1 << 7)) != 0;

			table[mask] = (
				CalculateVertexAO(s0, s2, c0),
				CalculateVertexAO(s0, s3, c2),
				CalculateVertexAO(s1, s2, c1),
				CalculateVertexAO(s1, s3, c3)
			);
		}
		return table;
	}

	private static byte CalculateVertexAO(bool side1, bool side2, bool corner) {
		if (side1 && side2) return 0;
		int s1 = side1 ? 1 : 0;
		int s2 = side2 ? 1 : 0;
		int c = corner ? 1 : 0;
		return (byte)(3 - (s1 + s2 + c));
	}
}