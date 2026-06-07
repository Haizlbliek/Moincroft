namespace Moincroft.Definitions.Blocks;

public class CopperBulbBlock : Block {
	public static readonly BooleanProperty POWERED = BlockStateProperties.POWERED;
	public static readonly BooleanProperty LIT = BlockStateProperties.LIT;

	public override Property[] Properties => [ ..base.Properties, POWERED, LIT ];

	public CopperBulbBlock(BlockData data) : base(data) {
	}
}