namespace Moincroft.Definitions.Blocks;

public class GrassBlock : Block {
	public static readonly BooleanProperty SNOWY = BlockStateProperties.SNOWY;

	public override Property[] Properties => [ ..base.Properties, SNOWY ];

	public GrassBlock(BlockData data) : base(data) {
	}
}