namespace Moincroft.Definitions.Blocks;

public static class BlockStateProperties {
	public static readonly BooleanProperty POWERED = new("powered");
	public static readonly IntegerProperty POWER = new("power", 0, 15);
	public static readonly EnumProperty<RedstoneSide> EAST_REDSTONE = new("east");
	public static readonly EnumProperty<RedstoneSide> NORTH_REDSTONE = new("north");
	public static readonly EnumProperty<RedstoneSide> SOUTH_REDSTONE = new("south");
	public static readonly EnumProperty<RedstoneSide> WEST_REDSTONE = new("west");
	public static readonly BooleanProperty LOCKED = new("locked");
	public static readonly IntegerProperty DELAY = new("delay", 1, 4);
	public static readonly EnumProperty<ComparatorMode> MODE_COMPARATOR = new("mode");

	public enum RedstoneSide {
		Up,
		Side,
		None,
	}

	public enum ComparatorMode {
		Compare,
		Subtract,
	}
}