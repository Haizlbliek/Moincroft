namespace Moincroft.Definitions;

public enum HorizontalFacing {
	North,
	East,
	South,
	West,
}

public class HorizontalDirectionalBlock : Block {
	public static readonly EnumProperty<HorizontalFacing> FACING = new("facing");

	public override Property[] Properties => [ FACING ];

	public HorizontalDirectionalBlock(BlockData data) : base(data) {}
}