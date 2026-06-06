namespace Moincroft.Definitions;


public class ObserverBlock : DirectionalBlock {
	public static readonly BooleanProperty POWERED = BlockStateProperties.POWERED;

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public ObserverBlock(BlockData data) : base(data) {}
}