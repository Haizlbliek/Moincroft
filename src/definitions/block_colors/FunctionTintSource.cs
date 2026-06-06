namespace Moincroft.Definitions.BlockColors;

public class FunctionalTintSource : IBlockTintSource {
	private readonly Func<PropertyState, int> _colorFunc;
	private readonly ISet<IProperty> _properties;

	public FunctionalTintSource(Func<PropertyState, int> colorFunc, ISet<IProperty>? properties = null) {
		this._colorFunc = colorFunc ?? throw new ArgumentNullException(nameof(colorFunc));
		this._properties = properties ?? new HashSet<IProperty>();
	}

	public int GetColor(PropertyState state) => this._colorFunc(state);

	public ISet<IProperty> GetRelevantProperties() => this._properties;
}
