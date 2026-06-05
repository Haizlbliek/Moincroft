namespace Moincroft.Definitions;

public class DaylightDetectorBlock : Block {
	public static readonly BooleanProperty INVERTED = new BooleanProperty("inverted");

	public override IProperty[] Properties => [ INVERTED ];

	public DaylightDetectorBlock(BlockData data) : base(data) {}
}