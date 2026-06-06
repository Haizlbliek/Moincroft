using System.Diagnostics.CodeAnalysis;

namespace Moincroft.Definitions.Blocks;

public readonly struct PropertyStateKey : IEquatable<PropertyStateKey> {
	private readonly string[] _keys;
	private readonly string[] _values;
	private readonly int _hashCode;

	public PropertyStateKey(Dictionary<string, string> values) {
		KeyValuePair<string, string>[] orderedPairs = values.OrderBy(kvp => kvp.Key).ToArray();

		this._keys = new string[orderedPairs.Length];
		this._values = new string[orderedPairs.Length];

		if (this._keys.Length == 0) {
			this._hashCode = 0;
			return;
		}

		HashCode hash = new HashCode();
		for (int i = 0; i < orderedPairs.Length; i++) {
			this._keys[i] = orderedPairs[i].Key;
			this._values[i] = orderedPairs[i].Value;
			hash.Add(orderedPairs[i].Key);
			hash.Add(orderedPairs[i].Value);
		}
		this._hashCode = hash.ToHashCode();
	}

	public PropertyStateKey(string from) {
		if (string.IsNullOrEmpty(from)) {
			this._keys = [];
			this._values = [];
			this._hashCode = 0;
			return;
		}

		KeyValuePair<string, string>[] orderedPairs = from.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(x => x.Split('='))
			.Select(parts => new KeyValuePair<string, string>(parts[0].Trim(), parts[1].Trim()))
			.OrderBy(kvp => kvp.Key)
			.ToArray();

		this._keys = new string[orderedPairs.Length];
		this._values = new string[orderedPairs.Length];

		HashCode hash = new HashCode();
		for (int i = 0; i < orderedPairs.Length; i++) {
			this._keys[i] = orderedPairs[i].Key;
			this._values[i] = orderedPairs[i].Value;
			hash.Add(orderedPairs[i].Key);
			hash.Add(orderedPairs[i].Value);
		}
		this._hashCode = hash.ToHashCode();
	}

	public bool Matches(PropertyStateKey targetState) {
		if (this._keys == null || this._keys.Length == 0) return true;
		if (targetState._keys == null || targetState._keys.Length == 0) return false;

		int thisIdx = 0;
		int targetIdx = 0;

		while (thisIdx < this._keys.Length && targetIdx < targetState._keys.Length) {
			int comparison = string.CompareOrdinal(this._keys[thisIdx], targetState._keys[targetIdx]);

			if (comparison == 0) {
				if (this._values[thisIdx] != targetState._values[targetIdx]) {
					return false;
				}

				thisIdx++;
				targetIdx++;
			}
			else if (comparison > 0) {
				targetIdx++;
			}
			else {
				return false;
			}
		}

		return thisIdx == this._keys.Length;
	}

	public bool Equals(PropertyStateKey other) {
		if (this._hashCode != other._hashCode) return false;
		if (this._keys.Length != other._keys.Length) return false;

		for (int i = 0; i < this._keys.Length; i++) {
			if (this._keys[i] != other._keys[i] || this._values[i] != other._values[i]) {
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
		if (this._keys == null || this._keys.Length == 0) {
			return "";
		}
		return string.Join(",", this._keys.Zip(this._values, (k, v) => $"{k}={v}"));
	}
}
