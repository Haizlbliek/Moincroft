namespace Moincroft.Definitions.Blocks;

public class RedstoneWallTorchBlock : RedstoneTorchBlock {
	public static readonly EnumProperty<Direction> FACING = HorizontalDirectionalBlock.FACING;

	public override Property[] Properties => [ ..base.Properties, FACING ];

	public RedstoneWallTorchBlock(BlockData data) : base(data) {
	}
}