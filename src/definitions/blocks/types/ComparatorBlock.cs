namespace Moincroft.Definitions.Blocks;

public class ComparatorBlock : DiodeBlock {
	public static readonly EnumProperty<BlockStateProperties.ComparatorMode> MODE = BlockStateProperties.MODE_COMPARATOR;

	public override Property[] Properties => [ ..base.Properties, MODE ];

	public ComparatorBlock(BlockData data) : base(data) {
	}
}