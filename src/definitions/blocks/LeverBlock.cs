namespace Moincroft.Definitions;


public class LeverBlock : FaceAttachedHorizontalDirectionalBlock {
	public static readonly BooleanProperty POWERED = new("powered");

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public LeverBlock(BlockData data) : base(data) {}
}