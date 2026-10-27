namespace Moincroft.Definitions.BlockColors;

public static class BlockTintSources {
	public static IBlockTintSource Constant(int color) => new FunctionalTintSource(state => color);

	public static IBlockTintSource Redstone() {
		return new FunctionalTintSource(
			state => RedstoneWireBlock.GetColorForPower(state.Get(RedstoneWireBlock.POWER))
		);
	}

	public static IBlockTintSource GrassBlock() {
		return new FunctionalTintSource(
			state => ARGB.ColorFromFloat(1f, 0.56863f, 0.74118f, 0.34902f)
		);
	}
}