namespace Moincroft.Definitions.Models;

public class Model {
	public Quad[] quads = [];
	// LATER: ambientocclusion

	public struct Quad {
		public Vector3 v0, v1, v2, v3;
		public Vector2 uv0, uv1, uv2, uv3;
		public Direction direction;
		public Direction cullFace;
		public int tintIndex;
		public int lightEmisson;
		public bool shade;
	}
}