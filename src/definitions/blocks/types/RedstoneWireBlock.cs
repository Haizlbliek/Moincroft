namespace Moincroft.Definitions.Blocks;

public class RedstoneWireBlock : Block {
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> EAST = BlockStateProperties.EAST_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> NORTH = BlockStateProperties.NORTH_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> SOUTH = BlockStateProperties.SOUTH_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> WEST = BlockStateProperties.WEST_REDSTONE;
	public static readonly IntegerProperty POWER = BlockStateProperties.POWER;

	public override Property[] Properties => [ ..base.Properties, NORTH, EAST, SOUTH, WEST, POWER ];

	public RedstoneWireBlock(BlockData data) : base(data) {
	}
}