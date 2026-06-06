namespace Moincroft.Definitions.Blocks;

public abstract class BaseRailBlock : Block {
	public static readonly BooleanProperty WATERLOGGED = BlockStateProperties.WATERLOGGED;

	public override Property[] Properties => [ ..base.Properties, WATERLOGGED ];

	public BaseRailBlock(BlockData data) : base(data) {
	}
}