namespace Moincroft.Definitions;

public enum HorizontalDirection {
	North,
	East,
	South,
	West,
}

public class HorizontalDirectionBlock : Block {
	public static readonly EnumProperty<HorizontalDirection> DIRECTION = new EnumProperty<HorizontalDirection>("facing");

	public override IProperty[] Properties => [ DIRECTION ];

	public HorizontalDirectionBlock(BlockData data) : base(data) {}
}