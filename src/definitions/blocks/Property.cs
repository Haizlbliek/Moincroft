global using BlockStateId = ushort;
using System.Collections.Immutable;
using System.ComponentModel;

namespace Moincroft.Definitions.Blocks;

public interface IProperty {
	string Name { get; }
	int ValueCount { get; }
	object GetValueAt(int index);
	int GetIndexOf(object value);
}

public abstract class Property : IProperty {
	public abstract string Name { get; }
	public abstract int ValueCount { get; }

	public abstract int GetIndexOf(object value);
	public abstract object GetValueAt(int index);
}

public abstract class Property<T> : Property where T : notnull {
	public override string Name { get; }
	public ImmutableArray<T> AllowedValues { get; }
	private readonly Dictionary<T, int> _valueToStreamIndex;

	public Property(string name, IEnumerable<T> values) {
		this.Name = name;
		this.AllowedValues = [.. values];
		this._valueToStreamIndex = this.AllowedValues
			.Select((val, idx) => (val, idx))
			.ToDictionary(x => x.val, x => x.idx);
	}

	public override int ValueCount => this.AllowedValues.Length;
	public override object GetValueAt(int index) => this.AllowedValues[index];
	public override int GetIndexOf(object value) => this._valueToStreamIndex[(T) value];
}

public class BooleanProperty : Property<bool> {
	public BooleanProperty(string name) : base(name, [false, true]) {
	}
}

public class IntegerProperty : Property {
	public override string Name { get; }

	private readonly int min;
	private readonly int max;

	public IntegerProperty(string name, int min, int max) {
		this.Name = name;
		this.min = min;
		this.max = max;
	}

	public override int ValueCount => this.max - this.min + 1;

	public override object GetValueAt(int index) {
		if (index >= this.ValueCount)
			throw new ArgumentOutOfRangeException(nameof(index));

		return index + this.min;
	}

	public override int GetIndexOf(object value) {
		int v = (int)value;

		if (v < this.min || v > this.max)
			throw new ArgumentOutOfRangeException(nameof(value));

		return v - this.min;
	}
}

public class EnumProperty<T> : Property<T> where T : struct, Enum {
	public EnumProperty(string name) : base(name, Enum.GetValues<T>()) {
	}
}