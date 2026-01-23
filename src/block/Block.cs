namespace Moincroft.Blocks;

public class Block {
	public Properties properties;
	public BlockId index;

	public Block() {
	}

	public class Properties {
		public bool opaque;
	}

	public class VisibleProperties : Properties {
		public FaceUv px, nx, py, ny, pz, nz;

		public VisibleProperties(FaceUv px, FaceUv nx, FaceUv py, FaceUv ny, FaceUv pz, FaceUv nz) {
			this.px = px;
			this.nx = nx;
			this.py = py;
			this.ny = ny;
			this.pz = pz;
			this.nz = nz;
		}

		public static VisibleProperties XYZ(FaceUv x, FaceUv y, FaceUv z) {
			return new VisibleProperties(x, x, y, y, z, z);
		}

		public static VisibleProperties LogX(FaceUv side, FaceUv pos, FaceUv neg) {
			return new VisibleProperties(pos, neg, side, side, side, side);
		}

		public static VisibleProperties LogY(FaceUv side, FaceUv pos, FaceUv neg) {
			return new VisibleProperties(side, side, pos, neg, side, side);
		}

		public static VisibleProperties LogZ(FaceUv side, FaceUv pos, FaceUv neg) {
			return new VisibleProperties(side, side, side, side, pos, neg);
		}

		public static VisibleProperties One(FaceUv face) {
			return new VisibleProperties(face, face, face, face, face, face);
		}
	}
}