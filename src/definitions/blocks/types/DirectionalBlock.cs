namespace Moincroft.Definitions;

public class DirectionalBlock : Block {
	public static readonly EnumProperty<Direction> FACING = BlockStateProperties.FACING;

	public override Property[] Properties => [ ..base.Properties, FACING ];

	public DirectionalBlock(BlockData data) : base(data) {}
}