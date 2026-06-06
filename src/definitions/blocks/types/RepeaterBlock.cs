namespace Moincroft.Definitions.Blocks;

public class RepeaterBlock : DiodeBlock {
	public static readonly BooleanProperty LOCKED = BlockStateProperties.LOCKED;
	public static readonly IntegerProperty DELAY = BlockStateProperties.DELAY;

	public override Property[] Properties => [ ..base.Properties, LOCKED, DELAY ];

	public RepeaterBlock(BlockData data) : base(data) {
	}
}