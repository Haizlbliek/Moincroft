global using BlockId = ushort;
using System.Runtime.CompilerServices;

namespace Moincroft.Definitions.Blocks;

public class Properties {
	private bool visible = true;
	private RenderLayer renderLayer = RenderLayer.Opaque;
	private BlockStateData blockStateData = null!;
	private string Id = "";
	private bool occludes = true;

	private Properties() {}

	public static Properties Of() => new Properties();

	public BlockData Compress() {
		return new BlockData(visible: this.visible, renderLayer: this.renderLayer, blockStateData: this.blockStateData, id: this.Id, occludes: this.occludes);
	}

	public Properties Air() {
		this.visible = false;
		return this;
	}

	public Properties Transparent() {
		this.renderLayer = RenderLayer.Transparent;
		return this;
	}

	public Properties NonOccluding() {
		this.occludes = false;
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
	public readonly RenderLayer RenderLayer;
	public readonly bool Occludes;
	public readonly BlockStateData BlockStateData;
	public readonly string Id;

	public BlockData(bool visible, RenderLayer renderLayer, bool occludes, BlockStateData blockStateData, string id) {
		this.Visible = visible;
		this.RenderLayer = renderLayer;
		this.Occludes = occludes;
		this.BlockStateData = blockStateData;
		this.Id = id;
	}
}

public static class BlockRegistry {
	private static Block[] _blocks = new Block[256];
	public static int Count { get; private set; }

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
	public static readonly BlockId COBBLESTONE = BlockRegistry.Register("cobblestone", d => new Block(d), Properties.Of());
	public static readonly BlockId STONE = BlockRegistry.Register("stone", d => new Block(d), Properties.Of());
	public static readonly BlockId DIRT = BlockRegistry.Register("dirt", d => new Block(d), Properties.Of());
	public static readonly BlockId REDSTONE_BLOCK = BlockRegistry.Register("redstone_block", d => new Block(d), Properties.Of());
	public static readonly BlockId DRAGON_EGG = BlockRegistry.Register("dragon_egg", d => new Block(d), Properties.Of().NonOccluding());
	public static readonly BlockId SLIME_BLOCK = BlockRegistry.Register("slime_block", d => new Block(d), Properties.Of().Transparent().NonOccluding());
	public static readonly BlockId DAYLIGHT_DETECTOR = BlockRegistry.Register("daylight_detector", d => new DaylightDetectorBlock(d), Properties.Of().NonOccluding());
	public static readonly BlockId CARVED_PUMPKIN = BlockRegistry.Register("carved_pumpkin", d => new HorizontalDirectionalBlock(d), Properties.Of());
	public static readonly BlockId FLETCHING_TABLE = BlockRegistry.Register("fletching_table", d => new Block(d), Properties.Of());
	public static readonly BlockId DROPPER = BlockRegistry.Register("dropper", d => new DirectionalBlock(d), Properties.Of());
	public static readonly BlockId STONE_BUTTON = BlockRegistry.Register("stone_button", d => new ButtonBlock(d), Properties.Of().NonOccluding());
	public static readonly BlockId LEVER = BlockRegistry.Register("lever", d => new LeverBlock(d), Properties.Of().NonOccluding());
	public static readonly BlockId OBSERVER = BlockRegistry.Register("observer", d => new ObserverBlock(d), Properties.Of());
	public static readonly BlockId STONE_PRESSURE_PLATE = BlockRegistry.Register("stone_pressure_plate", d => new PressurePlateBlock(d), Properties.Of().NonOccluding());
	public static readonly BlockId REDSTONE_TORCH = BlockRegistry.Register("redstone_torch", d => new RedstoneTorchBlock(d), Properties.Of().NonOccluding());
	public static readonly BlockId REDSTONE_WALL_TORCH = BlockRegistry.Register("redstone_wall_torch", d => new RedstoneWallTorchBlock(d), Properties.Of().NonOccluding());

	public static void Initialize() {}
}