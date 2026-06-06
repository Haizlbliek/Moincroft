namespace Moincroft.Definitions;


public class LeverBlock : FaceAttachedHorizontalDirectionalBlock {
	public static readonly BooleanProperty POWERED = BlockStateProperties.POWERED;

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public LeverBlock(BlockData data) : base(data) {}
}