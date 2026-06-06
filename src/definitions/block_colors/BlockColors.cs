namespace Moincroft.Definitions.BlockColors;

using Blocks = Blocks.Blocks;

public static class BlockColors {
	private static readonly IBlockTintSource BLANK_LAYER = BlockTintSources.Constant(-1);
	private static readonly Dictionary<BlockId, List<IBlockTintSource>> sources = [];

	public static void Initialize() {
		Register([BlockTintSources.Redstone()], Blocks.REDSTONE_WIRE);
	}

	private static void Register(List<IBlockTintSource> layers, params BlockId[] blocks) {
		foreach (BlockId block in blocks) {
			if (!sources.TryGetValue(block, out List<IBlockTintSource>? blockLayers)) {
				blockLayers = [];
				sources[block] = blockLayers;
			}

			blockLayers.AddRange(layers);
		}
	}

	public static int GetColor(BlockType blockType, int tintIndex) {
		if (!sources.ContainsKey(blockType.Type))
			return -1;

		IBlockTintSource layer = sources[blockType.Type][tintIndex];
		return layer.GetColor(blockType.State);
	}
}