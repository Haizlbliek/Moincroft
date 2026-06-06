namespace Moincroft.Definitions.Blocks;

public class RedstoneLampBlock : Block {
	public static readonly BooleanProperty LIT = RedstoneTorchBlock.LIT;

	public override Property[] Properties => [ ..base.Properties, LIT ];

	public RedstoneLampBlock(BlockData data) : base(data) {
	}
}