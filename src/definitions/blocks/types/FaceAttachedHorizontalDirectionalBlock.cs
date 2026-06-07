namespace Moincroft.Definitions;


public class FaceAttachedHorizontalDirectionalBlock : HorizontalDirectionalBlock {
	public static readonly EnumProperty<AttachFace> FACE = BlockStateProperties.ATTACH_FACE;

	public override Property[] Properties => [ ..base.Properties, FACE ];

	public FaceAttachedHorizontalDirectionalBlock(BlockData data) : base(data) {}
}