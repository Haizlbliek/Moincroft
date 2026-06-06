namespace Moincroft.Definitions;

public readonly struct FaceBasis {
	public readonly BlockPos Front;
	public readonly BlockPos Right;
	public readonly BlockPos Up;

	public FaceBasis(BlockPos front, BlockPos right, BlockPos up) {
		this.Front = front;
		this.Right = right;
		this.Up = up;
	}

	public FaceBasis Rotated(int rx, int ry, int rz) {
		Vector3 front = (Vector3) this.Front;
		Vector3 right = (Vector3) this.Right;
		Vector3 up = (Vector3) this.Up;

		front = Chunk.RotateAroundOrigin(front, rx, ry, rz);
		right = Chunk.RotateAroundOrigin(right, rx, ry, rz);
		up = Chunk.RotateAroundOrigin(up, rx, ry, rz);

		return new FaceBasis(
			new BlockPos(Mathf.RoundToInt(front.x), Mathf.RoundToInt(front.y), Mathf.RoundToInt(front.z)),
			new BlockPos(Mathf.RoundToInt(right.x), Mathf.RoundToInt(right.y), Mathf.RoundToInt(right.z)),
			new BlockPos(Mathf.RoundToInt(up.x), Mathf.RoundToInt(up.y), Mathf.RoundToInt(up.z))
		);
	}
}