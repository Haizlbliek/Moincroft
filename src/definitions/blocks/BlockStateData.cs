using Moincroft.Definitions.Models;

namespace Moincroft.Definitions;

public class BlockStateItem {
	public required Model model;
	public int rotationX = 0;
	public int rotationY = 0;
	public int rotationZ = 0;
	public bool UVLock = false;
	public int weight = 1;
}

public abstract class BlockStateData {
	public abstract BlockStateItem GetModel(BlockType type);
}

public class VariantBlockStateData : BlockStateData {
	public Dictionary<PropertyStateKey, BlockStateItem[]> Variants { get; } = [];

	private BlockStateItem FetchFrom(BlockStateItem[] items) {
		return items[0]; // TODO
	}

	public override BlockStateItem GetModel(BlockType type) {
		Block block = BlockRegistry.GetBlock(type.Type);
		PropertyStateKey key = type.State.PropertyKey;

		if (this.Variants.TryGetValue(key, out BlockStateItem[]? items))
			return this.FetchFrom(items);

		Console.WriteLine($"[\n{string.Join(",\n", this.Variants.Select(kvp => $"\t\"{kvp.Key}\": {kvp.Value}    (== {kvp.Key == key})"))}\n]");
		throw new Exception($"Missing blockstate model variant for {block.data.Id} at \"{key}\"");
	}
}

// TODO: Multipart