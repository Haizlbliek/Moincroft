namespace Moincroft.Definitions.Models;

public class Model {
	public Quad[] quads = [];
	// LATER: ambientocclusion

	public struct Quad {
		public Vector3 from, to;
		public float u0, v0, u1, v1;
		public Direction direction;
		public Direction cullFace;
		public int faceRotation;

		public Vector3 rotation;
		public Vector3 rotationOrigin;
		public bool rotationRescale; // TODO

		// LATER: shade
		// LATER: lightEmission
		// LATER REVIEW: tintIndex
	}
}