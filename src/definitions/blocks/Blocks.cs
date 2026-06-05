global using BlockId = ushort;
using System.Runtime.CompilerServices;
using Moincroft.Definitions.Models;

namespace Moincroft.Definitions.Blocks;

public class Properties {
	private bool visible = true;
	private bool opaque = true;
	private BlockStateData blockStateData = null!;
	private string Id = "";

	private Properties() {}

	public static Properties Of() => new Properties();

	public BlockData Compress() {
		return new BlockData(visible: this.visible, opaque: this.opaque, blockStateData: this.blockStateData, id: this.Id);
	}

	public Properties Air() {
		this.visible = false;
		this.opaque = false;
		return this;
	}

	public Properties BlockStateData(BlockStateData blockStateData) {
		this.blockStateData = blockStateData;
		return this;
	}

	public Properties ID(string id) {
		this.Id = id;
		return this;
	}
}

public readonly struct BlockData {
	public readonly bool Visible;
	public readonly bool Opaque;
	public readonly BlockStateData BlockStateData;
	public readonly string Id;

	public BlockData(bool visible, bool opaque, BlockStateData blockStateData, string id) {
		this.Visible = visible;
		this.Opaque = opaque;
		this.BlockStateData = blockStateData;
		this.Id = id;
	}
}

public static class BlockRegistry {
	private static Block[] _blocks = new Block[256];
	private static int Count;

	public static BlockId Register(string id, Func<BlockData, Block> factory, Properties properties, BlockId? forcedId = null) {
		BlockId index = forcedId == null ? (BlockId)Count++ : forcedId.Value;

		if (index >= _blocks.Length) {
			Array.Resize(ref _blocks, _blocks.Length * 2);
		}

		BlockStateData blockStateData = BlockStateLoader.GetBlockStateData(id);

		BlockData data = properties.ID(id).BlockStateData(blockStateData).Compress();
		Block block = factory(data);

		_blocks[index] = block;

		if (index >= Count) Count = index + 1;
		return index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Block GetBlock(BlockId id) => _blocks[id];
}

public static class Blocks {
	public static readonly BlockId AIR = BlockRegistry.Register("air", d => new Block(d), Properties.Of().Air(), forcedId: 0);
	// public static readonly BlockId COBBLESTONE = BlockRegistry.Register("cobblestone", d => new Block(d), Properties.Of());
	public static readonly BlockId STONE = BlockRegistry.Register("stone", d => new Block(d), Properties.Of());
	// public static readonly BlockId DIRT = BlockRegistry.Register("dirt", d => new Block(d), Properties.Of());
	// public static readonly BlockId REDSTONE_BLOCK = BlockRegistry.Register("redstone_block", d => new Block(d), Properties.Of());
	// public static readonly BlockId DRAGON_EGG = BlockRegistry.Register("dragon_egg", d => new Block(d), Properties.Of());
	// public static readonly BlockId SLIME_BLOCK = BlockRegistry.Register("slime_block", d => new Block(d), Properties.Of());
	public static readonly BlockId DAYLIGHT_DETECTOR = BlockRegistry.Register("daylight_detector", d => new DaylightDetectorBlock(d), Properties.Of());

	public static void Initialize() {}
}