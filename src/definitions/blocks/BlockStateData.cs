using System.Security.Cryptography;
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
	public abstract BlockStateItem GetModel(BlockType type, BlockPos pos);
}

public class VariantBlockStateData : BlockStateData {
	public Dictionary<PropertyStateKey, BlockStateItem[]> Variants { get; } = [];
	public BlockStateItem[]? DefaultVariant;

	private BlockStateItem FetchFrom(BlockStateItem[] items, BlockPos pos) {
		if (items.Length == 1) return items[0];
		Random random = pos.GetSeededRandom();

		int totalWeight = 0;
		for (int i = 0; i < items.Length; i++) {
			totalWeight += items[i].weight;
		}

		int roll = random.Next(totalWeight);
		int currentWeightSum = 0;

		for (int i = 0; i < items.Length; i++) {
			currentWeightSum += items[i].weight;
			if (roll < currentWeightSum) {
				return items[i];
			}
		}

		return items[0];
	}

	public override BlockStateItem GetModel(BlockType type, BlockPos pos) {
		Block block = BlockRegistry.GetBlock(type.Type);
		PropertyStateKey key = type.State.PropertyKey;

		if (this.Variants.TryGetValue(key, out BlockStateItem[]? items))
			return this.FetchFrom(items, pos);

		if (this.DefaultVariant != null)
			return this.FetchFrom(this.DefaultVariant, pos);

		Console.WriteLine($"[\n{string.Join(",\n", this.Variants.Select(kvp => $"\t\"{kvp.Key}\": {kvp.Value}    (== {kvp.Key == key})"))}\n]");
		throw new Exception($"Missing blockstate model variant for {block.data.Id} at \"{key}\"");
	}
}

// TODO: Multipart