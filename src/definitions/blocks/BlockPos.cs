namespace Moincroft.Definitions;

public readonly struct BlockPos {
	public readonly int x;
	public readonly int y;
	public readonly int z;

	public BlockPos(int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public BlockPos Up(int v = 1) => new BlockPos(this.x, this.y + v, this.z);
	public BlockPos Down(int v = 1) => new BlockPos(this.x, this.y - v, this.z);
	public BlockPos North(int v = 1) => new BlockPos(this.x, this.y, this.z - v);
	public BlockPos South(int v = 1) => new BlockPos(this.x, this.y, this.z + v);
	public BlockPos East(int v = 1) => new BlockPos(this.x + v, this.y, this.z);
	public BlockPos West(int v = 1) => new BlockPos(this.x - v, this.y, this.z);

	public BlockPos PY(int v = 1) => new BlockPos(this.x, this.y + v, this.z);
	public BlockPos NY(int v = 1) => new BlockPos(this.x, this.y - v, this.z);
	public BlockPos NZ(int v = 1) => new BlockPos(this.x, this.y, this.z - v);
	public BlockPos PZ(int v = 1) => new BlockPos(this.x, this.y, this.z + v);
	public BlockPos PX(int v = 1) => new BlockPos(this.x + v, this.y, this.z);
	public BlockPos NX(int v = 1) => new BlockPos(this.x - v, this.y, this.z);

	public readonly Random GetSeededRandom() {
		long seed = (long)this.x * 3129871 ^ this.z * 116129781L ^ this.y;
		seed = seed * seed * 42317861L + seed * 11L;
		return new Random((int)seed);
	}
}