namespace Moincroft.Definitions;


public class TargetBlock : Block {
	public static readonly IntegerProperty OUTPUT_POWER = BlockStateProperties.POWER;

	public override Property[] Properties => [ ..base.Properties, OUTPUT_POWER ];

	public TargetBlock(BlockData data) : base(data) {}
}