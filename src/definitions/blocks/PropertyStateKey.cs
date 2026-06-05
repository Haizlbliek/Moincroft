using System.Diagnostics.CodeAnalysis;

namespace Moincroft.Definitions.Blocks;

public readonly struct PropertyStateKey : IEquatable<PropertyStateKey> {
	private readonly string[] _keys;
	private readonly string[] _values;
	private readonly int _hashCode;

	public PropertyStateKey(Dictionary<string, string> values) {
		KeyValuePair<string, string>[] orderedPairs = values.OrderBy(kvp => kvp.Key).ToArray();

		_keys = new string[orderedPairs.Length];
		_values = new string[orderedPairs.Length];

		if (_keys.Length == 0) {
			_hashCode = 0;
			return;
		}

		HashCode hash = new HashCode();
		for (int i = 0; i < orderedPairs.Length; i++) {
			_keys[i] = orderedPairs[i].Key;
			_values[i] = orderedPairs[i].Value;
			hash.Add(orderedPairs[i].Key);
			hash.Add(orderedPairs[i].Value);
		}
		_hashCode = hash.ToHashCode();
	}

	public PropertyStateKey(string from) {
		if (string.IsNullOrEmpty(from)) {
			_keys = [];
			_values = [];
			_hashCode = 0;
			return;
		}

		KeyValuePair<string, string>[] orderedPairs = from.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(x => x.Split('='))
			.Select(parts => new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()))
			.OrderBy(kvp => kvp.Key)
			.ToArray();

		_keys = new string[orderedPairs.Length];
		_values = new string[orderedPairs.Length];

		HashCode hash = new HashCode();
		for (int i = 0; i < orderedPairs.Length; i++) {
			_keys[i] = orderedPairs[i].Key;
			_values[i] = orderedPairs[i].Value;
			hash.Add(orderedPairs[i].Key);
			hash.Add(orderedPairs[i].Value);
		}
		_hashCode = hash.ToHashCode();
	}

	public bool Equals(PropertyStateKey other) {
		if (_hashCode != other._hashCode) return false;
		if (_keys.Length != other._keys.Length) return false;

		for (int i = 0; i < _keys.Length; i++) {
			if (_keys[i] != other._keys[i] || _values[i] != other._values[i]) {
				return false;
			}
		}

		return true;
	}

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is PropertyStateKey key && this.Equals(key);
	public override int GetHashCode() => this._hashCode;

	public static bool operator ==(PropertyStateKey left, PropertyStateKey right) => left.Equals(right);
	public static bool operator !=(PropertyStateKey left, PropertyStateKey right) => !left.Equals(right);

	public override string ToString() {
		if (_keys == null || _keys.Length == 0) {
			return "";
		}
		return string.Join(",", _keys.Zip(_values, (k, v) => $"{k}={v}"));
	}
}
