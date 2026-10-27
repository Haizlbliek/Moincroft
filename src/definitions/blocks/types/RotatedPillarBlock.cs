using Moincroft.Definitions;

namespace Moincroft.World;

public class RotatedPillarBlock : Block {
	public static readonly EnumProperty<Axis> AXIS = BlockStateProperties.AXIS;

	public override Property[] Properties => [ ..base.Properties, AXIS ];

	public RotatedPillarBlock(BlockData data) : base(data) {}
	
}