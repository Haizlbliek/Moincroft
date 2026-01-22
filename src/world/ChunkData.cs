using System.Runtime.CompilerServices;

namespace Moincroft.World;

public class ChunkData {
	public World world;
	public BlockId[] blocks;
	public int cx;
	public int cy;
	public int cz;

	public unsafe ChunkData(World world, int cx, int cy, int cz) {
		this.world = world;
		this.blocks = new BlockId[16 * 16 * 16];
		this.cx = cx;
		this.cy = cy;
		this.cz = cz;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int x, int y, int z, BlockId block) {
		this.blocks[x + y * 16 + z * 256] = block;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId GetBlock(int x, int y, int z) {
		return this.blocks[x + y * 16 + z * 256];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CarefulSetBlock(int x, int y, int z, BlockId block) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return;

		this.SetBlock(x, y, z, block);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId CarefulGetBlock(int x, int y, int z) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return 0;

		return this.GetBlock(x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId GetBlockOutside(int x, int y, int z) {
		if (x < 0 || x > 15 || y < 0 || y > 15 || z < 0 || z > 15) return this.world.GetBlock(x + this.cx * 16, y + this.cy * 16, z + this.cz * 16);

		return this.GetBlock(x, y, z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsVisiblySolid(int x, int y, int z) {
		BlockId type = this.GetBlockOutside(x, y, z);
		return type > 0; // && Blocks.Blocks.blocks[type].opaque;
	}
}