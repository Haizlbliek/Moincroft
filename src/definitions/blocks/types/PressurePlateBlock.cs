namespace Moincroft.Definitions.Blocks;

public class PressurePlateBlock : BasePressurePlateBlock {
	public static readonly BooleanProperty POWERED = new("powered");

	public override Property[] Properties => [ ..base.Properties, POWERED ];

	public PressurePlateBlock(BlockData data) : base(data) {
	}
}