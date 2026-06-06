namespace Moincroft.Definitions.Models;

public class Model {
	public Quad[] quads = [];
	// LATER: ambientocclusion

	public struct Quad {
		public Vector3 from, to;
		public Vector2 uv0, uv1, uv2, uv3;
		public Direction direction;
		public Direction cullFace;

		public Vector3 rotation;
		public Vector3 rotationOrigin;
		public bool rotationRescale; // TODO

		// LATER: shade
		// LATER: lightEmission
		// LATER REVIEW: tintIndex
		// NOTE: tintIndex is required for quad to be tinted.
	}
}