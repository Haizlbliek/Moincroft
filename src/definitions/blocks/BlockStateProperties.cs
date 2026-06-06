namespace Moincroft.Definitions.Blocks;

public static class BlockStateProperties {
	public static readonly IntegerProperty POWER = new("power", 0, 15);
	public static readonly EnumProperty<RedstoneSide> EAST_REDSTONE = new("east");
	public static readonly EnumProperty<RedstoneSide> NORTH_REDSTONE = new("north");
	public static readonly EnumProperty<RedstoneSide> SOUTH_REDSTONE = new("south");
	public static readonly EnumProperty<RedstoneSide> WEST_REDSTONE = new("west");

	public enum RedstoneSide {
		Up,
		Side,
		None,
	}
}