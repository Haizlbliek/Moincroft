namespace Moincroft.Definitions.Blocks;

public class RedstoneTorchBlock : BaseTorchBlock {
	public static readonly BooleanProperty LIT = new("lit");

	public override Property[] Properties => [ ..base.Properties, LIT ];

	public RedstoneTorchBlock(BlockData data) : base(data) {
	}
}