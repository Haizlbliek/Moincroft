using System.Runtime.CompilerServices;

namespace Moincroft.World;

public class ChunkData {
	public World world;
	public BlockId[] blocks = new BlockId[16 * 16 * 16];
	public byte[] light = new byte[16 * 16 * 16];

	public int cx;
	public int cy;
	public int cz;

	public ChunkData(World world, int cx, int cy, int cz) {
		this.world = world;
		this.cx = cx;
		this.cy = cy;
		this.cz = cz;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int GetIdx(int x, int y, int z) => (x << 8) | (y << 4) | z;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlock(int x, int y, int z, BlockId block) {
		ref BlockId blockRef = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.blocks), GetIdx(x, y, z));
		blockRef = block;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockId GetBlock(int x, int y, int z) {
		return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.blocks), GetIdx(x, y, z));
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

#region Lighting

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetSunlight(int x, int y, int z) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.light), GetIdx(x, y, z)) >> 4;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBlockLight(int x, int y, int z) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.light), GetIdx(x, y, z)) & 0xF;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetSunlight(int x, int y, int z, byte val) {
		int i = GetIdx(x, y, z);
		ref byte lightRef = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.light), i);
		lightRef = (byte)((lightRef & 0x0F) | (val << 4));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBlockLight(int x, int y, int z, byte val) {
		int i = GetIdx(x, y, z);
		ref byte lightRef = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(this.light), i);
		lightRef = (byte)((lightRef & 0xF0) | (val & 0x0F));
	}

	public bool CalculateLight() {
		ChunkData? chunkAbove = this.world.GetChunk(this.cx, this.cy + 1, this.cz);
		ref byte lightBase = ref MemoryMarshal.GetArrayDataReference(this.light);
		ref BlockId blockBase = ref MemoryMarshal.GetArrayDataReference(this.blocks);

		bool recalculateBelow = false;

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				byte lightLevel = (byte) (chunkAbove?.GetBlockLight(x, 0, z) ?? 15);

				for (int y = 15; y >= 0; y--) {
					int i = GetIdx(x, y, z);

					ref byte currentLight = ref Unsafe.Add(ref lightBase, i);
					ref BlockId currentBlock = ref Unsafe.Add(ref blockBase, i);

					if (currentBlock != 0) {
						lightLevel = 0;
					}

					if (y == 0 && currentLight != lightLevel) {
						recalculateBelow = recalculateBelow || true;
					}
					currentLight = lightLevel;
				}
			}
		}

		return recalculateBelow;
	}
#endregion
}