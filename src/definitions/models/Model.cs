namespace Moincroft.Definitions.Models;

public class Model {
	public Quad[] quads = [];

	public struct Quad {
		public Vector3 from, to;
		public float u0, v0, u1, v1;
		public Direction direction;
		public Direction cullFace;
	}
}