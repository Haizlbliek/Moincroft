global using BlockStateId = ushort;
using System.Collections.Immutable;

namespace Moincroft.Definitions.Blocks;

public interface IProperty {
	string Name { get; }
	int ValueCount { get; }
	object GetValueAt(int index);
	int GetIndexOf(object value);
}

public abstract class Property<T> : IProperty where T : notnull {
	public string Name { get; }
	public ImmutableArray<T> AllowedValues { get; }
	private readonly Dictionary<T, int> _valueToStreamIndex;

	public Property(string name, IEnumerable<T> values) {
		this.Name = name;
		this.AllowedValues = [.. values];
		this._valueToStreamIndex = this.AllowedValues
			.Select((val, idx) => (val, idx))
			.ToDictionary(x => x.val, x => x.idx);
	}

	public int ValueCount => this.AllowedValues.Length;
	public object GetValueAt(int index) => this.AllowedValues[index];
	public int GetIndexOf(object value) => this._valueToStreamIndex[(T) value];
}

public class BooleanProperty : Property<bool> {
	public BooleanProperty(string name) : base(name, [false, true]) {
	}
}

public class EnumProperty<T> : Property<T> where T : struct, Enum {
	public EnumProperty(string name) : base(name, Enum.GetValues<T>()) {
	}
}