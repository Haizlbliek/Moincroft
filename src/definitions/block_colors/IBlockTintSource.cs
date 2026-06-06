namespace Moincroft.Definitions.BlockColors;

public interface IBlockTintSource {
	public int GetColor(PropertyState state);

	public ISet<IProperty> GetRelevantProperties() => new HashSet<IProperty>();
}