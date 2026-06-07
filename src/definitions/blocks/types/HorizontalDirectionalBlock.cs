namespace Moincroft.Definitions;

public class HorizontalDirectionalBlock : Block {
	public static readonly EnumProperty<Direction> FACING = BlockStateProperties.HORIZONTAL_FACING;

	public override Property[] Properties => [ ..base.Properties, FACING ];

	public HorizontalDirectionalBlock(BlockData data) : base(data) {}
}