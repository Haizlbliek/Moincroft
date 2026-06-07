using System.Runtime.CompilerServices;
using Moincroft.Definitions;

namespace Moincroft.World;

public class ChunkData {
	public World world;
	public PalettedContainer<BlockType> blocks = new PalettedContainer<BlockType>(new BlockType());
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
	public void SetBlock(BlockPos pos, BlockType block) {
		this.blocks.Set(pos, block);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockType GetBlock(BlockPos pos) {
		return this.blocks.Get(pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void CarefulSetBlock(BlockPos pos, BlockType block) {
		if (pos.x < 0 || pos.x > 15 || pos.y < 0 || pos.y > 15 || pos.z < 0 || pos.z > 15) return;

		this.SetBlock(pos, block);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockType CarefulGetBlock(BlockPos pos) {
		if (pos.x < 0 || pos.x > 15 || pos.y < 0 || pos.y > 15 || pos.z < 0 || pos.z > 15) return default;

		return this.GetBlock(pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BlockType GetBlockOutside(BlockPos pos) {
		if (pos.x < 0 || pos.x > 15 || pos.y < 0 || pos.y > 15 || pos.z < 0 || pos.z > 15) return this.world.GetBlock(pos.x + this.cx * 16, pos.y + this.cy * 16, pos.z + this.cz * 16);

		return this.GetBlock(pos);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsVisiblySolid(BlockPos pos) {
		BlockType type = this.GetBlockOutside(pos);
		return this.IsVisiblySolid(type);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsVisiblySolid(BlockType type) {
		if (type.Type == 0) return false;
		BlockData data = BlockRegistry.GetBlock(type.Type).data;
		return data.Occludes;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Occludes(BlockType type, BlockType self) {
		if (type.Type == 0) return false;
		BlockData data = BlockRegistry.GetBlock(type.Type).data;
		BlockData selfData = BlockRegistry.GetBlock(self.Type).data;
		return data.Occludes && (data.RenderLayer == selfData.RenderLayer);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Occludes(BlockType type, BlockData selfData) {
		if (type.Type == 0) return false;
		BlockData data = BlockRegistry.GetBlock(type.Type).data;
		return data.Occludes && (data.RenderLayer == selfData.RenderLayer);
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

		bool recalculateBelow = false;

		for (int z = 0; z < 16; z++) {
			for (int x = 0; x < 16; x++) {
				byte lightLevel = (byte) (chunkAbove?.GetBlockLight(x, 0, z) ?? 15);

				for (int y = 15; y >= 0; y--) {
					int i = GetIdx(x, y, z);
					BlockPos pos = new BlockPos(x, y, z);

					ref byte currentLight = ref Unsafe.Add(ref lightBase, i);
					BlockType currentBlock = this.blocks.Get(pos);
					BlockData data = BlockRegistry.GetBlock(currentBlock.Type).data;

					if (data.Occludes && data.Visible) {
						lightLevel = 0;
					}
					else if (currentBlock.Type != 0) {
						if (lightLevel > 0) lightLevel--;
					}

					if (y == 0 && currentLight != lightLevel) {
						recalculateBelow = true;
					}
					currentLight = lightLevel;
				}
			}
		}

		return recalculateBelow;
	}
#endregion
}