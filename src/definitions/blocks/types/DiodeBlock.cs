namespace Moincroft.Definitions.Blocks;

public abstract class DiodeBlock : HorizontalDirectionalBlock {
	public static readonly BooleanProperty POWERED = BlockStateProperties.POWERED;

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public DiodeBlock(BlockData data) : base(data) {
	}
}