global using BlockId = uint;

namespace Moincroft.Blocks;

public static class Blocks {
	public static Dictionary<string, BlockId> blockIds = [];
	public static Dictionary<BlockId, Block> blocks = [];

	public static readonly Block AIR = Register<Block>("air", new Block.Properties());
	public static readonly Block COBBLESTONE = Register<Block>("cobblestone", Block.VisibleProperties.One(Atlas.GetFace("cobblestone")));
	public static readonly Block STONE = Register<Block>("stone", Block.VisibleProperties.One(Atlas.GetFace("stone")));
	public static readonly Block DIRT = Register<Block>("dirt", Block.VisibleProperties.One(Atlas.GetFace("dirt")));

	public static T Register<T>(string key, Block.Properties properties) where T : Block, new() {
		T block = new T {
			properties = properties,
			index = (BlockId) blockIds.Count
		};

		blockIds.Add(key, (BlockId) blocks.Count);
		blocks.Add((BlockId) blocks.Count, block);
		return block;
	}

	public static void Initialize() {
	}
}