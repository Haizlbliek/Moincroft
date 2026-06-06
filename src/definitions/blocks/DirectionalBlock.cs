namespace Moincroft.Definitions;

public enum Directional {
	North,
	East,
	South,
	West,
	Up,
	Down,
}

public class DirectionalBlock : Block {
	public static readonly EnumProperty<Directional> FACING = new EnumProperty<Directional>("facing");

	public override Property[] Properties => [ FACING ];

	public DirectionalBlock(BlockData data) : base(data) {}
}