using Moincroft.Definitions.Models;
using Stride.Core;

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
	public abstract BlockStateItem[] GetModels(BlockType type, BlockPos pos);

	protected BlockStateItem FetchFrom(BlockStateItem[] items, BlockPos pos) {
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
}

public class VariantBlockStateData : BlockStateData {
	public Dictionary<PropertyStateKey, BlockStateItem[]> Variants { get; } = [];
	public BlockStateItem[]? DefaultVariant;

	public override BlockStateItem[] GetModels(BlockType type, BlockPos pos) {
		Block block = BlockRegistry.GetBlock(type.Type);
		PropertyStateKey key = type.State.PropertyKey;

		var matchingVariant = this.Variants
			.FirstOrDefault(kvp => kvp.Key.Matches(type.State.PropertyKey));

		if (matchingVariant.Value != null)
			return [ this.FetchFrom(matchingVariant.Value, pos) ];

		if (this.DefaultVariant != null)
			return [ this.FetchFrom(this.DefaultVariant, pos) ];

		Console.WriteLine($"[\n{string.Join(",\n", this.Variants.Select(kvp => $"\t\"{kvp.Key}\": {kvp.Value}    (== {kvp.Key == key})"))}\n]");
		throw new Exception($"Missing blockstate model variant for {block.data.Id} at \"{key}\"");
	}
}

public class MultiPartBlockStateData : BlockStateData {
	public readonly List<Part> parts = [];

	private static bool ConditionApplies(Dictionary<string, string[]> condition, BlockType type) {
		Property[] properties = BlockRegistry.GetBlock(type.Type).Properties;

		foreach (KeyValuePair<string, string[]> item in condition) {
			Property property = properties.First(x => x.Name == item.Key);

			if (!item.Value.Contains(type.State.Get(property).ToString()!.ToLower()))
				return false;
		}

		return true;
	}

	public override BlockStateItem[] GetModels(BlockType type, BlockPos pos) {
		List<BlockStateItem> models = [];

		foreach (Part part in this.parts) {
			if (part.conditionType == Part.ConditionType.And) {
				if (!part.conditions.All(c => ConditionApplies(c, type)))
					continue;
			}
			else {
				if (!part.conditions.Any(c => ConditionApplies(c, type)))
					continue;
			}

			models.Add(this.FetchFrom(part.apply, pos));
		}

		return [ ..models ];
	}

	public readonly struct Part {
		public readonly BlockStateItem[] apply;
		public readonly Dictionary<string, string[]>[] conditions;
		public readonly ConditionType conditionType;

		public Part(BlockStateItem[] apply, Dictionary<string, string[]>[] conditions, ConditionType conditionType) {
			this.apply = apply;
			this.conditions = conditions;
			this.conditionType = conditionType;
		}

		public enum ConditionType {
			And,
			Or,
		}
	}
}