namespace Moincroft.Definitions;

public class DaylightDetectorBlock : Block {
	public static readonly BooleanProperty INVERTED = new BooleanProperty("inverted");

	public override Property[] Properties => [ INVERTED ];

	public DaylightDetectorBlock(BlockData data) : base(data) {}
}