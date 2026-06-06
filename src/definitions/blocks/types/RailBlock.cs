namespace Moincroft.Definitions.Blocks;

public class RailBlock : BaseRailBlock {
	public static readonly EnumProperty<RailShape> SHAPE = BlockStateProperties.RAIL_SHAPE;

	public override Property[] Properties => [ ..base.Properties, SHAPE ];

	public RailBlock(BlockData data) : base(data) {
	}
}