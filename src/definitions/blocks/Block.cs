using System.Runtime.CompilerServices;

namespace Moincroft.Definitions.Blocks;

public class PropertyState {
	private readonly int[] _valueIndices;
	private readonly IProperty[] _properties;
	public PropertyStateKey PropertyKey { get; }

	public PropertyState(IProperty[] properties, int[] valueIndices, PropertyStateKey key) {
		this._properties = properties;
		this._valueIndices = valueIndices;
		this.PropertyKey = key;
	}

	public T Get<T>(Property<T> property) where T : notnull {
		int propIndex = Array.IndexOf(this._properties, property);
		int valIndex = this._valueIndices[propIndex];
		return (T) property.GetValueAt(valIndex);
	}
}

public class Block {
	private readonly PropertyState[] _idToState;
	private readonly int[] _propertyStrides;

	public virtual IProperty[] Properties => [];

	public readonly BlockData data;

	public Block(BlockData data) {
		this.data = data;
		this._propertyStrides = new int[this.Properties.Length];

		List<int[]> combinations = GenerateCombinations(this.Properties);
		this._idToState = new PropertyState[combinations.Count];

		int stride = 1;
		for (int i = this.Properties.Length - 1; i >= 0; i--) {
			this._propertyStrides[i] = stride;
			stride *= this.Properties[i].ValueCount;
		}

		for (int i = 0; i < combinations.Count; i++) {
			Dictionary<string, string> v = [];
			for (int j = 0; j < this.Properties.Length; j++) {
				v[this.Properties[j].Name] = this.Properties[j].GetValueAt(combinations[i][j]).ToString()!.ToLower();
			}
			PropertyStateKey key = new PropertyStateKey(v);
			this._idToState[i] = new PropertyState(this.Properties, combinations[i], key);
		}
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public PropertyState GetState(BlockStateId id) => this._idToState[id];

	public BlockStateId ModifyState(BlockStateId currentId, IProperty property, int newValueIndex) {
		int propIndex = Array.IndexOf(this.Properties, property);
		PropertyState currentState = this._idToState[currentId];
		int currentValueIndex = property.GetIndexOf(currentState.Get((dynamic) property));

		int diff = (newValueIndex - currentValueIndex) * this._propertyStrides[propIndex];
		return (BlockStateId) (currentId + diff);
	}

	private static List<int[]> GenerateCombinations(IProperty[] props) {
		List<int[]> result = [];
		GenerateRecursive(props, 0, new int[props.Length], result);
		return result;
	}

	private static void GenerateRecursive(IProperty[] props, int depth, int[] current, List<int[]> result) {
		if (depth == props.Length) {
			result.Add((int[]) current.Clone());
			return;
		}
		for (int i = 0; i < props[depth].ValueCount; i++) {
			current[depth] = i;
			GenerateRecursive(props, depth + 1, current, result);
		}
	}
}