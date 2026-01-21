global using BlockId = uint;

namespace Moincroft.Blocks;

public static class Blocks {
	public static Dictionary<string, BlockId> blockIds = [];
	public static Dictionary<BlockId, BlockData> blocks = [];

	public static void AddBlock(BlockData data) {
		blockIds.Add(data.id, (BlockId) blocks.Count);
		blocks.Add((BlockId) blocks.Count, data);
	}

	public static void Initialize() {
		AddBlock(new BlockData<Block>("air"));
		AddBlock(new BlockData<Block>("grass_block"));
	}
}