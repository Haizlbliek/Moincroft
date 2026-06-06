namespace Moincroft.Definitions.Blocks;

public class RedstoneWireBlock : Block {
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> EAST = BlockStateProperties.EAST_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> NORTH = BlockStateProperties.NORTH_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> SOUTH = BlockStateProperties.SOUTH_REDSTONE;
	public static readonly EnumProperty<BlockStateProperties.RedstoneSide> WEST = BlockStateProperties.WEST_REDSTONE;
	public static readonly IntegerProperty POWER = BlockStateProperties.POWER;

	public override Property[] Properties => [ ..base.Properties, NORTH, EAST, SOUTH, WEST, POWER ];

	private static readonly int[] Colors;
	static RedstoneWireBlock() {
		Colors = new int[16];

		for (int i = 0; i <= 15; i++) {
			float power = i / 15f;
			float red = power * 0.6f + (power > 0.0f ? 0.4f : 0.3f);
			float green = Math.Clamp(power * power * 0.7f - 0.5f, 0.0f, 1.0f);
			float blue = Math.Clamp(power * power * 0.6f - 0.7f, 0.0f, 1.0f);
			Colors[i] = ARGB.ColorFromFloat(1f, red, green, blue);
		}
	}

	public static int GetColorForPower(int power) {
		return Colors[power];
	}

	public RedstoneWireBlock(BlockData data) : base(data) {
	}
}