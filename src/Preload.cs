using Moincroft.Definitions;

namespace Moincroft;

public static class Preload {
	public static Shader Basic = null!;
	public static Shader Selection = null!;

	//  1   2   4 
	//
	// 128  -   8 
	//
	//  64  32  16
	public static (byte, byte, byte, byte)[] AmbientOcclusionVertexLUT = null!;
	public static Direction[,,,] RotationLUT = null!;

	public static readonly FaceBasis[] FaceBases = [
		new FaceBasis(new( 0, 0,-1), new( 1, 0, 0), new( 0, 1, 0)), // NZ
		new FaceBasis(new( 0, 0, 1), new(-1, 0, 0), new( 0, 1, 0)), // PZ
		new FaceBasis(new( 1, 0, 0), new( 0, 0, 1), new( 0, 1, 0)), // PX
		new FaceBasis(new(-1, 0, 0), new( 0, 0,-1), new( 0, 1, 0)), // NX
		new FaceBasis(new( 0, 1, 0), new( 1, 0, 0), new( 0, 0, 1)), // PY
		new FaceBasis(new( 0,-1, 0), new( 1, 0, 0), new( 0, 0,-1)), // NY
	];

	public static void Initialize() {
		Basic = Shader.Load("assets/shaders/Basic.vert", "assets/shaders/Basic.frag");
		Selection = Shader.Load("assets/shaders/Selection.vert", "assets/shaders/Selection.frag");

		Program.gl.UseProgram(Basic);
		Basic.SetUniform("uTexture", 0);
		Program.gl.UseProgram(0);

		AmbientOcclusionVertexLUT = PrecomputeAmbientOcclusionLUT();
		RotationLUT = PrecomputeRotationLUT();
	}

	public static (byte, byte, byte, byte)[] PrecomputeAmbientOcclusionLUT() {
		(byte, byte, byte, byte)[] table = new (byte, byte, byte, byte)[256];

		for (int mask = 0; mask < 256; mask++) {
			bool c0 = (mask & 1) != 0;
			bool s0 = (mask & 2) != 0;
			bool c1 = (mask & 4) != 0;
			bool s1 = (mask & 8) != 0;
			bool c2 = (mask & 16) != 0;
			bool s2 = (mask & 32) != 0;
			bool c3 = (mask & 64) != 0;
			bool s3 = (mask & 128) != 0;

			table[mask] = (
				CalculateVertexAO(s3, s0, c0),
				CalculateVertexAO(s0, s1, c1),
				CalculateVertexAO(s2, s3, c3),
				CalculateVertexAO(s1, s2, c2)
			);
		}
		return table;
	}

	private static byte CalculateVertexAO(bool side1, bool side2, bool corner) {
		if (side1 && side2)
			return 0;

		int s1 = side1 ? 1 : 0;
		int s2 = side2 ? 1 : 0;
		int c = corner ? 1 : 0;

		return (byte) (3 - (s1 + s2 + c));
	}

	public static Direction Rotate(Direction direction, int x, int y, int z) {
		return RotationLUT[(int) direction, x & 3, y & 3, z & 3];
	}

	private static (int x, int y, int z) DirToVec(Direction d) {
		return d switch {
			Direction.NZ => (0, 0, -1),
			Direction.PZ => (0, 0, 1),
			Direction.PX => (1, 0, 0),
			Direction.NX => (-1, 0, 0),
			Direction.PY => (0, 1, 0),
			Direction.NY => (0, -1, 0),
			_ => (0, 0, 0)
		};
	}

	private static Direction VecToDir((int x, int y, int z) v) {
		if (v.x == 1) return Direction.PX;
		if (v.x == -1) return Direction.NX;
		if (v.y == 1) return Direction.PY;
		if (v.y == -1) return Direction.NY;
		if (v.z == 1) return Direction.PZ;
		if (v.z == -1) return Direction.NZ;

		return Direction.None;
	}

	private static (int x, int y, int z) RotateX((int x, int y, int z) v, int times) {
		for (int i = 0; i < times; i++) {
			// REVIEW
			v = (v.x, -v.z, v.y);
		}
		return v;
	}

	private static (int x, int y, int z) RotateY((int x, int y, int z) v, int times) {
		for (int i = 0; i < times; i++) {
			v = (-v.z, v.y, v.x);
		}
		return v;
	}

	private static (int x, int y, int z) RotateZ((int x, int y, int z) v, int times) {
		for (int i = 0; i < times; i++) {
			// REVIEW
			v = (-v.y, v.x, v.z);
		}
		return v;
	}

	public static Direction[,,,] PrecomputeRotationLUT() {
		Direction[,,,] table = new Direction[6, 4, 4, 4];

		for (int d = 0; d < 6; d++) {
			(int x, int y, int z) v = DirToVec((Direction) d);

			for (int x = 0; x < 4; x++) {
				for (int y = 0; y < 4; y++) {
					for (int z = 0; z < 4; z++) {

						(int x, int y, int z) r = v;

						r = RotateX(r, x);
						r = RotateY(r, y);
						r = RotateZ(r, z);

						table[d, x, y, z] = VecToDir(r);
					}
				}
			}
		}

		return table;
	}
}