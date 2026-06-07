namespace Moincroft.Definitions;

public class DaylightDetectorBlock : Block {
	public static readonly IntegerProperty POWER = BlockStateProperties.POWER;
	public static readonly BooleanProperty INVERTED = BlockStateProperties.INVERTED;

	public override Property[] Properties => [ ..base.Properties, POWER, INVERTED ];

	public DaylightDetectorBlock(BlockData data) : base(data) {}
}