namespace Moincroft.Definitions.BlockColors;

public static class BlockTintSources {
	public static IBlockTintSource Constant(int color) => new FunctionalTintSource(state => color);

	public static IBlockTintSource Redstone() {
		return new FunctionalTintSource(
			state => RedstoneWireBlock.GetColorForPower(state.Get(RedstoneWireBlock.POWER))
		);
	}
}