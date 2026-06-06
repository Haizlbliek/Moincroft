namespace Moincroft.Definitions;

public enum AttachFace {
	Floor,
	Wall,
	Ceiling
}

public class FaceAttachedHorizontalDirectionalBlock : HorizontalDirectionalBlock {
	public static readonly EnumProperty<AttachFace> FACE = new("face");

	public override Property[] Properties => [ ..base.Properties, FACE ];

	public FaceAttachedHorizontalDirectionalBlock(BlockData data) : base(data) {}
}