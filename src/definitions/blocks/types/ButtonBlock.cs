namespace Moincroft.Definitions;


public class ButtonBlock : FaceAttachedHorizontalDirectionalBlock {
	public static readonly BooleanProperty POWERED = BlockStateProperties.POWERED;

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public ButtonBlock(BlockData data) : base(data) {}
}